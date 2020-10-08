webRtcInterop = (function () {

    let public = {};

    // Use same value on .NET side.
    const jsObjectRefKey = '__jsObjectRefId';

    let jsObjectRefs = {};
    let jsObjectRefId = 0;

    DotNet.attachReviver(function (key, value) {
        if (value && typeof value === 'object' && value.hasOwnProperty(jsObjectRefKey) &&
            typeof value[jsObjectRefKey] === 'number') {
            let id = value[jsObjectRefKey];
            if (!(id in jsObjectRefs)) {
                throw new Error('JS object reference with id:' + id + ' does not exist');
            }
            return jsObjectRefs[id];
        } else {
            return value;
        }
    })

    addJsObjectRef = function (object) {
        let id = jsObjectRefId++;
        jsObjectRefs[id] = object;
        let jsObjectRef = {};
        jsObjectRef[jsObjectRefKey] = id;
        return jsObjectRef;
    }

    public.removeJsObjectRef = function (id) {
        delete jsObjectRefs[id];
    }

    getPropertyObject = function (instance, property) {
        if (instance === null) {
            instance = window;
        }
        let list = property.replace('[', '.').replace(']', '').split('.');
        if (list[0] === "") {
            list.shift();
        }
        let object = instance;
        for (i = 0; i < list.length; i++) {
            if (list[i] in object) {
                object = object[list[i]];
            } else {
                return null;
            }
        }
        return object;
    }

    public.getPropertyJsObjectRef = function (instance, property) {
        let object = getPropertyObject(instance, property);
        let objectRef = addJsObjectRef(object);
        return objectRef;
    }


    public.callMethodAsync = async function (instance, methodPath, ...args) {
        if (methodPath.indexOf('.') >= 0) {
            // if it's a method call on a child object we get this child object so the method call will happen in the 
            // context of the child object
            // some method like window.locaStorage.setItem  will throw an exception if the context is not expected
            var instancePath = methodPath.substring(0, methodPath.lastIndexOf('.'));
            instance = self.getInstanceProperty(instance, instancePath);
            methodPath = methodPath.substring(methodPath.lastIndexOf('.') + 1);
        }
        for (let index = 0; index < args.length; index++) {
            const element = args[index];
            // we change null value to undefined as there is no way to pass undefined value from C# and most of 
            // the browser API use undefined instead of null value for "no value"
            if (element === null) {
                args[index] = undefined;
            }
        }
        let method = getPropertyObject(instance, methodPath);
        let ret = await method.apply(instance, args);
        return ret;
    }


    return public;

})();