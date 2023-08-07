using System;
namespace Tryceratops
{
    public class TwitchMessage
    {
        public string? Command { get; }
        public string? Channel { get; }

        public TwitchMessage(string receivedMessage)
		{
            string? rawTagComponent = null;
            string? rawSourceComponent = null;
            string? rawParametersComponent = null;
            int idx = 0;
            int endIdx;

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
                    this.Command = commandParts[0];
                    this.Channel = commandParts[1];
                    break;
                case "PING":
                    this.Command = commandParts[0];
                    break;
                case "CAP":
                    this.Command = commandParts[0];
                    isCapRequestEnabled = commandParts[2] == "ACK" ? true : false;
                    break;
                case "GLOBALUSERSTATE":
                    this.Command = commandParts[0];
                    break;
                case "USERSTATE":
                case "ROOMSTATE":
                    this.Command = commandParts[0];
                    this.Channel = commandParts[1];
                    break;
                case "RECONNECT":
                    Console.WriteLine("The Twitch IRC server is about to terminate the connection for maintenance.");
                    this.Command = commandParts[0];
                    break;
                case "421":
                    Console.WriteLine($"Unsupported IRC command:{commandParts[2]}");
                    break;
                case "001":
                    this.Command = commandParts[0];
                    this.Channel = commandParts[1];
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

            Console.WriteLine(this.Command);
            Console.WriteLine(this.Channel);
            Console.WriteLine(isCapRequestEnabled);

            //parse Tags

            if (rawTagComponent is not null)
            {
                string[] parsedTags = rawTagComponent.Split(';');

                foreach (var tag in parsedTags)
                {
                    string[] parsedTag = tag.Split('=');
                    string? tagValue = (parsedTag[1] == "") ? null : parsedTag[1];

                    switch (parsedTag[0])
                    {
                        case "badges":
                        case "badges-info":

                            if (tagValue is not null)
                            {
                                string[] badges = tagValue.Split(',');
                                foreach (var pair in badges)
                                {
                                    string[] badgeParts = pair.Split('/');
                                    //save on badge object
                                    foreach (var badgePart in badgeParts)
                                    {
                                        Console.WriteLine(badgePart);
                                    }
                                }
                            }
                            break;
                        case "emotes":
                            if (tagValue is not null)
                            {
                                string[] emotes = tagValue.Split('/');

                                foreach (var emote in emotes)
                                {
                                    string[] emoteParts = emote.Split(':');

                                    string[] positions = emoteParts[1].Split(',');

                                    foreach (var position in positions)
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

            if (rawSourceComponent is not null)
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
	}
}

