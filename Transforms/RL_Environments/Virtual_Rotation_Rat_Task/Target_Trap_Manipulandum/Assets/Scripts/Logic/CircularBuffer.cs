using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class CircularBuffer<T>
{
    //Creates a circular buffer

    private readonly ConcurrentQueue<T> _data;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly int _size;

    public CircularBuffer(int size)
    {
        if (size < 1)
        {
            throw new ArgumentException($"{nameof(size)} cannot be negative or zero");
        }
        _data = new ConcurrentQueue<T>();
        _size = size;
    }

    public T Latest()
    {
        T value = _data.ToArray()[_data.Count - 1];
        return value;
    }
    

    public IEnumerable<T> AsArray()
    {
        return _data.ToArray();
    }

    public void Add(T t)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_data.Count == _size)
            {
                T value;
                _data.TryDequeue(out value);
            }

            _data.Enqueue(t);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
