using System;

namespace EventBus.NET;

/// <summary>
/// 事件订阅者接口，允许订阅和取消订阅事件。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public interface IEventSubscriber<TEventSender, TEventArgs>
{
    /// <summary>
    /// 订阅事件。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="handler">事件处理程序。</param>
    /// <returns>返回一个可释放的对象，用于取消订阅。</returns>
    IDisposable Subject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs;

    /// <summary>
    /// 取消订阅事件。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="handler">事件处理程序。</param>
    void UnSubject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs;
}
