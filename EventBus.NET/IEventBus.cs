namespace EventBus.NET;

/// <summary>
/// 事件总线接口，结合了事件订阅和发布功能。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public interface IEventBus<TEventSender, TEventArgs> : IEventSubscriber<TEventSender, TEventArgs>, IEventPublisher<TEventSender, TEventArgs>
{
}
