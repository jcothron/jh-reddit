namespace JH_Reddit.Server.Controllers
{
    #region usings

    using JH_Reddit.Server.Models;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Net;

    #endregion

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RedditDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public RedditDataController(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "SubReddit")]
        public IActionResult SubReddit([FromBody] SubReddit subReddit)
        {
            var token = _configuration["AppSettings:token"];
            var userAgent = _configuration["AppSettings:user-agent"];
            var baseUrl = _configuration["AppSettings:base-url"];

            if (string.IsNullOrEmpty(userAgent))
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://oauth.reddit.com");
                        client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                        var response = client.GetAsync($"/r/{subReddit.Name}").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var headersDictionary = response.Headers.ToDictionary(header => header.Key, header => header.Value.FirstOrDefault());

                            subReddit.Limits.Used = Convert.ToInt32(headersDictionary["x-ratelimit-used"]);
                            subReddit.Limits.Remaining = Convert.ToDouble(headersDictionary["x-ratelimit-remaining"]);
                            subReddit.Limits.Reset = Convert.ToInt32(headersDictionary["x-ratelimit-reset"]);

                            var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                            var wl = new List<RedditPost>();
                            Parallel.ForEach(json["data"]["children"], post =>
                            {
                                var postData = post["data"];
                                var wp = new RedditPost
                                {
                                    Id = postData["id"].ToString(),
                                    Author = postData["author"].ToString(),
                                    BeginningUps = (int)postData["ups"],
                                    Title = postData["title"].ToString()
                                };


                                var current = subReddit.Posts.FirstOrDefault(x => x.Id == postData["id"].ToString());
                                if (null != current)
                                {
                                    current.CurrentUps = (int)postData["ups"];
                                    current.UpVotes = current.CurrentUps - current.BeginningUps;
                                    wl.Add(current);
                                }
                                else
                                {
                                    wl.Add(wp);
                                }
                            });

                            subReddit.Posts = wl.OrderByDescending(x => x.UpVotes).ToList();
                            subReddit.TopAuthor = (wl.GroupBy(n => n.Author)
                                .Select(authors => new { Name = authors.Key, Count = authors.Count() })
                                .OrderByDescending(c => c.Count))
                                .Select(n => n.Name).FirstOrDefault() ?? string.Empty;
                            subReddit.TopPost = wl.OrderByDescending(x => x.UpVotes).FirstOrDefault() ?? new RedditPost();


                            return Ok(subReddit);
                        }
                        else
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                return Unauthorized(response);
                            }
                            return BadRequest(response);
                        }
                    }
                }
                catch(Exception ex)
                {
                    return BadRequest(ex);
                }
            }
        }
    }
}
