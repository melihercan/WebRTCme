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
     * Gets specified property.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be 
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to get. If 'null', parent object will be returned.
     * @param {boolean} isObjectRef: If true, JS object refence will be returned. If false, JS object will be returned.
     *                               'contentSpec' if not null, will specify the filter of what parameters shall be
     *                               returned.
     * @param {string} contentSpec: If 'isObjectRef' is false and 'contentSpec' is not 'null', parameters specified in 
     *                              the spec will be returned. If 'contentSpec' is null then JS object will be returned.
     */
    public.get = function (parent, property, isObjectRef, contentSpec) {

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
     * returna JS object reference. Otherwise the primitive type is returned.
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
     * Adds a new event listener. .NET callback will be invoken on JS event.
     * 
     * @param {any} parent
     * @param {any} property
     * @param {any} event
     * @param {any} callback
     */
    public.addEventListener = function (parent, property, event, callback) {
    };

    /**
     *  Removes the specifed event listener.
     *  
     * @param {any} parent
     * @param {any} property
     * @param {any} event
     * @param {any} id
     */
    public.removeEventListener = function (parent, property, event, id) {
    }


})