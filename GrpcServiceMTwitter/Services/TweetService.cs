using Grpc.Core;
using GrpcServiceMTwitter.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServiceMTwitter.Services
{
    // Service class that implements the Tweet service defined in the .proto file
    public class TweetService : Tweet.TweetBase
    {
        private readonly TweetDbContext _dbContext;

        // Constructor to initialize the TweetService with a TweetDbContext
        public TweetService(TweetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Method to handle incoming requests to post a tweet
        public override Task<TweetResponse> PostTweet(TweetRequest request, ServerCallContext context)
        {
            // Log the received tweet information to the server console
            Console.WriteLine($"Tweet received on the server from {request.UserName}: {request.Content}");

            // Check if the user exists in the database
            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user == null)
            {
                // Return a response indicating that the user was not found
                return Task.FromResult(new TweetResponse { Message = "User not found" });
            }

            // Check if the tweet content exceeds the maximum length
            if (request.Content.Length > 80)
            {
                // Return a response indicating that the tweet is too long
                return Task.FromResult(new TweetResponse { Message = "Tweet too long" });
            }

            // Create a new tweet and add it to the database
            var tweet = new Models.Tweet
            {
                Content = request.Content,
                User = user
            };

            _dbContext.Tweets.Add(tweet);
            _dbContext.SaveChanges();

            // Return a response indicating that the tweet was posted successfully
            return Task.FromResult(new TweetResponse { Message = "Tweet posted successfully" });
        }

        // Method to handle incoming requests to get the last N tweets
        public override Task<GetLastNTweetsResponse> GetLastNTweets(GetLastNTweetsRequest request, ServerCallContext context)
        {
            // Retrieve the last N tweets from the database
            var tweets = _dbContext.Tweets
                .OrderByDescending(t => t.Id)
                .Take(request.Count >= 0 ? request.Count : 0)
                .Select(t => new TweetRequest { Content = t.Content, UserName = t.User.UserName })
                .ToList();

            // Create a response with the retrieved tweets
            var response = new GetLastNTweetsResponse
            {
                Tweets = { tweets }
            };

            // Return the response
            return Task.FromResult(response);
        }

        // Method to authenticate a user based on provided credentials
        public override Task<AuthenticationResponse> AuthenticateUser(UserCredentials request, ServerCallContext context)
        {
            // Attempt to find a user with the provided username and password in the database
            var user = _dbContext.Users
                .FirstOrDefault(u => u.UserName == request.Username && u.Password == request.Password);

            // Set the userId based on whether the user was found
            int userId = user != null ? user.Id : 0;

            // Return a response indicating whether the authentication was successful and the user's ID
            return Task.FromResult(new AuthenticationResponse
            {
                IsAuthenticated = user != null,
                Id = userId
            });
        }

        // Method to get all tweets posted by a specific user
        public override Task<GetLastNTweetsResponse> GetAllYourPost(GetLastNTweetsRequest request, ServerCallContext context)
        {
            // Retrieve all tweets posted by the user with the specified ID
            var result = _dbContext.Tweets
             .Where(t => t.User.Id == request.Count)
             .Select(t => new TweetRequest
             {
                 Content = t.Content,
                 UserName = t.User.UserName
             });

            // Create a response with the retrieved tweets
            return Task.FromResult(new GetLastNTweetsResponse
            {
                Tweets = { result }
            });
        }
    }
}