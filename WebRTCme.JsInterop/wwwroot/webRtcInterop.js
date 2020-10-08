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

    public.getProperty = function (object, property) {
        if (object === null) {
            object = window;
        }
        let list = property.replace('[', '.').replace(']', '').split('.');
        if (list[0] === "") {
            list.shift();
        }
        let current = object;
        for (i = 0; i < list.length; i++) {
            if (list[i] in current) {
                current = current[list[i]];
            } else {
                return null;
            }
        }

        let ret = addJsObjectRef(current);
        return ret;
    }

    return public;

})();