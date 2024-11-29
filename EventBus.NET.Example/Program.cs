using EventBus.NET;

// 创建事件总线（对外访问可以使用IEventSubscriber接口进行访问，这里就不细分了）
var eventBus = new EventBus<MyEventSender, EventArgs>();

// 事件发布时，在调用用户的处理程序时捕捉到异常的事件
eventBus.OnCallbackException += (ex) =>
{
    Console.WriteLine(ex.Message);
};

// 订阅事件（基类订阅, 子类发布事件，基类订阅者也会被通知）
var subscription1 = eventBus.Subject<EventArgs>((sender, args) =>
{
    Console.WriteLine($"{sender.SenderName}: {args.BaseMessage}  ==>  {typeof(EventArgs).Name}");
    // throw new Exception("错误");  // Exception test
});

// 订阅事件
var subscription2 = eventBus.Subject<MyEventArgs>((sender, args) =>
{
    Console.WriteLine($"{sender.SenderName}: {args.Message}  ==>  {typeof(MyEventArgs).Name}");
});

// 创建事件发送者
var sender = new MyEventSender("Sender");

// 发布事件
eventBus.Publish(sender, new MyEventArgs("Hello, EventBus, This is 1 output!"));

// 取消订阅
subscription1.Dispose();
subscription2.Dispose();

// 发布事件（订阅处理程序已经被取消，不会有新的输出）
eventBus.Publish(sender, new MyEventArgs("Hello, EventBus, This is 2 output!"));

// 阻塞
Console.ReadLine();

abstract record EventArgs(string BaseMessage);
record MyEventArgs(string Message) : EventArgs(Message);
record MyEventSender(string SenderName);