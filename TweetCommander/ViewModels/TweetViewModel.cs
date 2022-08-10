using TwitterSharp.Response.RTweet;

namespace TweetCommander.ViewModels
{
    internal class TweetViewModel
    {
        public string Type { get; set; }

        public Tweet Tweet { get; set; }

        public int Id { get; set; }

        public TweetViewModel(Tweet tweet, string type, int id)
        {
            Tweet = tweet;
            Type = type;
            Id = id;
        }
    }
}
