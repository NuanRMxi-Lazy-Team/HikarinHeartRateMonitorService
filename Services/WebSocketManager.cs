using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using HikarinHeartRateMonitorService.Models;

namespace HikarinHeartRateMonitorService.Services
{
    public class WebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly string _validToken = File.ReadAllText("token.txt");

        public void AddSocket(string id, WebSocket socket)
        {
            _sockets.TryAdd(id, socket);
        }

        public async Task RemoveSocket(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                    "Connection closed by the server", CancellationToken.None);
            }
        }

        public async Task HandleMessageAsync(string senderId, string message)
        {
            try
            {
                var heartRateData = JsonSerializer.Deserialize<HeartRateData>(message);

                // 验证Token
                if (heartRateData?.Token != _validToken)
                {
                    await CloseInvalidConnection(senderId, "Invalid token");
                    return;
                }

                // 创建不包含Token的响应对象
                var response = new HeartRateResponse
                {
                    HeartRate = heartRateData.HeartRate,
                    Timestamp = heartRateData.Timestamp,
                    DeviceName = heartRateData.DeviceName
                };

                var responseJson = JsonSerializer.Serialize(response);
                var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                // 向除了发送者之外的所有客户端广播消息
                var tasks = _sockets
                    .Where(kvp => kvp.Key != senderId)
                    .Select(kvp => SendMessageAsync(kvp.Value, responseBytes));

                await Task.WhenAll(tasks);
            }
            catch (JsonException)
            {
                await CloseInvalidConnection(senderId, "Invalid message format");
            }
        }

        private async Task CloseInvalidConnection(string id, string reason)
        {
            if (_sockets.TryGetValue(id, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, 
                    reason, CancellationToken.None);
                await RemoveSocket(id);
            }
        }

        private static async Task SendMessageAsync(WebSocket socket, byte[] message)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(message), 
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
