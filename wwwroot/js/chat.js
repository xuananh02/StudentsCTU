"use strict";

const indicatorN = document.getElementById("indicator-notication");
const btnN = document.getElementById("btn-notification");
const sendChatForm = document.getElementById("send-chat-form");
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;
document.getElementById("messagesList").innerHTML = '';
indicatorN.classList.add("hidden");

btnN.addEventListener("click", () => {
    indicatorN.classList.add("hidden");
});

sendChatForm.addEventListener("submit", function (event) {
    event.preventDefault(); // Prevent the default form submission

    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;

    // Create a JavaScript object with the data
    var postData = {
        userId: user,
        message: message
    };

    var jsonData = JSON.stringify(postData);

    const urlPost = "/SendChat";

    const options = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: jsonData
    };


    fetch(urlPost, options)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json(); // Parse response body as JSON
        })
        .then(data => {
            console.log("Oke! " + data);
        });

    var divChat = document.createElement("li");
    var divBubble = document.createElement("div");
    document.getElementById("messagesList").appendChild(divChat);

    divChat.classList.add("chat", "chat-end");
    divBubble.classList.add("chat-bubble");

    divChat.appendChild(divBubble);

    divBubble.innerText = message;

});

connection.on("ReceiveMessageVer2", function (user, message) {
    var divChat = document.createElement("li");
    var divBubble = document.createElement("div");
    document.getElementById("messagesList").appendChild(divChat);

    divChat.classList.add("chat", "chat-start");
    divBubble.classList.add("chat-bubble");

    divChat.appendChild(divBubble);

    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    divBubble.innerText = message;
});

connection.on("NotificationPost", function (userId, postId, message) {
    indicatorN.classList.remove("hidden");
    
    const li = document.createElement("li");
    const div = document.createElement("div");
    const button = document.createElement("button");
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>`;

    li.classList.add("flex", "flex-col", "items-center", "border", "rounded-lg");
    div.classList.add("font-semibold");
    button.classList.add("btn", "btn-circle", "btn-outline", "w-10", "h-10");
    button.innerHTML = svg;
    div.innerText = message;

    button.addEventListener("click", () => {
        const url = "DeleteNotificationPost/" + userId + "-" + postId;

        const options = {
            method: 'DELETE', // Specify the HTTP method as DELETE
        };

        // Perform the fetch request
        fetch(url, options)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                console.log('Deleted successfully', data);
                li.remove();
            })
            .catch(error => {
                console.error('There was a problem with your fetch operation:', error);
            });

    });

    li.append(div, button);
    document.getElementById("notification-post").append(li);

});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});
