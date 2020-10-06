
webRtcInterop = new (function () {
    var jsObjectRefs = {};
    var jsObjectRefId = 0;
    var self = this;

    const jsRefKey = '__jsObjectRefId'; // Keep in sync with ElementRef.cs

    // reviver will help me store js object ref on .net like the .net do with elementreference or dotnetobjectreference
    this.jsObjectRefRevive = function (key, value) {
        if (value &&
            typeof value === 'object' &&
            value.hasOwnProperty(jsRefKey) &&
            typeof value[jsRefKey] === 'number') {

            var id = value[jsRefKey];
            if (!(id in jsObjectRefs)) {
                throw new Error("This JS object reference does not exists : " + id);
            }
            return jsObjectRefs[id];
        } else {
            return value;
        }
    };
    // this simple method will be used for getting the content of a given js object ref because js interop 
    // will call the reviver with the given C# js object ref
    this.returnInstance = function (instance, serializationSpec) {
        return self.getSerializableObject(instance, [], serializationSpec);
    }
    DotNet.attachReviver(this.jsObjectRefRevive);
    // this reviver change a given parameter to a method, it's usefull for sending .net callback to js
    DotNet.attachReviver(function (key, value) {
        if (value &&
            typeof value === 'object' &&
            value.hasOwnProperty("__isCallBackWrapper")) {

            var netObjectRef = value.callbackRef;

            return function () {
                var args = [];
                if (!value.getJsObjectRef) {
                    for (let index = 0; index < arguments.length; index++) {
                        const element = arguments[index];
                        args.push(self.getSerializableObject(element, [], value.serializationSpec));
                    }
                } else {
                    for (let index = 0; index < arguments.length; index++) {
                        const element = arguments[index];
                        args.push(self.storeObjectRef(element));
                    }
                }
                return netObjectRef.invokeMethodAsync('Invoke', ...args);
            };
        } else {
            return value;
        }
    });
    var eventListenersIdCurrent = 0;
    var eventListeners = {};
    this.addEventListener = function (instance, propertyPath, eventName, callback) {
        var target = self.getInstanceProperty(instance, propertyPath);
        target.addEventListener(eventName, callback);
        var eventId = eventListenersIdCurrent++;
        eventListeners[eventId] = callback;
        return eventId;
    };
    this.removeEventListener = function (instance, propertyPath, eventName, eventListenersId) {
        var target = self.getInstanceProperty(instance, propertyPath);
        target.removeEventListener(eventName, eventListeners[eventListenersId]);
        delete eventListeners[eventListenersId];
    };
    this.getProperty = function (propertyPath) {
        return self.getInstanceProperty(window, propertyPath);
    };
    this.hasProperty = function (instance, propertyPath) {
        return self.getInstanceProperty(instance, propertyPath) !== null;
    };
    this.getPropertyRef = function (propertyPath) {
        return self.getInstancePropertyRef(window, propertyPath);
    };
    this.getInstancePropertyRef = function (instance, propertyPath) {
        return self.storeObjectRef(self.getInstanceProperty(instance, propertyPath));
    };
    this.storeObjectRef = function (obj) {
        var id = jsObjectRefId++;
        jsObjectRefs[id] = obj;
        var jsRef = {};
        jsRef[jsRefKey] = id;
        return jsRef;
    }
    this.removeObjectRef = function (id) {
        delete jsObjectRefs[id];
    }
    function getPropertyList(path) {
        var res = path.replace('[', '.').replace(']', '').split('.');
        if (res[0] === "") { // if we pass "[0].id" we want to return [0,'id']
            res.shift();
        }
        return res;
    }
    this.getInstanceProperty = function (instance, propertyPath) {
        if (propertyPath === '') {
            return instance;
        }
        var currentProperty = instance;
        var splitProperty = getPropertyList(propertyPath);

        for (i = 0; i < splitProperty.length; i++) {
            if (splitProperty[i] in currentProperty) {
                currentProperty = currentProperty[splitProperty[i]];
            } else {
                return null;
            }
        }
        return currentProperty;
    };
    this.setInstanceProperty = function (instance, propertyPath, value) {
        var currentProperty = instance;
        var splitProperty = getPropertyList(propertyPath);
        for (i = 0; i < splitProperty.length; i++) {
            if (splitProperty[i] in currentProperty) {
                if (i === splitProperty.length - 1) {
                    currentProperty[splitProperty[i]] = value;
                    return;
                } else {
                    currentProperty = currentProperty[splitProperty[i]];
                }
            } else {
                return;
            }
        }
    };
    this.getInstancePropertySerializable = function (instance, propertyName, serializationSpec) {
        var data = self.getInstanceProperty(instance, propertyName);
        if (data instanceof Promise) {// needed when some properties like beforeinstallevent.userChoice are promise
            return data;
        }
        var res = self.getSerializableObject(data, [], serializationSpec);
        return res;
    };
    this.callInstanceMethod = function (instance, methodPath, ...args) {
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
        var method = self.getInstanceProperty(instance, methodPath);
        return method.apply(instance, args);
    };
    this.callInstanceMethodGetRef = function (instance, methodPath, ...args) {
        return this.storeObjectRef(this.callInstanceMethod(instance, methodPath, ...args));
    };
    this.getSerializableObject = function (data, alreadySerialized, serializationSpec) {
        if (serializationSpec === false) {
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
        var res = (Array.isArray(data)) ? [] : {};
        if (!serializationSpec) {
            serializationSpec = "*";
        }
        for (var i in data) {
            var currentMember = data[i];

            if (typeof currentMember === 'function' || currentMember === null) {
                continue;
            }
            var currentMemberSpec;
            if (serializationSpec != "*") {
                currentMemberSpec = Array.isArray(data) ? serializationSpec : serializationSpec[i];
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
                    res[i] = [];
                    for (var j = 0; j < currentMember.length; j++) {
                        const arrayItem = currentMember[j];
                        if (typeof arrayItem === 'object') {
                            res[i].push(self.getSerializableObject(arrayItem, alreadySerialized, currentMemberSpec));
                        } else {
                            res[i].push(arrayItem);
                        }
                    }
                } else {
                    // the browser provides some member (like plugins) as hash with index as key, 
                    // if length == 0 we shall not convert it
                    if (currentMember.length === 0) {
                        res[i] = [];
                    } else {
                        res[i] = self.getSerializableObject(currentMember, alreadySerialized, currentMemberSpec);
                    }
                }


            } else {
                // string, number or boolean
                if (currentMember === Infinity) { //inifity is not serialized by JSON.stringify
                    currentMember = "Infinity";
                }
                if (currentMember !== null) { //needed because the default json serializer in jsinterop serialize null values
                    res[i] = currentMember;
                }
            }
        }
        return res;
    };
    this.navigator = new (function () {
        this.geolocation = new (function () {
            this.getCurrentPosition = function (options) {
                return new Promise(function (resolve) {
                    navigator.geolocation.getCurrentPosition(
                        position => resolve({ location: self.getSerializableObject(position) }),
                        error => resolve({ error: self.getSerializableObject(error) }),
                        options)
                });
            };
            this.watchPosition = function (options, wrapper) {
                return navigator.geolocation.watchPosition(
                    position => {
                        const result = { location: self.getSerializableObject(position) };
                        return wrapper.invokeMethodAsync('Invoke', result);
                    },
                    error => wrapper.invokeMethodAsync('Invoke', { error: self.getSerializableObject(error) }),
                    options
                );
            };
        })();
        this.getBattery = function () {
            return new Promise(function (resolve, reject) {
                if (navigator.battery) {//some browser does not implement getBattery but battery instead see https://developer.mozilla.org/en-US/docs/Web/API/Navigator/battery
                    var res = self.getSerializableObject(navigator.battery);
                    resolve(res);
                }
                else if ('getBattery' in navigator) {
                    navigator.getBattery().then(
                        function (battery) {
                            var res = self.getSerializableObject(battery);
                            resolve(res);
                        }
                    );
                }
                else {
                    resolve(null);
                }
            });
        }
    })();
})();