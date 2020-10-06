class Person {
    constructor(firstName, lastName) {
        this.firstName = firstName;
        this.lastName = lastName;
    }
    get fullName() {
        return `${this.firstName} ${this.lastName}`;
    }
};

let args = ['John', 'Doe'];

let john = Reflect.construct(
    Person,
    args
);

console.log(john instanceof Person);
console.log(john.fullName);

let result = Reflect.apply(Math.max, Math, [10, 20, 30]);
console.log(result);

let person = {
    name: 'John Doe'
};

if (Reflect.defineProperty(person, 'age', {
        writable: true,
        configurable: true,
        enumerable: false,
        value: 25,
    })) {
    console.log(person.age);
} else {
    console.log('Cannot define the age property on the person object.');

}

const json = '{"result":true, "inner": {"count":42}}';
const obj = JSON.parse(json,(k,v) => {console.log(k,v); return v;} );

localConnection = new RTCPeerConnection();

var constraints = window.constraints = {
    audio: false,
    video: true
  };
  var md = window.navigator.mediaDevices.getUserMedia(constraints);


x = new window.RTCPeerConnection();

console.log("hello");