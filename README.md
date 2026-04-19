# ChatApp

## Overview

ChatApp is a web application based on **ASP .NET** for the backend and **Vue.js** for the frontend.
It uses SignalR to enable instant communication between connected users and a Web API.

## Core Features

- Real time messaging between two users
- Chat history retrieval
- Online and offline user status updates
- Typing indicator
- Server side message validation

## Technologies

- Backend; ASP .NET Core Web API and SignalR
- Frontend: vue.js and TypeScript

## How to run

### Backend

To be able to run the Backend we need to do The Flow:

1. Navigate to the solution Folder:

    ```sh
        cd ChatApp\ChatAppApi
    ```

1. To build the Project run command:

    ```sh
        dotnet build
    ```

1. To run Test

    ```sh
        dotnet test
    ```

1. To run the Server run the command

    ```sh
        cd ChatAppApi
        dotnet run
    ```

### Frontend

To be able to run the Frontend we need to do The Flow:

1. Navigate to the Frontend project Folder:

    ```sh
        cd ChatAppUi
    ```

1. To install the Project packages:

    ```sh
        npm install
    ```

1. To run the Project:

    ```sh
        npm run dev
    ```

Visit <http://localhost:5176/> to view the application.

**Please Note** that the Backend and the frontend are running on **different** Ports:

- Backend: `5001` and `7007`
- Frontend: `5176`

## Message Format

The message format contains five properties defined in **ChatAppApi\Models\Message.cs** as follows:

| Format       | Description                                                                                                                   |
| :----------- | :---------------------------------------------------------------------------------------------------------------------------- |
| `Type`       | The Type property contains the type of the message, like "chat," "typing," or "error."                                        |
| `SenderId`   | The SenderId property contains the `UserId` of the message sender  and identifies the user.                                   |
| `ReceiverId` | The ReceiverId property contains the `UserId` of the message receiver, which is linked with the SenderId to send the message. |
| `Timestamp`  | The Timestamp property contains `DateTime` of the message.                                                                    |
| `Data`       | The Data property contains the message content.                                                                               |

Example:

```JSON
{
    "Type": "chat",
    "SenderId": "62fc6f0e-712b-4de7-a8c9-a338c9b67728",
    "ReceiverId":"cd54e6ef-c4d3-4bb1-a764-8a0b4a334ff8",
    "Timestamp": "2026-04-19T17:28:02.6303608+02:00",
    "Data": "Mahlzeit"
}
```

## Real Time Flow

When a user connects to the chat hub via SignalR, the server updates and broadcasts a list of connected users.
When a message is sent through the API, SignalR instantly delivers it to the recipients in real time. At the same time, typing activity is shared, allowing users to see when someone is typing.

![SequenceDiagram](/assets/SequenceDiagram.png)  

## What i would improve next

- Add authentication and authorization
- Add persistence Database
- Add ability to delete Messages after sending it
- Add ability to personal profile
- Add End 2 End UI Tests
- Support group chat
- Code improvements
- UI/UX adjustments
