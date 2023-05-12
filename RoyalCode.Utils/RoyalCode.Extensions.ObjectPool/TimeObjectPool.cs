// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RoyalCode.Extensions.ObjectPool;

/// <summary>
/// Default implementation of <see cref="ObjectPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type to pool objects for.</typeparam>
/// <remarks>This implementation keeps a cache of retained objects. This means that if objects are returned when the pool has already reached "maximumRetained" objects they will be available to be Garbage Collected.</remarks>
public sealed class TimeObjectPool<T> : ObjectPool<T>, IDisposable
    where T : class
{
    private volatile bool _isDisposed;

    private readonly Func<T> _createFunc;
    private readonly Func<T, bool> _returnFunc;
    private readonly Action<T> _destroyFunc;
    private readonly int _minCapacity, _maxCapacity;
    private int _numItems;

    private readonly ConcurrentQueue<T> _items = new();
    private readonly Timer _timer;

    private T? _fastItem;

    /// <summary>
    /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
    /// </summary>
    /// <param name="policy">The pooling policy to use.</param>
    public TimeObjectPool(IPooledObjectPolicy<T> policy, TimeSpan? timeSpan = null)
        : this(policy, Environment.ProcessorCount * 2, Environment.ProcessorCount * 8, timeSpan)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="DefaultObjectPool{T}"/>.
    /// </summary>
    /// <param name="policy">The pooling policy to use.</param>
    /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
    public TimeObjectPool(IPooledObjectPolicy<T> policy, int minimalRetained, int maximumRetained,
        TimeSpan? timeSpan = null)
    {
        if (minimalRetained < 1)
            throw new ArgumentOutOfRangeException(
                nameof(minimalRetained), minimalRetained, "Minimal retained must be greater than 0.");

        if (maximumRetained <= minimalRetained)
            throw new ArgumentOutOfRangeException(
                nameof(maximumRetained), maximumRetained, "Maximum retained must be greater than minimal retained.");

        // cache the target interface methods, to avoid interface lookup overhead
        _createFunc = policy.Create;
        _returnFunc = policy.Return;
        _minCapacity = minimalRetained;
        _maxCapacity = maximumRetained - 1; // -1 to account for _fastItem
        _destroyFunc = policy is IDestructionPolicy<T> destructionPolicy ? destructionPolicy.Destroy : DestroyItem;

        var time = timeSpan ?? TimeSpan.FromMinutes(4);
        _timer = new Timer(TryDestroy, null, time, time);
    }

    private void TryDestroy(object? _)
    {
        while(_numItems > _minCapacity)
            if (_items.TryDequeue(out var item))
            {
                Interlocked.Decrement(ref _numItems);
                _destroyFunc(item);
            }
    }

    /// <inheritdoc />
    public override T Get()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        var item = _fastItem;
        if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
        {
            if (_items.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _numItems);
                return item;
            }

            // no object available, so go get a brand new one
            return _createFunc();
        }

        return item;
    }

    /// <inheritdoc />
    public override void Return(T obj)
    {
        // When the pool is disposed or the obj is not returned to the pool, destroy it
        if (_isDisposed || !ReturnCore(obj))
        {
            _destroyFunc(obj);
        }
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <returns>true if the object was returned to the pool</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ReturnCore(T obj)
    {
        if (!_returnFunc(obj))
        {
            // policy says to drop this object
            return false;
        }

        if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
        {
            if (Interlocked.Increment(ref _numItems) <= _maxCapacity)
            {
                _items.Enqueue(obj);
                return true;
            }

            // no room, clean up the count and drop the object on the floor
            Interlocked.Decrement(ref _numItems);
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _timer.Dispose();

        if (_fastItem is not null)
        {
            _destroyFunc(_fastItem);
            _fastItem = null;
        }
        
        while (_items.TryDequeue(out var item))
        {
            _destroyFunc(item);
        }
    }

    private static void DestroyItem(T? item)
    {
        if (item is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
