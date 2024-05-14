namespace JH_Reddit.Server.Models
{
    public class RedditPost
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public int BeginningUps { get; set; }

        public int CurrentUps { get; set; }

        public int UpVotes { get;set; }

        public string? Author { get; set; }
    }
}
