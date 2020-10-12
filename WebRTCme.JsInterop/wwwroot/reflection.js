reflection = (function () {

    let public = {};


    /**
     * Create a new object, add the object to 'objectRefs' and return JS object reference.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} interface: Interface(class) name to be created.
     * @param {...any} args: Argument list of the constructor.
     */
    public.create = function (parent, interface, ...args) {
    }

    /**
     * Delete the specified JS object reference from 'objectRefs'.
     * 
     * @param {number} id
     */
    public.delete = function (id) {
    }

    /**
     * Gets specified property. If property is 'object' type and 'contentSpec' is 'null', it adds the object 
     * to the 'objectRefs' and returns the JS object reference. If 'contentSpec' is provided then the content
     * specified in 'contentSpec' will be returned.
     * If property is a primitive type, it will be returned verbatim. 
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be 
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to get. If 'null', parent object will be returned.
     * @param {string} contentSpec: Filter of the content to be returned. 'null' indicates that JS object reference
     *                              shall be returned if property specifies an 'object'
     */
    public.get = function (parent, property, contentSpec) {

    }

    /**
     * Sets specified property.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to set. If 'null', parent object will be set.
     * @param {any} value: Object to set. It can be JS object reference or a string. JS object reference will be
     *                     converted into a JS object by the reviver. If it is a string, it will be converted into
     *                     JS object first.
     */
    public.set = function (parent, property, value) {

    }

    /**
     * Calls a method synchronously. If return value is object type, it adds the object to 'objectRefs' and 
     * returns JS object reference. Otherwise the primitive type is returned.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} method: String specifying the method to be called.
     * @param {...any} args: Argument list of the method.
     */
    public.call = function (parent, method, ...args) {
    }


    /**
     * Calls a method asynchronously. it waits for the promise to be completed. If return value is object type, 
     * it adds the object to 'objectRefs' and returna JS object reference. Otherwise the primitive type is returned.
     *
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} method: String specifying the method to be called.
     * @param {...any} args: Argument list of the method.
     */
    public.callAsync = async function (parent, method, ...args) {
    }

    /**
     * Adds a new event listener. .NET callback will be invoken on JS event. It returns an id as event reference. 
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to set. If 'null', parent object will be used.
     * @param {string} event: String indicating the event name.
     * @param {any} callback: .NET callback handler. 
     */
    public.addEventListener = function (parent, property, event, callback) {
    };

    /**
     *  Removes the specifed event listener.
     *  
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to set. If 'null', parent object will be used.
     * @param {string} event: String indicating the event name.
     * @param {int} id: Event id.
     */
    public.removeEventListener = function (parent, property, event, id) {
    }


})