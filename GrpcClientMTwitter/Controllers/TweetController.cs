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

namespace GrpcClientMTwitter.Controllers
{
    [Authorize]
    public class TweetController : Controller
    {
        private readonly List<GrpcClientMTwitter.Models.Tweet> _tweets = new List<GrpcClientMTwitter.Models.Tweet>();
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        private readonly GrpcServiceMTwitter.Tweet.TweetClient _tweetClient;
        public TweetController(GrpcServiceMTwitter.Tweet.TweetClient tweetClient)
        {
            _tweetClient = tweetClient ?? throw new ArgumentNullException(nameof(tweetClient));
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Fetch the last N tweets from the server
            var lastNTweetsRequest = new GetLastNTweetsRequest { Count = 5 }; // Adjust the count as needed
            var lastNTweetsResponse = _tweetClient.GetLastNTweets(lastNTweetsRequest);

            // Convert the gRPC TweetItem to the client's TweetItem
            var tweetsViewModel = new TweetsViewModel
            {
                LastNTweets = lastNTweetsResponse.Tweets.Select(t => new Models.Tweet
                {
                    // Map properties accordingly
                    Content = t.Content,
                    UserName = t.UserName,
            
                    // Map other properties
                })
            };

            return View(tweetsViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostTweet(string content)
        {
            // Validate and save the tweet locally
            var tweet = new Models.Tweet
            {
                Id = _tweets.Count + 1,
                Content = content,
                UserName = User.Identity.Name
            };

            _tweets.Add(tweet);

            // Make an asynchronous gRPC call to post the tweet to the server
            await PostTweetToGrpc(tweet);

            // Redirect to the tweet list after posting
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
