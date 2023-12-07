using Grpc.Core;
using GrpcServiceMTwitter.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServiceMTwitter.Services
{
    public class TweetService : Tweet.TweetBase
    {
        private readonly TweetDbContext _dbContext;

        public TweetService(TweetDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public override Task<TweetResponse> PostTweet(TweetRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Tweet received on the server from {request.UserName}: {request.Content}");
            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user == null)
            {
                return Task.FromResult(new TweetResponse { Message = "User not found" });
            }
            if (request.Content.Length > 80)
            {
                return Task.FromResult(new TweetResponse
                {
                    Message = "Tweet too long"
                });
            }
            var tweet = new Models.Tweet
            {
                Content = request.Content,
                User = user
            };

            _dbContext.Tweets.Add(tweet);
            _dbContext.SaveChanges();

            return Task.FromResult(new TweetResponse
            {
                Message = "Tweet posted successfully"
            });
        }

        public override Task<GetLastNTweetsResponse> GetLastNTweets(GetLastNTweetsRequest request, ServerCallContext context)
        {
            var tweets = _dbContext.Tweets
                .OrderByDescending(t => t.Id)
                .Take(request.Count >= 0? request.Count:0)
                .Select(t => new TweetRequest { Content = t.Content, UserName = t.User.UserName }) // Assuming 'Content' is the property that holds the tweet message
                .ToList();

            var response = new GetLastNTweetsResponse
            {
                Tweets = { tweets }
            };

            return Task.FromResult(response);
        }

        public override Task<AuthenticationResponse> AuthenticateUser(UserCredentials request, ServerCallContext context)
        {
            var user = _dbContext.Users
                .FirstOrDefault(u => u.UserName == request.Username && u.Password == request.Password);

            int userId = 0;
            if (user != null)
            {
                userId = user.Id;
            }
            return Task.FromResult(new AuthenticationResponse
            {
                IsAuthenticated = user != null,
                Id = userId
            });
        }

        public override Task<GetLastNTweetsResponse> GetAllYourPost(GetLastNTweetsRequest request, ServerCallContext context)
        {
            var result = _dbContext.Tweets
             .Where(t => t.User.Id == request.Count)
             .Select(t => new TweetRequest
             {
                 Content = t.Content,
                 UserName = t.User.UserName
                 
             });

            return Task.FromResult(new GetLastNTweetsResponse
            {
                Tweets = { result }
            });
        }

    }
}
