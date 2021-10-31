const Logger = require('./Logger');
const EnhancedEventEmitter = require('./EnhancedEventEmitter');
const Message = require('./Message');

const logger = new Logger('Peer');

class Peer extends EnhancedEventEmitter
{
	/**
	 * @param {String} peerId
	 * @param {protoo.Transport} transport
	 *
	 * @emits close
	 * @emits {request: protoo.Request, accept: Function, reject: Function} request
	 * @emits {notification: protoo.Notification} notification
	 */
	constructor(peerId, transport)
	{
		super(logger);

		logger.debug('constructor()');

		// Closed flag.
		// @type {Boolean}
		this._closed = false;

		// Peer id.
		// @type {String}
		this._id = peerId;

		// Transport.
		// @type {protoo.Transport}
		this._transport = transport;

		// Custom data object.
		// // @type {Object}
		this._data = {};

		// Map of pending sent request objects indexed by request id.
		// @type {Map<Number, Object>}
		this._sents = new Map();

		// Handle transport.
		this._handleTransport();
	}

	/**
	 * Peer id
	 *
	 * @returns {String}
	 */
	get id()
	{
		return this._id;
	}

	/**
	 * Whether the Peer is closed.
	 *
	 * @returns {Boolean}
	 */
	get closed()
	{
		return this._closed;
	}

	/**
	 * App custom data.
	 *
	 * @returns {Object}
	 */
	get data()
	{
		return this._data;
	}

	/**
	 * Invalid setter.
	 */
	set data(data) // eslint-disable-line no-unused-vars
	{
		throw new Error('cannot override data object');
	}

	/**
	 * Close this Peer and its Transport.
	 */
	close()
	{
		if (this._closed)
			return;

		logger.debug('close()');

		this._closed = true;

		// Close Transport.
		this._transport.close();

		// Close every pending sent.
		for (const sent of this._sents.values())
		{
			sent.close();
		}

		// Emit 'close' event.
		this.safeEmit('close');
	}

	/**
	 * Send a protoo request to the remote Peer.
	 *
	 * @param {String} method
	 * @param {Object} [data]
	 *
	 * @async
	 * @returns {Object} The response data Object if a success response is received.
	 */
	async request(method, data = undefined)
	{
		const request = Message.createRequest(method, data);

		console.log('*************** OUT REQUEST: %s', JSON.stringify(request));

		this._logger.debug('request() [method:%s, id:%s]', method, request.id);

		// This may throw.
		await this._transport.send(request);

		return new Promise((pResolve, pReject) =>
		{
			const timeout = 2000 * (15 + (0.1 * this._sents.size));
			const sent =
			{
				id      : request.id,
				method  : request.method,
				resolve : (data2) =>
				{
					if (!this._sents.delete(request.id))
						return;

					clearTimeout(sent.timer);
					pResolve(data2);
				},
				reject : (error) =>
				{
					if (!this._sents.delete(request.id))
						return;

					clearTimeout(sent.timer);
					pReject(error);
				},
				timer : setTimeout(() =>
				{
					if (!this._sents.delete(request.id))
						return;

					pReject(new Error('request timeout'));
				}, timeout),
				close : () =>
				{
					clearTimeout(sent.timer);
					pReject(new Error('peer closed'));
				}
			};

			// Add sent stuff to the map.
			this._sents.set(request.id, sent);
		});
	}

	/**
	 * Send a protoo notification to the remote Peer.
	 *
	 * @param {String} method
	 * @param {Object} [data]
	 *
	 * @async
	 */
	async notify(method, data = undefined)
	{

		const notification = Message.createNotification(method, data);
		console.log('================ OUT NOTIFICATION: %s', JSON.stringify(notification));


		this._logger.debug('notify() [method:%s]', method);

		// This may throw.
		await this._transport.send(notification);
	}

	_handleTransport()
	{
		if (this._transport.closed)
		{
			this._closed = true;

			setImmediate(() => this.safeEmit('close'));

			return;
		}

		this._transport.on('close', () =>
		{
			if (this._closed)
				return;

			this._closed = true;

			this.safeEmit('close');
		});

		this._transport.on('message', (message) =>
		{
			if (message.request)
				this._handleRequest(message);
			else if (message.response)
				this._handleResponse(message);
			else if (message.notification)
				this._handleNotification(message);
		});
	}

	_handleRequest(request)
	{

		console.log('################ IN REQUEST: %s', JSON.stringify(request));

		try
		{
			this.emit('request',
				// Request.
				request,
				// accept() function.
				(data) =>
				{
					const response = Message.createSuccessResponse(request, data);

					console.log('################ IN RESPONSE: %s', JSON.stringify(response));
					this._transport.send(response)
						.catch(() => {});
				},
				// reject() function.
				(errorCode, errorReason) =>
				{
					if (errorCode instanceof Error)
					{
						errorReason = errorCode.message;
						errorCode = 500;
					}
					else if (typeof errorCode === 'number' && errorReason instanceof Error)
					{
						errorReason = errorReason.message;
					}

					const response =
						Message.createErrorResponse(request, errorCode, errorReason);
						console.log('################ IN RESPONSE: %s', JSON.stringify(response));

						this._transport.send(response)
						.catch(() => {});
				});
		}
		catch (error)
		{
			const response = Message.createErrorResponse(request, 500, String(error));

			this._transport.send(response)
				.catch(() => {});
		}
	}

	_handleResponse(response)
	{
		console.log('*************** OUT RESPONSE: %s', JSON.stringify(response));

		const sent = this._sents.get(response.id);

		if (!sent)
		{
			logger.error(
				'received response does not match any sent request [id:%s]', response.id);

			return;
		}


		if (response.ok)
		{
			sent.resolve(response.data);
		}
		else
		{
			const error = new Error(response.errorReason);

			error.code = response.errorCode;
			sent.reject(error);
		}
	}

	_handleNotification(notification)
	{
		console.log('================ IN NOTIFICATION: %s', JSON.stringify(notification));
		this.safeEmit('notification', notification);
	}
}

module.exports = Peer;
