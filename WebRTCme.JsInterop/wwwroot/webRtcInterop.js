webRtcInterop = (function () {

    let public = {};

    // Use same value on .NET side, "JsObjectRef.cs".
    const jsObjectRefKey = '__jsObjectRefId';

    let objectRefs = {};
    let objectRefId = 0;

    DotNet.attachReviver(function (key, value) {
        if (value && typeof value === 'object' && value.hasOwnProperty(jsObjectRefKey) &&
            typeof value[jsObjectRefKey] === 'number') {
            let id = value[jsObjectRefKey];
            if (!(id in objectRefs)) {
                throw new Error('JS object reference with id:' + id + ' does not exist');
            }
            return objectRefs[id];
        } else {
            return value;
        }
    })

    addObjectRef = function (object) {
        let id = objectRefId++;
        objectRefs[id] = object;
        let objectRef = {};
        objectRef[jsObjectRefKey] = id;
        return objectRef;
    }

    getPropertyObject = function (rootObject, property) {
        if (rootObject === null) {
            rootObject = window;
        }
        let list = property.replace('[', '.').replace(']', '').split('.');
        if (list[0] === "") {
            list.shift();
        }
        let object = rootObject;
        for (i = 0; i < list.length; i++) {
            if (list[i] in object) {
                object = object[list[i]];
            } else {
                throw new Error("Object referenced by " + property + " does not exist");
            }
        }
        return object;
    }

    getInterfaceObject = function (rootObject, interface) {
        if (rootObject === null) {
            rootObject = window;
        }
        let list = interface.replace('[', '.').replace(']', '').split('.');
        if (list[0] === "") {
            list.shift();
        }
        let object = rootObject;
        for (i = 0; i < list.length; i++) {
            if (list[i] in object) {
                object = object[list[i]];
            } else {
                throw new Error("Object referenced by " + interface + " does not exist");
            }
        }
        return object;
    }


    getMethodObject = function (rootObject, method) {
        if (method.includes(".")) {
            let property = method.substring(0, method.lastIndexOf('.'));
            rootObject = getPropertyObject(rootObject, property);
            method = method.substring(method.lastIndexOf('.') + 1);
        }
        let object = getPropertyObject(rootObject, method);
        return object;
    }

    getObjectContent = function (data, alreadySerialized, contentSpec) {
        if (contentSpec === false) {
            return undefined;
        }
        if (!alreadySerialized) {
            alreadySerialized = [];
        }
        if (typeof data == "undefined" ||
            data === null) {
            return null;
        }
        if (typeof data === "number" ||
            typeof data === "string" ||
            typeof data == "boolean") {
            return data;
        }
        var content = (Array.isArray(data)) ? [] : {};
        if (!contentSpec) {
            contentSpec = "*";
        }
        for (var i in data) {
            var currentMember = data[i];

            if (typeof currentMember === 'function' || currentMember === null) {
                continue;
            }
            var currentMemberSpec;
            if (contentSpec != "*") {
                currentMemberSpec = Array.isArray(data) ? contentSpec : contentSpec[i];
                if (!currentMemberSpec) {
                    continue;
                }
            } else {
                currentMemberSpec = "*"
            }
            if (typeof currentMember === 'object') {
                if (alreadySerialized.indexOf(currentMember) >= 0) {
                    continue;
                }
                alreadySerialized.push(currentMember);
                if (Array.isArray(currentMember) || currentMember.length) {
                    content[i] = [];
                    for (var j = 0; j < currentMember.length; j++) {
                        const arrayItem = currentMember[j];
                        if (typeof arrayItem === 'object') {
                            content[i].push(self.getSerializableObject(arrayItem, alreadySerialized, currentMemberSpec));
                        } else {
                            content[i].push(arrayItem);
                        }
                    }
                } else {
                    // the browser provides some member (like plugins) as hash with index as key, 
                    // if length == 0 we shall not convert it
                    if (currentMember.length === 0) {
                        content[i] = [];
                    } else {
                        content[i] = self.getSerializableObject(currentMember, alreadySerialized, currentMemberSpec);
                    }
                }


            } else {
                // string, number or boolean
                if (currentMember === Infinity) { //inifity is not serialized by JSON.stringify
                    currentMember = "Infinity";
                }
                if (currentMember !== null) { //needed because the default json serializer in jsinterop serialize null values
                    content[i] = currentMember;
                }
            }
        }
        return content;
    };


    ///////////////////// API

    // JS object ref
    public.createObject = function (rootObject, interface, ...args) {
        let interfaceObject = getInterfaceObject(rootObject, interface);
        let createdObject = new interfaceObject(args);
        let objectRef = addObjectRef(createdObject);
        return objectRef;
    }

    // void
    public.removeObject = function (id) {
        delete objectRefs[id];
    }

    // JS object ref
    public.getProperty = function (rootObject, property) {
        let propertyObject = getPropertyObject(rootObject, property);
        let objectRef = addObjectRef(propertyObject);
        return objectRef;
    }

    // JSON serialized content per spec
    public.getContent = function (rootObject, property, contentSpec) {
        let propertyObject = getPropertyObject(rootObject, property);
        let content = getObjectContent(propertyObject, [], contentSpec);
        return content;
    }

    // JS object ref
    public.callMethod = function (rootObject, method, ...args) {
        let methodObject = getMethodObject(rootObject, method);
        let ret = methodObject.apply(rootObject, args);
        if (ret != {}) {
            let objectRef = addObjectRef(ret);
            return objectRef;
        }
        return ret;
    }

    // JS object ref
    public.callMethodAsync = async function (rootObject, method, ...args) {
        let methodObject = getMethodObject(rootObject, method);
        let ret = await methodObject.apply(rootObject, args);
        if (ret != {}) {
            let objectRef = addObjectRef(ret);
            return objectRef;
        }
        return ret;
    }



    return public;

})();