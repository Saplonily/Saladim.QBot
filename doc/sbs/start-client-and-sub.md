# Saladim.QBot docs

## 启动Client并订阅事件

### 启动Client

client对象是本框架的核心, 用于和gocq沟通, 接收的上报由它调度并引发事件, 调用的api由它序列化发送出去. 在GoCqHttp实现里为`CqClient`抽象类.  
首先不用质疑的是新建一个示例并接收:
```cs
CqClient cqClient = new CqWebSocketClient("ws://127.0.0.1:8080", LogLevel.Trace);
```
如果你使用http通讯:
```cs
CqClient cqClient = new CqHttpClient("http://127.0.0.1:5700","http://127.0.0.1:5701", LogLevel.Trace);
```

上述的构造器中最后一个可选参数表示该client的日志限制等级, 目前我们没有日志量过大的顾虑, 所以我们显式设置为Trace, 即输出所有日志.

然后我们异步开启这个client:
```cs
await cqClient.StartAsync();
```
在中间阻塞或者等待一个特殊指令:
```cs
string cmd = "";
while((cmd = Console.ReadLine()) != "exit")
{ }
```
最后记得异步关闭它
```cs
await cqClient.StopAsync();
```
然后订阅一下日志事件, 让我们了解client的状态:
```cs
cqClient.OnLog += Console.WriteLine();
```
现在我们可以直接编译运行查看结果了, 目前来说你的输出会是这个样子:
```log
[Info][Client/Connection] Connecting api session...
[Info][Client/Connection] Connecting post session...
[Info][Client/Connection] Connection completed.
```
常见的异常输出可能有:
- 目标计算机积极拒绝
    - 检查你的目标是否正在监听这个地址
    - 检查端口号是否错误
- 连接超时
    - 检查地址是否输错
    - 检查本机网络是否正常
如果你遇到了其余的异常但是不能自己处理你可以可选的联系作者(非框架问题不保证完全解决)

### 订阅事件

一切正常后, 我们可以开始订阅收到消息时触发的事件了, 
首先我们订阅收到消息事件, 这个事件会在收到私聊/群聊消息时触发
```cs
client.OnMessageReceived += Client_OnMessageReceived;

// 函数原型如下:
void Client_OnMessageReceived(Message message)
```
然后我们在`Client_OnMessageReceived`里面做点事, 比如说直接把这个消息内容打印出来:
```cs
string s = $"{message.Author.Nickname.Value} 说: {message.MessageEntity.RawString}";
Console.WriteLine(s);
```
现在先不管这段代码干了什么, 先让我们运行看看效果, 记得给bot号或者他所在的任何一个群发消息:
```log
Saplonily 说: 你好这是群消息
Saplonily 说: 这是给你私聊的消息
```
一切正常时你应该会看到类似的结果, 如果没有的话你可以
- 查看gocq是否正在运行并且是否收到了来自群/私聊的消息
    - 如果是, 请检查是否正确订阅消息, 或是否正常输出消息
    - 如果否, 检查一下是否正确配置这个环境

如果你依旧遇到了无法解决的问题你可以联系作者(同样的, 非框架问题可能也不会确保完全解决).

最后修改: 2022-12-10 20:05:06