namespace JH_Reddit.Server.Models
{
    public class SubReddit
    {
        public string Name { get; set; }

        public RedditLimits Limits { get; set; }

        public List<RedditPost> Posts { get; set; }

        public string TopAuthor {  get; set; }

        public RedditPost TopPost { get; set; }
    }
}
