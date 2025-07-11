# 心率监测 WebSocket 服务

这是一个基于ASP.NET Core的WebSocket服务，用于接收心率监测设备的数据并转发给其他连接的客户端。

## 功能

- 接收心率数据（包含心率值、时间戳和设备名称）
- 验证客户端Token确保安全性
- 向其他所有已连接的客户端广播心率数据（不包含Token）

## 技术栈

- ASP.NET Core 9.0
- WebSockets
- System.Text.Json

## 数据格式

### 接收的数据格式

```json
{
  "heartRate": 75,
  "timestamp": "2023-04-10T15:30:45.123Z",
  "deviceName": "HeartMonitor-X1",
  "token": "token.txt"
}
```
token是鉴权的唯一方式，请自行在程序主目录添加token.txt，并填入token

### 广播的数据格式

```json
{
  "heartRate": 75,
  "timestamp": "2023-04-10T15:30:45.123Z",
  "deviceName": "HeartMonitor-X1"
}
```

## 使用方式

1. 启动服务
2. 通过WebSocket连接到 `ws://localhost:5000/ws` 或 `wss://localhost:5001/ws`
3. 发送包含有效Token的心率数据JSON
4. 接收来自其他客户端的心率数据广播
