using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBus.NET;

/// <summary>
/// 事件总线实现，支持线程安全的事件发布和订阅。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public class EventBus<TEventSender, TEventArgs> : IEventBus<TEventSender, TEventArgs> where TEventArgs : notnull
{
    private readonly object _lock = new();  
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = []; 

    /// <summary>
    /// 回调异常（发布方法在调用订阅者处理程序时发生的异常）
    /// </summary>
    public event Action<Exception>? OnCallbackException;

    /// <summary>
    /// 发布事件，支持事件继承关系的父类订阅。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="sender">事件发送者。</param>
    /// <param name="args">事件参数。</param>
    public void Publish<TArgs>(TEventSender sender, TArgs args) where TArgs : TEventArgs
    {
        // 获取所有与事件参数类型匹配的订阅者，包括父类类型的订阅者。
        lock (_lock)
        {
            var eventType = typeof(TArgs);
            var subscribers = _subscriptions
                .Where(entry => entry.Key.IsAssignableFrom(args.GetType())) // 支持继承关系
                .SelectMany(entry => entry.Value)
                .ToList();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber.DynamicInvoke(sender, args);
                }
                catch (TargetInvocationException ex)
                {
                    OnCallbackException?.Invoke(ex.InnerException);
                }
                catch (Exception ex)
                {
                    OnCallbackException?.Invoke(ex);
                }
            }

        }
    }

    /// <summary>
    /// 订阅事件，返回一个可释放的对象来取消订阅。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="handler">事件处理程序。</param>
    /// <returns>返回一个可释放的对象，用于取消订阅。</returns>
    public IDisposable Subject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs
    {
        lock (_lock)
        {
            if (!_subscriptions.ContainsKey(typeof(TArgs)))
            {
                _subscriptions[typeof(TArgs)] = [];
            }

            _subscriptions[typeof(TArgs)].Add(handler);
        }

        return new SubjectDisposable<TArgs>(this, handler);
    }

    /// <summary>
    /// 取消订阅事件。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="handler">事件处理程序。</param>
    public void UnSubject<TArgs>(EventHandler<TEventSender, TArgs> handler) where TArgs : TEventArgs
    {
        lock (_lock)
        {
            if (_subscriptions.ContainsKey(typeof(TArgs)))
            {
                _subscriptions[typeof(TArgs)].Remove(handler);
            }
        }
    }

    /// <summary>
    /// 订阅取消的实现类，允许通过实现 IDisposable 来清除资源。
    /// </summary>
    private class SubjectDisposable<TArgs>(EventBus<TEventSender, TEventArgs> eventBus, EventHandler<TEventSender, TArgs> handler) : IDisposable where TArgs : TEventArgs
    {
        private readonly EventBus<TEventSender, TEventArgs> _eventBus = eventBus;
        private readonly EventHandler<TEventSender, TArgs> _handler = handler;

        public void Dispose()
        {
            _eventBus.UnSubject(_handler);
        }
    }
}
