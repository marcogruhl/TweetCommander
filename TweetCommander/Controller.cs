using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TweetCommander.Helper;
using TweetCommander.ViewModels;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response;
using TwitterSharp.Response.RStream;
using Expression = TwitterSharp.Rule.Expression;

namespace TweetCommander;

internal class Controller : INotifyPropertyChanged, IAsyncDisposable
{
    private TwitterClient _client { get; set; }
    private Task? _tweetStream { get; set; }
    private CancellationTokenSource _cancellationTokenSource = new();
    public readonly ObservableCollection<StreamInfo> Rules = new();
    internal readonly ObservableCollection<TweetViewModel> Tweets = new();
    public readonly ObservableCollection<RateLimitViewModel> RateLimits = new ();

    public event Action<string> Error;
    public event EventHandler<bool?>? ConnectionChanged;

    private int _tweetId = 0;

    private bool? _isConnected { get; set; }
    
    public bool? IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            ConnectionChanged?.Invoke(this, _isConnected);
            OnPropertyChanged();
        }
    }


    private TweetOption[] _tweetOptions = {
        TweetOption.Created_At,
        TweetOption.In_Reply_To_User_Id,
        TweetOption.Referenced_Tweets,
        TweetOption.Reply_Settings,
        TweetOption.Lang,
        TweetOption.Public_Metrics
    };

    private UserOption[] _userOptions = {
        UserOption.Verified,
        UserOption.Created_At,
        UserOption.Entities,
        UserOption.Public_Metrics,
        UserOption.Protected
    };

    public Controller(Action<string> errorAction)
    {
        Error = errorAction;
        Application.Current.Exit += async (_, _) => await DisposeAsync();
        InitializeAsync(ConfigHelper.GetValue("BearerToken", Environment.GetEnvironmentVariable("TWITTER_TOKEN")));
    }
    
    public async void InitializeAsync(string bearerToken)
    {
        if (String.IsNullOrEmpty(bearerToken))
        {
            Error.Invoke("Empty BearerToken");
            return;
        }

        await DisposeAsync();

        _client = new TwitterClient(bearerToken);
        _client.RateLimitChanged += UpdateRateLimits;

        await RefreshRules();

        // if(AutoConnect)
        //     await Connect();
    }

    public async Task Connect()
    {
        try
        {
            IsConnected = false;

            if (!Rules.Any())
            {
                // Subscriptions.Add("Every3Minutes");
                // await RefreshSubscriptions();
            }

            _cancellationTokenSource = new();

            // NextTweetStreamAsync will continue to run in background
            _tweetStream = Task.Run(async () =>
            {
                // Take in parameter a callback called for each new tweet
                // Since we want to get the basic info of the tweet author, we add an empty array of UserOption
                await _client.NextTweetStreamAsync((tweet) =>
                {
                    if(tweet == null)
                        return;

                    Debug.WriteLine($"From {tweet.Author.Name}: {tweet.Text}");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Tweets.Add(new TweetViewModel(tweet, "Stream", Interlocked.Increment(ref _tweetId)));
                        Debug.WriteLine($"From {tweet.Author.Name}: {tweet.Text} (Rules: {string.Join(',', tweet.MatchingRules.Select(x => x.Tag))})");
                    });
                        
                }, new TweetSearchOptions
                {
                    TweetOptions = _tweetOptions,
                    UserOptions = _userOptions,

                });
            }, _cancellationTokenSource.Token).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted)
                {
                    IsConnected = null;
                    // _logger.Twitter(t.Exception.GetType() + " " + t.Exception.Message, t.Exception);
                }

                _cancellationTokenSource.Dispose();
            });

            IsConnected = true;

            // _client.C

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            IsConnected = null;
        }
    }

    private async Task RefreshRules()
    {
        try
        {
            var rules = await _client.GetInfoTweetStreamAsync();

            Rules.Clear();

            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }

        }
        catch (Exception e)
        {
            Error.Invoke(e.Message);
        }

        UpdateRateLimits(new RateLimitViewModel("RulesPerStream", 25 - Rules.Count, 25));
    }



    internal async Task GetTweetsById(string tweetId)
    {
        var searchOptions = new TweetSearchOptions
        {
            TweetOptions = _tweetOptions,
            UserOptions = _userOptions,
        };

        try
        {
            var tweet = await _client.GetTweetAsync(tweetId, searchOptions);
            Tweets.Add(new TweetViewModel(tweet, "TweetById", Interlocked.Increment(ref _tweetId)));
        }
        catch (TwitterException e)
        {
            Error.Invoke(TwitterExceptionToString(e));
        }
    }

    internal async Task GetTweetsFromUser(string userId, int amount = 10)
    {
        var searchOptions = new TweetSearchOptions
        {
            TweetOptions = _tweetOptions,
            UserOptions = _userOptions,
            Limit = amount < 10 ? 10 : amount > 100 ? 100 : amount
        };

        try
        {
            var res = await _client.GetTweetsFromUserIdAsync(userId, searchOptions);
            foreach (var tweet in res)
            {
                Tweets.Add(new TweetViewModel(tweet, "TweetsFromUser", Interlocked.Increment(ref _tweetId)));
            }
            
        }
        catch (TwitterException e)
        {
            Error.Invoke(TwitterExceptionToString(e));
        }
    }

    internal async Task GetRecentTweets(Expression expression, int amount = 10)
    {
        var searchOptions = new TweetSearchOptions
        {
            TweetOptions = _tweetOptions,
            UserOptions = _userOptions,
            Limit = amount < 10 ? 10 : amount > 100 ? 100 : amount
        };

        try
        {
            var res = await _client.GetRecentTweets(expression, searchOptions);
            foreach (var tweet in res)
            {
                Tweets.Add(new TweetViewModel(tweet, "Recent", Interlocked.Increment(ref _tweetId)));
            }
            
        }
        catch (TwitterException e)
        {
            Error.Invoke(TwitterExceptionToString(e));
        }
    }

    internal async Task AddRule(Expression expression, string subscriptionTag)
    {
        if (String.IsNullOrEmpty(expression.ToString()))
            Error.Invoke("expression is empty");
        if (String.IsNullOrEmpty(subscriptionTag))
            Error.Invoke("subscriptionTag is empty");
        
        try
        {
            var existingRules = Rules.Where(x => x.Tag == subscriptionTag);
            foreach (var existingRule in existingRules)
            {
                await DeleteRule(existingRule);
            }

            await _client.AddTweetStreamAsync(new TwitterSharp.Request.StreamRequest(expression, subscriptionTag));
            
            await RefreshRules();
        }
        catch (TwitterException e)
        {
            Error.Invoke(TwitterExceptionToString(e));
        }

        UpdateRateLimits(new RateLimitViewModel("RulesPerStream", 25 - Rules.Count, 25));
    }

    internal async Task DeleteRule(StreamInfo rule, bool refreshRules = false)
    {
        await _client.DeleteTweetStreamAsync(rule.Id);
        if(refreshRules)
            await RefreshRules();
    }

    private static string TwitterExceptionToString(TwitterException e)
    {
        StringBuilder sb = new();

        sb.AppendLine($"{e.Message} Title: {e.Title} Url: {e.Type}");

        if (e.Errors.Any())
            foreach (var error in e.Errors)
            {
                sb.AppendLine(
                    $"{error.Title} Value: {error.Value} {(error.Details != null ? $"Details: {String.Join(',', error.Details)}" : error.Message)}");
            }

        return sb.ToString();
    }

    internal void ClearTweets()
    {
        Tweets.Clear();
    }

    private void UpdateRateLimits(object? sender, RateLimit rateLimit)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var viewModel = RateLimits.FirstOrDefault(x => x.Name == rateLimit.Endpoint.ToString());

            if (viewModel == null)
            {
                RateLimits.Add(new RateLimitViewModel(rateLimit));
            }
            else
            {
                viewModel.Value = rateLimit.Remaining;
                viewModel.Reset = rateLimit.Reset;
            }
        });
    }

    private void UpdateRateLimits(RateLimitViewModel rateLimit)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var viewModel = RateLimits.FirstOrDefault(x => x.Name == rateLimit.Name);

            if (viewModel == null)
            {
                RateLimits.Add(rateLimit);
            }
            else
            {
                viewModel.Value = rateLimit.Value;
                viewModel.Reset = rateLimit.Reset;
            }
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task Disconnect()
    {
        IsConnected = false;

        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException e)
        {
            // Not throwing
        }
        
        if (_tweetStream != null)
        {
            _client.CancelTweetStream(true);
            await _tweetStream.WaitAsync(CancellationToken.None); // could take up to 20 sesonds (keep alive signal)
            _tweetStream.Dispose();
        }

        IsConnected = null;
    }

    public async ValueTask DisposeAsync()
    {
        await Disconnect();

        if(_client != null)
        {
            _client.RateLimitChanged -= UpdateRateLimits;
            _client.Dispose();
        }
    }
}