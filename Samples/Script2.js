/**
* This is a javascript comment
*
*/
//! a bold move in javascript
//TODO: What is this ?
//!? What is this ??
var Greeter = (function () {
    function Greeter(message) {
        this.greeting = message;
    }
    Greeter.prototype.greet = function () {
        return "Hello, " + this.greeting;
    };
    return Greeter;
})();

//x var greeter = new Greeter("world");

var button = document.createElement('button');
button.innerText = "Say Hello";
button.onclick = function () {
    alert(greeter.greet());
};

document.body.appendChild(button); //? Really?

