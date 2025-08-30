using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBus.NET;

public class EventBus<TEventArgs> : EventBus<object?, TEventArgs> { }

/// <summary>
/// 事件总线实现，支持线程安全的事件发布和订阅。
/// </summary>
/// <typeparam name="TEventSender">事件发送者的类型。</typeparam>
/// <typeparam name="TEventArgs">事件参数的类型。</typeparam>
public class EventBus<TEventSender, TEventArgs> : IEventBus<TEventSender, TEventArgs>
{
    private readonly Dictionary<Type, SubscriptionHandlersInfo> _subscriptions = [];
    private readonly List<EventHandler<TEventSender, Exception>> _exceptions = [];

    /// <summary>
    /// 发布事件，支持事件继承关系的父类订阅。
    /// </summary>
    /// <typeparam name="TArgs">事件参数的类型。</typeparam>
    /// <param name="sender">事件发送者。</param>
    /// <param name="args">事件参数。</param>
    public void Publish<TArgs>(TEventSender sender, TArgs args) where TArgs : TEventArgs
    {
        // 获取所有与事件参数类型匹配的订阅者，包括父类类型的订阅者。
        var subscribers = _subscriptions.Where(subInfo => subInfo.Key.IsAssignableFrom(args?.GetType()))   // 获取相关的订阅信息
            .SelectMany((subInfo) => subInfo.Value)
            .ToList();

        foreach (var subscriber in subscribers)
        {
            try
            {
                subscriber?.Invoke(sender, args);
            }
            catch (TargetInvocationException ex)
            {
                foreach (var item in _exceptions)
                {
                    item?.Invoke(sender, ex.InnerException);
                }

            }
            catch (Exception ex)
            {
                foreach (var item in _exceptions)
                {
                    item?.Invoke(sender, ex);
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
        // 包装处理程序以适应基类的事件参数类型
        var wrapper = new EventHandler<TEventSender, TEventArgs>((sender, args) =>
        {
            if (args is TArgs derivedArgs)
            {
                handler?.Invoke(sender, derivedArgs);
            }
        });
        // 将包装的处理程序添加到订阅列表中
        var type = typeof(TArgs);
        if (!_subscriptions.ContainsKey(type))
        {
            _subscriptions[type] = [];
        }
        _subscriptions[type].AddHandler(wrapper);

        // 创建并返回一个 IDisposable 对象，用于取消订阅
        return new SubjectDisposable<TArgs>(this, wrapper);
    }

    public IDisposable SubjectException(EventHandler<TEventSender, Exception> handler)
    {
        var wrapper = new EventHandler<TEventSender, TEventArgs>((sender, args) =>
        {
            if (args is Exception derivedArgs)
            {
                handler?.Invoke(sender, derivedArgs);
            }
        });

        if (!_exceptions.Contains(handler))
        {
            _exceptions.Add(handler);
        }

        return new SubjectDisposable<Exception>(this, wrapper);
    }

    /// <summary>
    /// 订阅取消的实现类，允许通过实现 IDisposable 来清除资源。
    /// </summary>
    private sealed class SubjectDisposable<TArgs>(EventBus<TEventSender, TEventArgs> eventBus, EventHandler<TEventSender, TEventArgs> handler) : IDisposable
    {
        private readonly EventBus<TEventSender, TEventArgs> _eventBus = eventBus;
        private readonly EventHandler<TEventSender, TEventArgs> _handler = handler;

        public void Dispose()
        {
            if (_eventBus._subscriptions.TryGetValue(typeof(TArgs), out var handlers))
            {
                if (handlers == null)
                {
                    return;
                }
                handlers.RemoveHandler(_handler);
            }
        }
    }

    /// <summary>
    /// 订阅信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="handler"></param>
    private sealed class SubscriptionHandlersInfo(params EventHandler<TEventSender, TEventArgs>[]? handlers) : IEnumerable<EventHandler<TEventSender, TEventArgs>>
    {
        /// 锁对象
        private readonly object _lock = new();

        /// <summary>
        /// 订阅处理程序列表
        /// </summary>
        private List<EventHandler<TEventSender, TEventArgs>> Handlers { get; set; } = handlers == null ? [] : [.. handlers];

        /// <summary>
        /// 获取指定类型的处理程序
        /// </summary>
        /// <typeparam name="TArgs"></typeparam>
        /// <returns></returns>
        public IEnumerable<EventHandler<TArgs>> GetHandlers<TArgs>() where TArgs : TEventArgs
        {
            lock (_lock)
            {
                return Handlers.OfType<EventHandler<TArgs>>();
            }
        }

        /// <summary>
        /// 获取所有处理程序
        /// </summary>
        /// <returns></returns>
        public List<EventHandler<TEventSender, TEventArgs>> GetHandlers()
        {
            lock (_lock)
            {
                return Handlers;
            }
        }

        /// <summary>
        /// 添加处理程序
        /// </summary>
        /// <param name="handler"></param>
        public void AddHandler(EventHandler<TEventSender, TEventArgs> handler)
        {
            lock (_lock)
            {
                Handlers.Add(handler);
            }
        }

        /// <summary>
        /// 移除处理程序
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveHandler(EventHandler<TEventSender, TEventArgs> handler)
        {
            lock (_lock)
            {
                if (Handlers.Contains(handler))
                {
                    Handlers.Remove(handler);
                }
            }
        }

        public IEnumerator<EventHandler<TEventSender, TEventArgs>> GetEnumerator()
        {
            return ((IEnumerable<EventHandler<TEventSender, TEventArgs>>)Handlers).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Handlers).GetEnumerator();
        }
    }
}
