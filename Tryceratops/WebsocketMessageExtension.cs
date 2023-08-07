using System;
using System.Net.WebSockets;
using System.Text;

namespace Tryceratops
{
	public static class WebsocketMessageExtension
	{
		public static async Task SendStringMessageAsync(this ClientWebSocket clientWebSocket, string message)
		{
            byte[] MessageByte = Encoding.UTF8.GetBytes(message);
			ArraySegment<byte> MessageByteArray = new ArraySegment<byte>(MessageByte);
            await clientWebSocket.SendAsync(MessageByteArray, WebSocketMessageType.Text, true, CancellationToken.None);
		}
	}
}

