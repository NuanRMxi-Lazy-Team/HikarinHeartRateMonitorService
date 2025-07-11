using HikarinHeartRateMonitorService.Middleware;
using HikarinHeartRateMonitorService.Services;
using WebSocketManager = HikarinHeartRateMonitorService.Services.WebSocketManager;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 添加控制器支持
builder.Services.AddControllers();

// 注册WebSocket管理器
builder.Services.AddSingleton<WebSocketManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 添加WebSocket支持
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
    AllowedOrigins = { "*" } // 生产环境中请设置具体的允许来源
});

// 使用自定义WebSocket中间件
app.UseHeartRateWebSockets();

// 添加静态文件支持
app.UseStaticFiles();

/*调试页面
app.MapGet("/", async context => {
    context.Response.Redirect("/index.html");
});
*/

app.MapControllers();

app.Run();