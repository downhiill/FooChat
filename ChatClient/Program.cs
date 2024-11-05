using ChatClient;

var client = new FooChatClient();

client.Connected += () => Console.WriteLine("Подключение успешно! Можете отправлять сообщения.");
client.MessageReceived += message => Console.WriteLine(message);
client.Disconnected += () =>
{
    Console.WriteLine("Соединение разорвано. Нажмите любую клавишу для выхода...");
    Environment.Exit(0); // Завершение работы при разрыве соединения
};

Console.Write("Введите свое имя: ");
string userName = Console.ReadLine();

await client.ConnectAsync(userName);

while (true)
{
    string message = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(message)) continue;

    try
    {
        await client.SendMessageAsync(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
        break;
    }
}

// Отключаемся при завершении
await client.Disconnect();