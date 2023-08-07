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
    Console.WriteLine(receivedMessage);

    //TODO: make all this an object

    int idx = 0;
    int endIdx;
    string? rawTagComponent = null;
    string? rawSourceComponent = null;
    string? rawParametersComponent = null;

    if (receivedMessage[idx] == '@')
    {
        endIdx = receivedMessage.IndexOf(' ');
        rawTagComponent = receivedMessage.Substring(idx, endIdx);
        idx = endIdx + 1;
    }

    if (receivedMessage[idx] == ':')
    {
        idx += 1;
        endIdx = receivedMessage.IndexOf(' ', idx);
        rawSourceComponent = receivedMessage[idx..endIdx];
        idx = endIdx + 1;
    }

    endIdx = receivedMessage.IndexOf(':', idx);
    if (endIdx == -1)
    {
        endIdx = receivedMessage.Length;
    }

    var rawCommandComponent = receivedMessage[idx..endIdx].Trim();

    if (endIdx != receivedMessage.Length)
    {  
        idx = endIdx + 1;
        rawParametersComponent = receivedMessage.Substring(idx);
    }

    //Parse command

    string? command = null;
    string? channel = null;
    bool? isCapRequestEnabled = null;
    string[] commandParts = rawCommandComponent.Split(' ');

    switch (commandParts[0])
    {
        case "JOIN":
        case "PART":
        case "NOTICE":
        case "CLEARCHAT":
        case "HOSTTARGET":
        case "PRIVMSG":
            command = commandParts[0];
            channel = commandParts[1];
            break;
        case "PING":
            command = commandParts[0];
            break;
        case "CAP":
            command = commandParts[0];
            isCapRequestEnabled = commandParts[2] == "ACK" ? true : false;
            break;
        case "GLOBALUSERSTATE":
            command = commandParts[0];
            break;
        case "USERSTATE":
        case "ROOMSTATE":
            command = commandParts[0];
            channel = commandParts[1];
            break;
        case "RECONNECT":
            Console.WriteLine("The Twitch IRC server is about to terminate the connection for maintenance.");
            command = commandParts[0];
            break;
        case "421":
            Console.WriteLine($"Unsupported IRC command:{commandParts[2]}");
            break;
        case "001":
            command = commandParts[0];
            channel = commandParts[1];
            break;
        case "002":
        case "003":
        case "004":
        case "353":
        case "366":
        case "372":
        case "375":
        case "376":
            Console.WriteLine($"numeric message:{commandParts[0]}");
            break;
        default:
            Console.WriteLine($"Unexpected command: {commandParts[0]}");
            break;

    }

    Console.WriteLine(command);
    Console.WriteLine(channel);
    Console.WriteLine(isCapRequestEnabled);

    //parse Tags

    if(rawTagComponent is not null)
    {
        string[] parsedTags = rawTagComponent.Split(';');

        foreach(var tag in parsedTags)
        {
            string[] parsedTag = tag.Split('=');
            string? tagValue = (parsedTag[1] == "") ? null : parsedTag[1];

            switch (parsedTag[0])
            {
                case "badges":
                case "badges-info":

                    if (tagValue is not null)
                    {
                        string [] badges = tagValue.Split(',');
                        foreach(var pair in badges)
                        {
                            string[] badgeParts = pair.Split('/');
                            //save on badge object
                            foreach(var badgePart in badgeParts)
                            {
                                Console.WriteLine(badgePart);
                            }
                        }
                    }
                    break;
                case "emotes":
                    if(tagValue is not null)
                    {
                        string[] emotes = tagValue.Split('/');

                        foreach(var emote in emotes)
                        {
                            string[] emoteParts = emote.Split(':');

                            string[] positions = emoteParts[1].Split(',');

                            foreach(var position in positions)
                            {
                                string[] positionParts = position.Split('-');
                                Console.WriteLine($"Starting emotes position:{positionParts[0]}");
                                Console.WriteLine($"Ending emotes position:{positionParts[1]}");
                            }
                        }
                    }
                    break;
                case "emotes-sets":

                    if (tagValue is not null)
                    {
                        string[] emoteString = tagValue.Split(',');
                        // Save as an array
                    }
                    break;
                default:
                    if (parsedTag[0] != "client-nonce" || parsedTag[0] != "flags")
                    {
                        Console.WriteLine(parsedTag[0]);
                    }
                    break;
            }
        }
    }

    //parse source

    if(rawSourceComponent is not null)
    {
        string[] sourceParts = rawSourceComponent.Split('!');

        string? nick = sourceParts.Length == 2 ? sourceParts[0] : null;
        string? host = sourceParts.Length == 2 ? sourceParts[1] : sourceParts[0];

        Console.WriteLine(nick);
        Console.WriteLine(host);

    }

    //parse parameters so this is the message

    Console.WriteLine(rawParametersComponent);





}

Console.WriteLine(clientWebSocket.State);
