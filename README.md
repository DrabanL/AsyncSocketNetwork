# AsyncSocketNetwork
An Async (TPL) version of <code>SocketNetwork</code> project.

This async version is much more scaleable, using Async socket I/O intreduced in .net framework 4.6 which is the minimum required framework to use this library.

Offers generic simplified implementation of networking based on sockets. 

It handles all of the internal socket communications while allowing you to apply your own data serialization.

## Data Serialization and Deserialization
When data is received or sent on <code>SocketClient</code> object, the data is being serialized using the <code>SerializationHandler</code> property, which means you must implement <code>INetworkMessageSerializationHandler</code> to transform the data to the expected type.

## Server Side
Implementing server side requires of you to create a new instace of <code>SocketServer</code> or inherit from it in your own class, while calling <code>SocketServer.StartAsync()</code> to start processing connections.

Managing new incoming connections is done via <code>ServerHandler</code> property so make sure to implement and assign it to apply your own logics.

For in-depth look and example for a full server side implementation, you can take a look on the example project <code>Example.Server</code>.

## Client Side
Implementing client side requires of you to create a new instace of <code>SocketClient</code> or inherit from it in your own class, while calling <code>SocketClient.ConnectAsync()</code> to attemp to connect <code>SocketServer</code> instances.

Managing connection events or received data is done via <code>ClientHandler</code> property so make sure to implement and assign it to apply your own logics.

For in-depth look and example for a full client side implementation, you can take a look on the example project <code>Example.Client</code>.

## Example Project
In the example projects you can check our how implementations of a communication protocol and packets obfuscation, for the purpose of a simple Chatting application.