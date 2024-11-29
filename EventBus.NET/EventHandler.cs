namespace EventBus.NET;

/// <summary>
/// 定义事件处理程序的委托类型。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public delegate void EventHandler<TEventSender, TEventArgs>(TEventSender sender, TEventArgs args);