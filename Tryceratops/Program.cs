using System;
using System.Threading;
using System.Text;
using Tryceratops;
using System.Net.WebSockets;
using Microsoft.Extensions.Configuration;

//See https://aka.ms/new-console-template for more information
IConfigurationRoot configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string twitchOauth = configuration["Twitch:Oauth"];


ClientWebSocket clientWebSocket = new ClientWebSocket();
await clientWebSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), CancellationToken.None);

await clientWebSocket.SendStringMessageAsync("CAP REQ :twitch.tv/membership twitch.tv/tags twitch.tv/commands");
await clientWebSocket.SendStringMessageAsync($"PASS oauth:{twitchOauth}");
await clientWebSocket.SendStringMessageAsync("NICK FirePig09");
await clientWebSocket.SendStringMessageAsync("JOIN #firepig09");


Console.WriteLine("sent");

while (clientWebSocket.State == WebSocketState.Open)
{
    byte[] buffer = new byte[1024];
    var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    if (result.MessageType == WebSocketMessageType.Close)
    {
        await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        Console.WriteLine(result.CloseStatusDescription);
    }

    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

    //TODO: make all this an object

    TwitchMessage parsedMessage = new TwitchMessage(receivedMessage);

    Console.WriteLine(parsedMessage.Channel);



}

Console.WriteLine(clientWebSocket.State);
