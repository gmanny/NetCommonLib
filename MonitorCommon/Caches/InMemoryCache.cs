using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MonitorCommon.Caches;

public class InMemoryCache<TKey, TValue>
{
    private readonly Dictionary<TKey, (TValue value, LinkedListNode<TimeBox<TKey>> listNode)> cache = new();
    private readonly LinkedList<TimeBox<TKey>> usageTimes = new();

    private readonly Object sync = new();
    private readonly TimeSpan maxAge;
    private readonly int maxSize;
    private readonly ILogger logger;
    private readonly string name;
    private readonly TimeSpan cacheSizeReportingTime;
    private readonly Action<TValue> onRemove;
    private readonly bool overflowIsCritical;

    private int hits;
    private int misses;
    private DateTime lastReport = DateTime.UtcNow;

    public InMemoryCache(TimeSpan maxAge, int maxSize, ILogger logger, string name, TimeSpan cacheSizeReportingTime, Action<TValue> onRemove = default, bool overflowIsCritical = true)
    {
        this.maxAge = maxAge;
        this.maxSize = maxSize;
        this.logger = logger;
        this.name = name;
        this.cacheSizeReportingTime = cacheSizeReportingTime;
        this.onRemove = onRemove;
        this.overflowIsCritical = overflowIsCritical;
    }

    private void RemoveItem(LinkedListNode<TimeBox<TKey>> item)
    {
        lock (sync)
        {
            if (cache.TryGetValue(item.Value.Value, out (TValue value, LinkedListNode<TimeBox<TKey>> listNode) val))
            {
                onRemove?.Invoke(val.value);

                cache.Remove(item.Value.Value);
            }
            usageTimes.Remove(item);
        }
    }

    private void RemoveExpiredEntries(DateTime now)
    {
        lock (sync)
        {
            LinkedListNode<TimeBox<TKey>> last = usageTimes.Last;
            while (last != null && now - last.Value.Time > maxAge)
            {
                RemoveItem(last);

                last = usageTimes.Last;
            }
        }
    }

    public void RemoveKey(TKey key)
    {
        lock (sync)
        {
            if (usageTimes.NonEmpty())
            {
                // remove expired entries first
                RemoveExpiredEntries(DateTime.UtcNow);

                // find access time
                if (cache.TryGetValue(key, out (TValue value, LinkedListNode<TimeBox<TKey>> listNode) val))
                {
                    RemoveItem(val.listNode);
                }
            }
        }
    }
        
    public TValue Get(TKey key, Func<TKey, TValue> producer)
    {
        DateTime now = DateTime.UtcNow;
        lock (sync)
        {
            // remove expired entries first
            RemoveExpiredEntries(now);

            if (now - lastReport > cacheSizeReportingTime && usageTimes.NonEmpty())
            {
                logger.LogDebug($"InMemCache `{name}` size = {usageTimes.Count}, hits = {hits}, misses = {misses}");
                   
                lastReport = now;
                    
                hits = 0;
                misses = 0;
            }

            if (usageTimes.NonEmpty())
            {
                // if cache hit
                if (cache.TryGetValue(key, out (TValue value, LinkedListNode<TimeBox<TKey>> listNode) existing))
                {
                    // update access time
                    LinkedListNode<TimeBox<TKey>> usageTime = existing.listNode;

                    usageTimes.Remove(usageTime);
                    LinkedListNode<TimeBox<TKey>> newNode = usageTimes.AddFirst(new TimeBox<TKey>(now, key));

                    cache[key] = (existing.value, newNode);

                    hits++;

                    return existing.value;
                }
            }

            misses++;

            // no cache hit, create & add
            TValue result = producer(key);

            LinkedListNode<TimeBox<TKey>> nn = usageTimes.AddFirst(new TimeBox<TKey>(now, key));
            cache.Add(key, (result, nn));

            // optionally truncate cache to allowed size
            if (usageTimes.Count > maxSize)
            {
                if (overflowIsCritical)
                {
                    logger.LogWarning($"InMemCache `{name}` overflow detected at {usageTimes.Count} items");
                }

                while (usageTimes.Count > maxSize)
                {
                    LinkedListNode<TimeBox<TKey>> last = usageTimes.Last;

                    RemoveItem(last);
                }
            }

            return result;
        }
    }
}