using System.Threading.RateLimiting;

namespace JH_Reddit.Server.Models
{
    public class RedditLimits
    {
        public int Used { get; set; } //X-Ratelimit-Used 
        public double Remaining { get; set; } //X-Ratelimit-Remaining { get; set; }
        public int Reset { get; set; } //X-Ratelimit - Reset { get; set; }
    }
}
