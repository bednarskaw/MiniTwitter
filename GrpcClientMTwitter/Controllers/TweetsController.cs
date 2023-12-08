using Grpc.Net.Client;
using GrpcServiceMTwitter;
using GrpcClientMTwitter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using System.Security.Claims;

namespace GrpcClientMTwitter.Controllers
{
    [Authorize]
    public class TweetsController : Controller
    {
        private readonly List<GrpcClientMTwitter.Models.Tweet> _tweets = new List<GrpcClientMTwitter.Models.Tweet>();
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        private readonly GrpcServiceMTwitter.Tweet.TweetClient _tweetClient;
        public TweetsController(GrpcServiceMTwitter.Tweet.TweetClient tweetClient)
        {
            _tweetClient = tweetClient ?? throw new ArgumentNullException(nameof(tweetClient));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cli = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var lastNTweetsRequest = new GetLastNTweetsRequest { Count = cli}; // Adjust the count as needed
            var lastNTweetsResponse = _tweetClient.GetAllYourPost(lastNTweetsRequest);

            // Convert the gRPC TweetItem to the client's TweetItem
            var tweetsViewModel = new TweetsViewModel
            {
                LastNTweets = lastNTweetsResponse.Tweets.Select(t => new Models.Tweet
                {
                    Content = t.Content,
                    UserName = t.UserName,
                })
            };

            return View(tweetsViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostTweet(string content)
        {

            var tweet = new Models.Tweet
            {
                Id = _tweets.Count + 1,
                Content = content,
                UserName = User.Identity.Name
            };

            _tweets.Add(tweet);

            await PostTweetToGrpc(tweet);

            return RedirectToAction(nameof(Index));
        }
        private async Task PostTweetToGrpc(Models.Tweet tweet)
        {

            try
            {

                var request = new TweetRequest
                {
                    Content = tweet.Content,
                    UserName = tweet.UserName
                };

                var response = await _tweetClient.PostTweetAsync(request);
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"gRPC error: {ex.Status.Detail}");
            }
        }


    }
}
