using System;
using System.Collections.Generic;
using System.Threading;

public class MessageRateCalculator
{
    private int _messageCount;
    private int _totalMessages;
    private double _maxRate;
    private readonly TimeSpan _interval;
    private readonly Timer _timer;
    private readonly object _lock = new object();
    private double _messageRate;
    private readonly MessageAnalyzer _analyzer;

    public MessageRateCalculator()
    {
        _analyzer = new MessageAnalyzer();
        _interval = TimeSpan.FromSeconds(1); // Intervalo de 1 segundo
        _timer = new Timer(UpdateMessageRate, null, TimeSpan.Zero, _interval);
    }

    public void IncrementMessageCount(long? fixtureId, string superOddsType)
    {
        lock (_lock)
        {
            AnalyzeMessage(fixtureId, superOddsType);
            _messageCount++;
            _totalMessages++;
        }
    }

    public void AnalyzeMessage(long? fixtureId, string superOddType)
    {
        _analyzer.AnalyzeMessage(fixtureId,superOddType);
    }

    private void UpdateMessageRate(object state)
    {
        lock (_lock)
        {
            _messageRate = _messageCount / _interval.TotalSeconds;
            _maxRate = Math.Max(_maxRate, _messageRate);
            var marketCounts = _analyzer.GetMarketCounts();
            var uniqueFixtureCount = _analyzer.GetUniqueFixtureCount();

            if (_messageRate > 0)
            {
                Console.WriteLine($"Current rate: {_messageRate} messages/s | Max rate: {_maxRate} messages/s | Total messages: {_totalMessages} | Unique fixtures: {uniqueFixtureCount}");
                foreach (var marketCount in marketCounts)
                {
                    Console.WriteLine($"Market '{marketCount.Key}': {marketCount.Value} messages");
                }
            }

            _messageCount = 0;
        }
    }

    public double GetRatesAndTotal()
    {
        lock (_lock)
        {
            return _totalMessages;
        }
    }

    public Dictionary<string, int> GetMarketCounts()
    {
        return _analyzer.GetMarketCounts();
    }

    public int GetUniqueFixtureCount()
    {
        return _analyzer.GetUniqueFixtureCount();
    }
}

public class MessageAnalyzer
{
    private readonly Dictionary<string, int> _marketCounts = new Dictionary<string, int>();
    private readonly HashSet<long?> _uniqueFixtureIds = new HashSet<long?>();

    public void AnalyzeMessage(long? fixtureId, string superOddsType)
    {
         if (_marketCounts.ContainsKey(superOddsType))
        {
            _marketCounts[superOddsType]++;
        }
        else
        {
            _marketCounts[superOddsType] = 1;
        }
        _uniqueFixtureIds.Add(fixtureId);
    }

    public Dictionary<string, int> GetMarketCounts()
    {
        return _marketCounts;
    }

    public int GetUniqueFixtureCount()
    {
        return _uniqueFixtureIds.Count;
    }
}