using ChatClient;



    Console.WriteLine("Введите своей имя:");
    string userName = Console.ReadLine();

    var client = new FooChatClient();
    await client.ConnectAsync(userName);
