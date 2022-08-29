using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using TweetCommander.Helper;
using TwitterSharp.Response.RStream;
using TwitterSharp.Rule;
using Expression = TwitterSharp.Rule.Expression;

namespace TweetCommander.ViewModels;

internal class MainWindowViewModel : BindableBaseLight
{
    public ListCollectionView TweetsCollectionView { get; }
    public ListCollectionView RulesCollectionView { get; }
    public ListCollectionView RateLimitsCollectionView { get; }
    private Controller _controller { get; }
    public DelegateCommand AddRuleCommand { get; set; }
    public DelegateCommand<StreamInfo> DeleteRuleCommand { get; set; }
    public DelegateCommand<StreamInfo> LoadRuleCommand { get; set; }
    public DelegateCommand GetRecentCommand { get; set; }
    public DelegateCommand StartStopStreamCommand { get; set; }
    public DelegateCommand ClearTweetsCommand { get; set; }
    public DelegateCommand GetTweetsByIdCommand { get; set; }
    public DelegateCommand GetTweetsFromUserCommand { get; set; }
    public DelegateCommand KeywordAndOrCommand { get; set; }
    public DelegateCommand HashtagAndOrCommand { get; set; }
    public DelegateCommand MentionAndOrCommand { get; set; }
    public DelegateCommand CashtagAndOrCommand { get; set; }
    public DelegateCommand ResetExpressionCommand { get; set; }


    private string _bearerToken = ConfigHelper.GetValue(nameof(BearerToken), Environment.GetEnvironmentVariable("TWITTER_TOKEN"));

    public string BearerToken
    {
        get => _bearerToken;
        set 
        {
            Error = String.Empty;
            ConfigHelper.SetValue(ref _bearerToken, value);
            _controller.InitializeAsync(_bearerToken);
            CheckCanButtonsExecute();
        }
    }

    public string BearerTokenView
    {
        get => _bearerToken.Substring(0, 20) + new string('*', _bearerToken.Length - 24) + _bearerToken.Substring(_bearerToken.Length - 4, 4);
        set
        {
            BearerToken = value;
            OnPropertyChanged();
        }
    }

    private string _ruleTag = ConfigHelper.GetValue(nameof(RuleTag), "TwitterSharpDemo");

    public string RuleTag
    {
        get => _ruleTag;
        set 
        {
            if (String.IsNullOrEmpty(value))
            {
                value = "TwitterSharpDemo";
            }

            ConfigHelper.SetValue(ref _ruleTag, value);
        }
    }

    private string _getTweetByIdTweetId = ConfigHelper.GetValue(nameof(GetTweetByIdTweetId), "1389189291582967809");

    public string GetTweetByIdTweetId
    {
        get => _getTweetByIdTweetId;
        set 
        {
            if (String.IsNullOrEmpty(value))
            {
                value = "1389189291582967809";
            }

            ConfigHelper.SetValue(ref _getTweetByIdTweetId, value);
        }
    }

    private string _getTweetsFromUserUserId = ConfigHelper.GetValue(nameof(GetTweetsFromUserUserId), "1109748792721432577");

    public string GetTweetsFromUserUserId
    {
        get => _getTweetsFromUserUserId;
        set 
        {
            if (String.IsNullOrEmpty(value))
            {
                value = "1109748792721432577";
            }

            ConfigHelper.SetValue(ref _getTweetsFromUserUserId, value);
        }
    }

    #region Search Expression
    
    private string _keyword = ConfigHelper.GetValue(nameof(Keyword), String.Empty);
    [IsExpressionProperty]
    public string Keyword
    {
        get => _keyword;
        set => ConfigHelper.SetValue(ref _keyword, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private AndOrEnum _keywordAndOr = ConfigHelper.GetValue(nameof(KeywordAndOr), AndOrEnum.And);
    [IsExpressionProperty]
    public AndOrEnum KeywordAndOr
    {
        get => _keywordAndOr;
        set => ConfigHelper.SetValue(ref _keywordAndOr, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _hashtag = ConfigHelper.GetValue(nameof(Hashtag), "Anime");
    [IsExpressionProperty]
    public string Hashtag
    {
        get => _hashtag;
        set => ConfigHelper.SetValue(ref _hashtag, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private AndOrEnum _hashtagAndOr = ConfigHelper.GetValue(nameof(HashtagAndOr), AndOrEnum.And);
    [IsExpressionProperty]
    public AndOrEnum HashtagAndOr
    {
        get => _hashtagAndOr;
        set => ConfigHelper.SetValue(ref _hashtagAndOr, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _from = ConfigHelper.GetValue(nameof(From), String.Empty);
    [IsExpressionProperty]
    public string From
    {
        get => _from;
        set => ConfigHelper.SetValue(ref _from, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _mention = ConfigHelper.GetValue(nameof(Mention), String.Empty);
    [IsExpressionProperty]
    public string Mention
    {
        get => _mention;
        set => ConfigHelper.SetValue(ref _mention, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private AndOrEnum _mentionAndOr = ConfigHelper.GetValue(nameof(MentionAndOr), AndOrEnum.And);
    [IsExpressionProperty]
    public AndOrEnum MentionAndOr
    {
        get => _mentionAndOr;
        set => ConfigHelper.SetValue(ref _mentionAndOr, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _cashtag = ConfigHelper.GetValue(nameof(Cashtag), String.Empty);
    [IsExpressionProperty]
    public string Cashtag
    {
        get => _cashtag;
        set => ConfigHelper.SetValue(ref _cashtag, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private AndOrEnum _cashtagAndOr = ConfigHelper.GetValue(nameof(CashtagAndOr), AndOrEnum.And);
    [IsExpressionProperty]
    public AndOrEnum CashtagAndOr
    {
        get => _cashtagAndOr;
        set => ConfigHelper.SetValue(ref _cashtagAndOr, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _author = ConfigHelper.GetValue(nameof(Author), String.Empty);
    [IsExpressionProperty]
    public string Author
    {
        get => _author;
        set => ConfigHelper.SetValue(ref _author, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _recipient = ConfigHelper.GetValue(nameof(Recipient), String.Empty);
    [IsExpressionProperty]
    public string Recipient
    {
        get => _recipient;
        set => ConfigHelper.SetValue(ref _recipient, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _url = ConfigHelper.GetValue(nameof(Url), String.Empty);
    [IsExpressionProperty]
    public string Url
    {
        get => _url;
        set => ConfigHelper.SetValue(ref _url, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _retweet = ConfigHelper.GetValue(nameof(Retweet), String.Empty);
    [IsExpressionProperty]
    public string Retweet
    {
        get => _retweet;
        set => ConfigHelper.SetValue(ref _retweet, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _context = ConfigHelper.GetValue(nameof(Context), String.Empty);
    [IsExpressionProperty]
    public string Context
    {
        get => _context;
        set => ConfigHelper.SetValue(ref _context, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _entity = ConfigHelper.GetValue(nameof(Entity), String.Empty);
    [IsExpressionProperty]
    public string Entity
    {
        get => _entity;
        set => ConfigHelper.SetValue(ref _entity, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _conversionId = ConfigHelper.GetValue(nameof(ConversionId), String.Empty);
    [IsExpressionProperty]
    public string ConversionId
    {
        get => _conversionId;
        set => ConfigHelper.SetValue(ref _conversionId, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _bio = ConfigHelper.GetValue(nameof(Bio), String.Empty);
    [IsExpressionProperty]
    public string Bio
    {
        get => _bio;
        set => ConfigHelper.SetValue(ref _bio, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _bioLocation = ConfigHelper.GetValue(nameof(BioLocation), String.Empty);
    [IsExpressionProperty]
    public string BioLocation
    {
        get => _bioLocation;
        set => ConfigHelper.SetValue(ref _bioLocation, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _place = ConfigHelper.GetValue(nameof(Place), String.Empty);
    [IsExpressionProperty]
    public string Place
    {
        get => _place;
        set => ConfigHelper.SetValue(ref _place, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _placeCountry = ConfigHelper.GetValue(nameof(PlaceCountry), String.Empty);
    [IsExpressionProperty]
    public string PlaceCountry
    {
        get => _placeCountry;
        set => ConfigHelper.SetValue(ref _placeCountry, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _isRetweet = ConfigHelper.GetValue(nameof(IsRetweet), (bool?)false);
    [IsExpressionProperty]
    public bool? IsRetweet
    {
        get => _isRetweet;
        set => ConfigHelper.SetValue(ref _isRetweet, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _isReply = ConfigHelper.GetValue(nameof(IsReply), (bool?)null);
    [IsExpressionProperty]
    public bool? IsReply
    {
        get => _isReply;
        set => ConfigHelper.SetValue(ref _isReply, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _isQuote = ConfigHelper.GetValue(nameof(IsQuote), (bool?)null);
    [IsExpressionProperty]
    public bool? IsQuote
    {
        get => _isQuote;
        set => ConfigHelper.SetValue(ref _isQuote, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _isVerified = ConfigHelper.GetValue(nameof(IsVerified), (bool?)true);
    [IsExpressionProperty]
    public bool? IsVerified
    {
        get => _isVerified;
        set => ConfigHelper.SetValue(ref _isVerified, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _isNotNullcast = ConfigHelper.GetValue(nameof(IsNotNullcast), (bool?)null);
    [IsExpressionProperty]
    public bool? IsNotNullcast
    {
        get => _isNotNullcast;
        set => ConfigHelper.SetValue(ref _isNotNullcast, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasHashtags = ConfigHelper.GetValue(nameof(HasHashtags), (bool?)null);
    [IsExpressionProperty]
    public bool? HasHashtags
    {
        get => _hasHashtags;
        set => ConfigHelper.SetValue(ref _hasHashtags, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasCashtags = ConfigHelper.GetValue(nameof(HasCashtags), (bool?)null);
    [IsExpressionProperty]
    public bool? HasCashtags
    {
        get => _hasCashtags;
        set => ConfigHelper.SetValue(ref _hasCashtags, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasLinks = ConfigHelper.GetValue(nameof(HasLinks), (bool?)null);
    [IsExpressionProperty]
    public bool? HasLinks
    {
        get => _hasLinks;
        set => ConfigHelper.SetValue(ref _hasLinks, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasMentions = ConfigHelper.GetValue(nameof(HasMentions), (bool?)null);
    [IsExpressionProperty]
    public bool? HasMentions
    {
        get => _hasMentions;
        set => ConfigHelper.SetValue(ref _hasMentions, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasMedia = ConfigHelper.GetValue(nameof(HasMedia), (bool?)null);
    [IsExpressionProperty]
    public bool? HasMedia
    {
        get => _hasMedia;
        set => ConfigHelper.SetValue(ref _hasMedia, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasImages = ConfigHelper.GetValue(nameof(HasImages), (bool?)null);
    [IsExpressionProperty]
    public bool? HasImages
    {
        get => _hasImages;
        set => ConfigHelper.SetValue(ref _hasImages, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasVideos = ConfigHelper.GetValue(nameof(HasVideos), (bool?)null);
    [IsExpressionProperty]
    public bool? HasVideos
    {
        get => _hasVideos;
        set => ConfigHelper.SetValue(ref _hasVideos, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private bool? _hasGeo = ConfigHelper.GetValue(nameof(HasGeo), (bool?)null);
    [IsExpressionProperty]
    public bool? HasGeo
    {
        get => _hasGeo;
        set => ConfigHelper.SetValue(ref _hasGeo, value, propertyChangedAction: () => OnPropertyChanged());
    }

    private string _lang = ConfigHelper.GetValue(nameof(Lang), "ko,ja,en");
    [IsExpressionProperty]
    public string Lang
    {
        get => _lang;
        set => ConfigHelper.SetValue(ref _lang, value, propertyChangedAction: () => OnPropertyChanged());
    }

    #endregion Search Expression

    private bool _updateFilterNeeded;

    public bool UpdateFilterNeeded
    {
        get => _updateFilterNeeded;
        set => SetProperty(ref _updateFilterNeeded, value);
    }

    private string _expressionString;

    public string ExpressionString
    {
        get => _expressionString;
        private set
        {
            if (SetProperty(ref _expressionString, value))
            {
                OnPropertyChanged(nameof(ExpressionLength));
                OnPropertyChanged(nameof(ExpressionLengthLimit));
            }
        }
    }

    public const int RuleCharacterLimit = 512;

    public int ExpressionLength => ExpressionString == null ? 0 : ExpressionString.Length;
    public bool ExpressionLengthLimit => ExpressionLength < RuleCharacterLimit;

    private string _error;

    public string Error
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public string StreamButtonText => _controller.IsConnected == null ? "Start Stream" : "Stop Stream";
    public string StreamStatusText => _controller.IsConnected == null ? "No Streaming" : "Stream Started";
    
    private bool? _isConnected;

    public bool? IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public MainWindowViewModel()
    {
        _controller = new((s) => Error = s);
        _controller.ConnectionChanged += OnConnectionChangedAction;
        IsConnected = _controller.IsConnected;

        TweetsCollectionView = new ListCollectionView(_controller.Tweets);
        TweetsCollectionView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
        RulesCollectionView = new ListCollectionView(_controller.Rules);
        RateLimitsCollectionView = new ListCollectionView(_controller.RateLimits);
        AddRuleCommand = new DelegateCommand(AddRuleAction, ButtonCanExecuteAction);
        DeleteRuleCommand = new DelegateCommand<StreamInfo>(DeleteRuleAction, ButtonCanExecuteAction);
        LoadRuleCommand = new DelegateCommand<StreamInfo>(LoadRuleAction, ButtonCanExecuteAction);
        GetRecentCommand = new DelegateCommand(GetRecentAction, ButtonCanExecuteAction);
        StartStopStreamCommand = new DelegateCommand(StartStopStreamAction, ButtonCanExecuteAction);
        ClearTweetsCommand = new DelegateCommand(ClearTweetsAction, ButtonCanExecuteAction);
        GetTweetsByIdCommand = new DelegateCommand(GetTweetsByIdAction, ButtonCanExecuteAction);
        GetTweetsFromUserCommand = new DelegateCommand(GetTweetsFromUserAction, ButtonCanExecuteAction);
        ResetExpressionCommand = new DelegateCommand(ResetExpressionAction);

        CheckCanButtonsExecute();

        KeywordAndOrCommand = new DelegateCommand(() => KeywordAndOr = KeywordAndOr == AndOrEnum.And ? AndOrEnum.Or : AndOrEnum.And);
        HashtagAndOrCommand = new DelegateCommand(() => HashtagAndOr = HashtagAndOr == AndOrEnum.And ? AndOrEnum.Or : AndOrEnum.And);
        MentionAndOrCommand = new DelegateCommand(() => MentionAndOr = MentionAndOr == AndOrEnum.And ? AndOrEnum.Or : AndOrEnum.And);
        CashtagAndOrCommand = new DelegateCommand(() => CashtagAndOr = CashtagAndOr == AndOrEnum.And ? AndOrEnum.Or : AndOrEnum.And);

        PropertyChanged += OnPropertyChanged;

        RefreshRule();
    }

    private void LoadRuleAction(StreamInfo obj)
    {
        // Group with different types
        var t1 = "(\"Twitter API\" OR #v2) -\"recent search\""; 
        var e1 = Expression.Keyword("Twitter API").Or(Expression.Hashtag("v2")).And(Expression.Keyword("recent search").Negate());
        var s1 = e1.ToString().Substring(1, e1.ToString().Length - 2);
        var q1 = s1.Equals(t1);



        var x1_e = ToExpression(t1);
        var x1_s = x1_e.ToString().Substring(1, x1_e.ToString().Length - 2);
        var x1_q = x1_s.Equals(t1);



        var t2 = "\"Twitter API\"";
        var e2 = Expression.Keyword("Twitter API");
        var s2 = e2.ToString();
        var q2 = s2.Equals(t2);



        // MultiLevelRule
        var t3 = "((\"Twitter API\" OR #v2) -\"recent search\") OR ((grumpy cat) OR (#meme has:images -is:retweet))"; 
        var e3 = Expression.Keyword("Twitter API").Or(Expression.Hashtag("v2")).And(Expression.Keyword("recent search").Negate());
        var s3 = e1.ToString().Substring(1, e1.ToString().Length - 2);
        var q3 = s1.Equals(t1);
        
        var x3_1 = Expression.Keyword("Twitter API");
        var x3_2 = Expression.Hashtag("v2");
        var x3_a = x3_1.Or(x3_2);
        var x3_3 = Expression.Keyword("recent search");
        var x3_3b = x3_3.Negate();
        var x3_b = x3_a.And(x3_3b);
        var x3_4 = Expression.Keyword("grumpy");
        var x3_5 = Expression.Keyword("cat");
        var x3_c = x3_4.And(x3_5);
        var x3_6 = Expression.Hashtag("meme");
        var x3_7 = Expression.HasImages();
        var x3_8 = Expression.IsRetweet();
        var x3_8b = x3_8.Negate();
        var x3_d = x3_6.And(x3_7, x3_8b);
        var x3_e = x3_c.Or(x3_d);
        var x3_f = x3_b.Or(x3_e);

        Expression ToExpression(string s)
        {
            // if(s[0] == '(' && s[s.Length -1] == ')')
            //     s = s.Substring(1, s.Length - 2);

            // Find Quote and Replace
            // https://regex101.com/r/9P1hCA/1
            var quotes = new List<string>();
            var q = 0;

            foreach (Match match in Regex.Matches(s, "(\\\").*?(\")"))
            {
                quotes.Add(match.Value);
                s = s.ReplaceFirst(match.Value, $"{r}q{q++}{r}");
            }

            List<Expression> expressions = new List<Expression>();
            var e = 0;
            foreach (var stringExpression in s.Replace($" OR ", $" ").Replace($"(", $"").Replace($")", $"").Split(' '))
            {
                var isNegate = stringExpression.StartsWith('-');

                var sr = isNegate ? stringExpression.Substring(1) : stringExpression;

                // TODO: Optimize quote logic
                if (stringExpression.Contains($"{r}q"))
                    sr = quotes[Int32.Parse(Regex.Match(stringExpression, @"\d+").Value)].Replace("\"","");

                if (sr.StartsWith('#'))
                    AddExpression(Expression.Hashtag(sr.Substring(1)));
                else if (sr.StartsWith('$'))
                    AddExpression(Expression.Cashtag(sr.Substring(1)));
                else if (sr.StartsWith('@'))
                    AddExpression(Expression.Mention(sr.Substring(1)));
                else if (sr.StartsWith("from:"))
                    AddExpression(Expression.Author(sr.Replace("from:", "")));
                else if (sr.StartsWith("to:"))
                    AddExpression(Expression.Recipient(sr.Replace("to:", "")));
                else if (sr.StartsWith("url:"))
                    AddExpression(Expression.Url(sr.Replace("url:", "")));
                else if (sr.StartsWith("retweets_of:"))
                    AddExpression(Expression.Retweet(sr.Replace("retweets_of:", "")));
                else if (sr.StartsWith("context:"))
                    AddExpression(Expression.Context(sr.Replace("context:", "")));
                else if (sr.StartsWith("entity:"))
                    AddExpression(Expression.Entity(sr.Replace("entity:", "")));
                else if (sr.StartsWith("conversation_id:"))
                    AddExpression(Expression.ConversationId(sr.Replace("conversation_id:", "")));
                else if (sr.StartsWith("bio:"))
                    AddExpression(Expression.Bio(sr.Replace("bio:", "")));
                else if (sr.StartsWith("bio_location:"))
                    AddExpression(Expression.BioLocation(sr.Replace("bio_location:", "")));
                else if (sr.StartsWith("place:"))
                    AddExpression(Expression.Place(sr.Replace("place:", "")));
                else if (sr.StartsWith("place_country:"))
                    AddExpression(Expression.PlaceCountry(sr.Replace("place_country:", "")));
                else if (sr.StartsWith("sample:"))
                    AddExpression(Expression.Sample(Int32.Parse(sr.Replace("sample:", ""))));
                else if (sr.StartsWith("lang:"))
                    AddExpression(Expression.Lang(sr.Replace("lang:", "")));
                else if (sr == "is:retweet")
                    AddExpression(Expression.IsRetweet());
                else if (sr == "is:reply")
                    AddExpression(Expression.IsReply());
                else if (sr == "is:quote")
                    AddExpression(Expression.IsQuote());
                else if (sr == "is:verified")
                    AddExpression(Expression.IsVerified());
                // TODO: FIX SPECIAL CASE
                else if (sr == "is:nullcast")
                    AddExpression(Expression.IsNotNullcast());
                else if (sr == "has:hashtags")
                    AddExpression(Expression.HasHashtags());
                else if (sr == "has:cashtags")
                    AddExpression(Expression.HasCashtags());
                else if (sr == "has:links")
                    AddExpression(Expression.HasLinks());
                else if (sr == "has:mentions")
                    AddExpression(Expression.HasMentions());
                else if (sr == "has:media")
                    AddExpression(Expression.HasMedia());
                else if (sr == "has:images")
                    AddExpression(Expression.HasImages());
                else if (sr == "has:videos")
                    AddExpression(Expression.HasVideos());
                else if (sr == "has:geo")
                    AddExpression(Expression.HasGeo());
                else
                    AddExpression(Expression.Keyword(sr));

                void AddExpression(Expression exp)
                {
                    if (isNegate)
                        expressions.Add(exp.Negate());
                    else
                        expressions.Add(exp);
                }

                s = s.ReplaceFirst(stringExpression, $"{r}e{e++}{r}");
            }

            // Find groups recursive
            // https://regex101.com/r/xJaODO/2
            var groups = new Dictionary<string[],bool>();
            // var g = 0;
            FindGroups();

            void FindGroups()
            {
                foreach (Match match in Regex.Matches(s, "\\(+.*?\\)"))
                {
                    var group = match.Value.Replace("(", "").Replace(")", "");
                    AddToGroup(group);
                    s = s.ReplaceFirst($"({group})", $"{r}g{e++}{r}");
                }
                
                if(s.Contains('(')) 
                    FindGroups();
                else
                    AddToGroup(s); // most top group should be left

                void AddToGroup(string ga)
                {
                    // TODO: Mixed groups!?
                    if(ga.Contains(" OR "))
                        groups.Add(ga.Split(" OR "), false);
                    else
                        groups.Add(ga.Split(" "), true);
                }
            }

            // build expression
            foreach (var group in groups)
            {
                var groupExpression = new List<Expression>();

                foreach (var k in group.Key)
                {
                    var index = Regex.Match(k, @"\d+").Value;
                    groupExpression.Add(expressions[Int32.Parse(index)]);

                }

                if (group.Value)
                    expressions.Add(groupExpression[0].And(groupExpression.Skip(1).ToArray()));
                else
                    expressions.Add(groupExpression[0].Or(groupExpression.Skip(1).ToArray()));
            }

                return expressions.Last();
        }
        
        var x3_s = x3_f.ToString().Substring(1, x3_f.ToString().Length - 2);
        var x3_q = x3_s.Equals(t3);

        var x3_x = ToExpression(t3);
        var x3_t = x3_x.ToString().Substring(1, x3_x.ToString().Length - 2);
        var x3_r = x3_t.Equals(t3);

        // _controller.AddRule(x3_f, "MultiLevel Rule");

        return;
















        // test: https://regex101.com/r/gFgyi1/1

        var t = "((key k1 k2) (#ht1 OR #\"ht 2\") @asdad OR from:\"sss dfdffs\" (lang:de OR lang:en) -is:retweet is:verified)";
        // var t =  obj.Value;

        var matches = MatchCollection(t);
        // List<Expression> expressions = new List<Expression>();

        foreach (Match match in matches)
        {

            // expressions.Add(GetExpression(match.Value));
            SetExpression(match.Value);
            // recursiv, wenn ( ) bzw. hier sind OR/AND wichtig
            // if (match.Contains('('))
            // {
            //     var matchess = MatchCollection(match.ToString());
            // }
        }
    }
        
    // const char r = '\t'; // replaceCharacter
    const char r = '~'; // replaceCharacter

    void SetExpression(string match)
    {
        // if(match[0] == '(')
        //     match = match.Substring(1, match.Length - 1);
        // if (matches.Count == 0 && !blu.Contains($"(") &&  (blu.Contains($"{r}AND{r}") || blu.Contains($"{r}OR{r}")))
        // {
        //     var splitable = blu.Replace($"{r}{r}", $" ").Replace($"{r}AND{r}", $"{r}").Replace($"{r}OR{r}", $"{r}").Replace($"{r}AND", $"{r}").Replace($"{r}OR", $"{r}").Replace($")", $"");
        //     var keys = splitable.Split(r, StringSplitOptions.RemoveEmptyEntries);
        //
        //     return keys;
        // }
        // else if(!blu.Contains($"("))
        // {
        //     return matches.Select(x => x.Value.Replace($"{r}{r}", $" ").Replace($"{r}AND{r}", $"{r}").Replace($"{r}OR{r}", $"{r}").Replace($"{r}AND", $"{r}").Replace($"{r}OR", $"{r}").Replace($")", $"")).ToArray();
        // }


        var c = match.Replace($"{r}{r}", $" ").Replace($"{r}AND{r}", $"{r}").Replace($"{r}OR{r}", $"{r}").Replace($"{r}AND", $"{r}").Replace($"{r}OR", $"{r}").Replace($"(", $"").Replace($")", $"");
        var keys = c.Split(r, StringSplitOptions.RemoveEmptyEntries);

        if (c.Contains("from:"))
        {

        }
        
        // return Expression.Bio("ddd");
    }

    private static MatchCollection MatchCollection(string t)
    {
        var c = t.Substring(1, t.Length - 2); // cut ( )

        // //Create a substitution pattern for the Replace method
        // var replacePattern = "${label}: ${tag}";
        //
        // return Regex.Replace(str, pattern, replacePattern, RegexOptions.IgnoreCase);
        var bli = Regex.Replace(c, "(\\\").*?(\")", (m) => m.ToString().Replace(" ", $"{r}{r}"));
        var blo = bli.Replace(" OR ", $"{r}OR{r}");
        var blu = blo.Replace(" ", $"{r}AND{r}");
        //     // var exp = new Expression(String.Empty, obj.Value);

        // Regex trennen, merkt nicht AND/OR -> ignorieren, da immer AND in tweet commander
        // (\().*?\)|(-?(@|is|from)).*?((?=((\tAND\t)|(\tOR\t)))|(?=($)))
        // var matches = Regex.Matches(blu, "(\\().*?\\)|(-?(@|#|\\$|is|from|lang)).*?((?=((\\tAND\\t)|(\\tOR\\t)))|(?=($)))"); // w/o AND & OR
        var matches = Regex.Matches(blu, $"((\\().*?\\)|(-?(@|#|\\$|is|from|lang))).*?({r}AND{r}|{r}OR{r}|($))"); // inclusice AND & OR

        return matches;
    }

    // class splitItem
    // {
    //
    //     private IEnumerable<splitItem> splitItems;
    //
    //     public splitItem(string s)
    //     {
    //         splitItems = new[] { s };
    //     }
    // }
    //
    // private static IEnumerable<splitItem> GetGroupedItemsGroup(string input)
    // {
    //     var split = input.Split('(', 2);
    //
    //     if(split.Length == 1)
    //         yield return new splitItem()split
    //     if (group.HasSubgroups)
    //         foreach (IGroup subGroup in group.Subgroups)
    //         foreach (var childOfChild in GetGroupedItemsGroup<T>(subGroup))
    //             yield return childOfChild;
    //     else
    //         foreach (var childOfChild in group.Items)
    //             yield return (T)childOfChild;
    // }

    private void ResetExpressionAction()
    {
        Keyword = String.Empty;
        Hashtag = String.Empty;
        Mention = String.Empty;
        Cashtag = String.Empty;
        From = String.Empty;
        Lang = String.Empty;
        
        KeywordAndOr = AndOrEnum.And;
        HashtagAndOr = AndOrEnum.And;
        MentionAndOr = AndOrEnum.And;
        CashtagAndOr = AndOrEnum.And;

        IsReply = null;
        IsRetweet = null;
        IsQuote = null;
        IsVerified = null;
        IsNotNullcast = null;
        HasHashtags = null;
        HasCashtags = null;
        HasLinks = null;
        HasMentions = null;
        HasMedia = null;
        HasImages = null;
        HasVideos = null;
        HasGeo = null;
    }

    private void OnConnectionChangedAction(object? sender, bool? e)
    {
        OnPropertyChanged(nameof(StreamButtonText));
        OnPropertyChanged(nameof(StreamStatusText));
        IsConnected = _controller.IsConnected;
    }

    private bool ButtonCanExecuteAction()
    {
        return !String.IsNullOrEmpty(BearerToken);
    }

    private void CheckCanButtonsExecute()
    {
        DeleteRuleCommand.RaiseCanExecuteChanged();
        GetRecentCommand.RaiseCanExecuteChanged();
        AddRuleCommand.RaiseCanExecuteChanged();
        StartStopStreamCommand.RaiseCanExecuteChanged();
        ClearTweetsCommand.RaiseCanExecuteChanged();
        GetTweetsByIdCommand.RaiseCanExecuteChanged();
        GetTweetsFromUserCommand.RaiseCanExecuteChanged();
    }

    private async void ClearTweetsAction()
    {
        _controller.ClearTweets();
    }

    private async void StartStopStreamAction()
    {
        if(_controller.IsConnected.HasValue && _controller.IsConnected.Value)
        {
            _controller.Disconnect();
        }
        else
        {
            _controller.Connect();
        }
    }

    private async void AddRuleAction()
    {
        Error = String.Empty;
        await _controller.AddRule(BuildExpression(), RuleTag);
    }

    private async void GetRecentAction()
    {
        Error = String.Empty;
        await _controller.GetRecentTweets(BuildExpression());
    }

    private async void GetTweetsByIdAction()
    {
        Error = String.Empty;
        await _controller.GetTweetsById(GetTweetByIdTweetId);
    }

    private async void GetTweetsFromUserAction()
    {
        Error = String.Empty;
        await _controller.GetTweetsFromUser(GetTweetsFromUserUserId);
    }

    private async void DeleteRuleAction(StreamInfo rule)
    {
        await _controller.DeleteRule(rule, true);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (IsExpressionPropertyAttribute.IsExpressionProperty(e.PropertyName, this))
        {
            RefreshRule();
        }
    }

    private async void RefreshRule(bool force = false)
    {
        var expression = BuildExpression();
        var expressionString = ExpressionString = expression.ToString();
    }
    
    private Expression BuildExpression()
    {
        List<Expression> expressions = new List<Expression>();

        List<Expression> keywordExpressions = new List<Expression>();
        List<Expression> hashtagExpressions = new List<Expression>();
        List<Expression> mentionExpressions = new List<Expression>();
        List<Expression> cachtagExpressions = new List<Expression>();

        List<Expression> langExpressions = new List<Expression>();
        List<Expression> fromExpressions = new List<Expression>();

        Keyword?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => keywordExpressions.Add(Expression.Keyword(x)));
        Hashtag?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => hashtagExpressions.Add(Expression.Hashtag(x)));
        Mention?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => mentionExpressions.Add(Expression.Mention(x)));
        Cashtag?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => cachtagExpressions.Add(Expression.Cashtag(x)));
        From?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => fromExpressions.Add(Expression.Author(x)));
        Lang?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ForEach(x => langExpressions.Add(Expression.Lang(x)));
        
        AddExpression(keywordExpressions, KeywordAndOr);
        AddExpression(hashtagExpressions, HashtagAndOr);
        AddExpression(mentionExpressions, MentionAndOr);
        AddExpression(cachtagExpressions, CashtagAndOr);
        AddExpression(fromExpressions, AndOrEnum.Or);
        AddExpression(langExpressions, AndOrEnum.Or);

        void AddExpression(List<Expression> searchExpression, AndOrEnum andOr)
        {
            if (searchExpression.Any())
            {
                expressions.Add(andOr == AndOrEnum.And
                    ? searchExpression[0].And(searchExpression.Skip(1).ToArray())
                    : searchExpression[0].Or(searchExpression.Skip(1).ToArray()));
            }
        }

        if (IsReply != null) expressions.Add(IsReply.Value ? Expression.IsReply() : Expression.IsReply().Negate());
        if (IsRetweet != null) expressions.Add(IsRetweet.Value ? Expression.IsRetweet() : Expression.IsRetweet().Negate());
        if (IsQuote != null) expressions.Add(IsQuote.Value ? Expression.IsQuote() : Expression.IsQuote().Negate());
        if (IsVerified != null) expressions.Add(IsVerified.Value ? Expression.IsVerified() : Expression.IsVerified().Negate());
        // if (IsNotNullcast != null) expressions.Add(IsNotNullcast.Value ? Expression.IsNotNullcast() : Expression.IsNotNullcast().Negate());
        if (HasHashtags != null) expressions.Add(HasHashtags.Value ? Expression.HasHashtags() : Expression.HasHashtags().Negate());
        if (HasCashtags != null) expressions.Add(HasCashtags.Value ? Expression.HasCashtags() : Expression.HasCashtags().Negate());
        if (HasLinks != null) expressions.Add(HasLinks.Value ? Expression.HasLinks() : Expression.HasLinks().Negate());
        if (HasMentions != null) expressions.Add(HasMentions.Value ? Expression.HasMentions() : Expression.HasMentions().Negate());
        if (HasMedia != null) expressions.Add(HasMedia.Value ? Expression.HasMedia() : Expression.HasMedia().Negate());
        if (HasImages != null) expressions.Add(HasImages.Value ? Expression.HasImages() : Expression.HasImages().Negate());
        if (HasVideos != null) expressions.Add(HasVideos.Value ? Expression.HasVideos() : Expression.HasVideos().Negate());
        if (HasGeo != null) expressions.Add(HasGeo.Value ? Expression.HasGeo() : Expression.HasGeo().Negate());

        var expression = expressions.Any()
            ? expressions.First().And(expressions.Skip(1).ToArray())
            : Expression.Keyword("");
        // Error-Test:
        // expression = Expression.Keyword(Keyword).And(Expression.IsReply().Negate(), Expression.IsRetweet().Negate(), Expression.PlaceCountry("xx"));

        return expression;
    }
}