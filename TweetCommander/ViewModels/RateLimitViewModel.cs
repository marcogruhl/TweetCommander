using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using TwitterSharp.Response;

namespace TweetCommander.ViewModels;

/// <summary>
/// https://developer.twitter.com/en/docs/twitter-api/rate-limits
/// </summary>
public class RateLimitViewModel : INotifyPropertyChanged
{
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private int _value;

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Percent));
        }
    }

    private int _max;



    public int Max
    {
        get => _max;
        set
        {
            _max = value;
            OnPropertyChanged();
        }
    }

    public double Percent => (double)Value / (double)Max;

    private int _reset;

    public int Reset
    {
        get => _reset;
        set
        {
            _reset = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ResetTime));
        }
    }

    public DateTime ResetTime => DateTimeOffset.FromUnixTimeSeconds(Reset).LocalDateTime;

    private Timer resetTimer = new(1000);

    private TimeSpan _resetTimeLeft;

    public TimeSpan ResetTimeLeft
    {
        get => _resetTimeLeft;
        set
        {
            _resetTimeLeft = value;
            OnPropertyChanged();
        }
    }

    public int ResetInterval => 900;

    public RateLimitViewModel(RateLimit rateLimit)
    {
        Name = rateLimit.Endpoint.ToString();
        Max = rateLimit.Limit;
        Value = rateLimit.Remaining;
        Reset = rateLimit.Reset;

        resetTimer.Elapsed += (sender, args) =>
        {
            if (DateTime.Now > ResetTime)
                Value = Max;
            else
            {
                ResetTimeLeft = ResetTime - DateTime.Now;
            }
        };

        resetTimer.Start();
    }

    public RateLimitViewModel(string name, int value, int max)
    {
        _name = name;
        _value = value;
        _max = max;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}