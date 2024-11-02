using FooChat;


    int port = 5000;
    var server = new ChatServer(port);
    await server.StartAsync();

