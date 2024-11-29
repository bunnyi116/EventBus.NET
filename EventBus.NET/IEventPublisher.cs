namespace EventBus.NET;

/// <summary>
/// 事件发布者接口，允许发布事件。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public interface IEventPublisher<TEventSender, TEventArgs>
{
    /// <summary>
    /// 发布事件。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="sender">事件发送者。</param>
    /// <param name="args">事件参数。</param>
    void Publish<TArgs>(TEventSender sender, TArgs args) where TArgs : TEventArgs;
}
