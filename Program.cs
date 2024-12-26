using Microsoft.SemanticKernel.ChatCompletion;
using DotnetChatbot.Helpers;
using Microsoft.Extensions.Configuration;
using DotnetChatbot.Services;


var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
            .Build();

Config.Initialize(configuration);

BasicChat chat = new BasicChat();

chat.init();


Console.WriteLine("Let's try this cool chat, when you get bored, just press ENTER to exit");

ChatHistory chatHistory = [];
string? input = null;
while (true)
{
    Console.Write("\nUser > ");
    input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        // Leaves if the user hit enter without typing any word
        break;
    }
    chatHistory.AddUserMessage(input);
    var chatResult = await chat.GetMessageAsync(chatHistory);
    foreach (var message in chatResult)
        Console.Write($"\nAssistant > {message}\n");
}

