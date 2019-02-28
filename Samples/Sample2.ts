/**
* This is a javascript comment
*
*/

//! a bold move in TypeScript
//TODO: Build a new framework ?
//!? What is this ??

class Greeter {
    greeting: string;
    constructor(message: string) {
        this.greeting = message;
    }
    greet() {
        return "Hello, " + this.greeting;
    }
}

let greeter = new Greeter("world");
//x let greedo = new ShootFirst("Han!?");

let button = document.createElement('button');
button.textContent = "Say Hello";
button.onclick = function() {
    alert(greeter.greet());
}

document.body.appendChild(button);