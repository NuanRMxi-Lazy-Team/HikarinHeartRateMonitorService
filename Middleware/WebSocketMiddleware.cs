using System.Net.WebSockets;
using System.Text;
using HikarinHeartRateMonitorService.Services;
using WebSocketManager = HikarinHeartRateMonitorService.Services.WebSocketManager;

namespace HikarinHeartRateMonitorService.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketManager _webSocketManager;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(RequestDelegate next, WebSocketManager webSocketManager, 
            ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _webSocketManager = webSocketManager;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            _webSocketManager.AddSocket(socketId, socket);
            _logger.LogInformation($"WebSocket连接已建立: {socketId}");

            await ReceiveMessages(socketId, socket);
        }

        private async Task ReceiveMessages(string socketId, WebSocket socket)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await _webSocketManager.HandleMessageAsync(socketId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocketManager.RemoveSocket(socketId);
                        _logger.LogInformation($"WebSocket连接已关闭: {socketId}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket处理时发生错误: {socketId}");
                await _webSocketManager.RemoveSocket(socketId);
            }
        }
    }

    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseHeartRateWebSockets(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketMiddleware>();
        }
    }
}
