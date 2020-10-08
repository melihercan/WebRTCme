// Use same value on .NET side.
const jsObjectRefKey = '__jsObjectRefId';

var jsObjectRefs = {};
var jsObjectRefId = 0;

DotNet.attachReviver(function (key, value) {
    if (value && typeof value === 'object' && value.hasOwnProperty(jsObjectRefKey) &&
        typeof value[jsObjectRefKey] === 'number') {
        let id = value[jsObjectRefKey];
        if (!(id in jsObjectRefs)) {
            throw new Error("JS object reference with id:" + id + " does not exist");
        }
        return jsObjectRefs[id];
    } else {
        return value;
    }
})

window.webRtcInterop = {
    initialize: function (msg) {
        console.log(msg);
    }
}
