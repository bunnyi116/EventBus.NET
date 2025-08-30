using EventBus.NET;

var events = new EventBus<MyEventSender, EventArgs>();
IEventSubscriber<MyEventSender, EventArgs> subscriber = events; // 对外接口
IEventPublisher<MyEventSender, EventArgs> publisher = events;   // 内部接口(内部当然也可以直接使用EventBus实例, 这里只是一个示范)


var ex = subscriber.SubjectException((sender, args) =>
{
    Console.WriteLine(args.Message);
});

var sub1 = subscriber.Subject<EventArgs>((sender, args) =>
{
    Console.WriteLine($"[EventArgs] {sender.SenderName}: {args.BaseMessage}");
    throw new Exception("这是订阅者1抛出的异常");  // 模拟异常抛出
});

var sub2 = subscriber.Subject<MyEventArgs>((sender, args) =>
{
    Console.WriteLine($"[MyEventArgs] {sender.SenderName}: {args.Message}");
});

var sender = new MyEventSender("Sender");

events.Publish(sender, new EventArgs("这是 1 个输出！"));     // 第1次发布事件(预期只有一个基类订阅者处理输出)
Console.WriteLine();

events.Publish(sender, new MyEventArgs("这是 2 个输出！"));   // 第2次发布事件
Console.WriteLine();

// 中途取已订阅的事件
sub1.Dispose();
sub2.Dispose();

// 第3次发布事件(正常情况下不会有任何输出，因为被取消了)
events.Publish(sender, new MyEventArgs("这是 3 个输出！"));


// 阻塞
Console.ReadLine();
record EventArgs(string BaseMessage);  // 基类
record MyEventArgs(string Message) : EventArgs(Message);    // 派生类
record MyEventSender(string SenderName);    // 事件发送者
