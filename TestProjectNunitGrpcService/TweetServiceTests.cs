using GrpcServiceMTwitter;
using NUnit.Framework;
using Grpc.Core;
using GrpcServiceMTwitter.Data;
using GrpcServiceMTwitter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Moq;

namespace TestProjectNunitGrpcService
{
    public class Tests
    {
        private TweetDbContext _dbContext;
        private TweetService _tweetService;
        private SqliteConnection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<TweetDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new TweetDbContext(options);
            _dbContext.Database.EnsureCreated(); // Create the database schema
            _tweetService = new TweetService(_dbContext);
        }

        [Test]
        public void PostTweet_UserNotFound_ReturnsUserNotFoundMessage()
        {
            var request = new TweetRequest
            {
                UserName = "NonExistentUser",
                Content = "Test tweet content"
            };

            // Act
            var response = _tweetService.PostTweet(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual("User not found", response.Message);
        }
        [Test]
        public void PostTweet_UserFound_TweetPostedSuccessfully()
        {
            // Arrange
            var user = new GrpcServiceMTwitter.Models.User { UserName = "TestUser" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var request = new TweetRequest
            {
                UserName = "TestUser",
                Content = "Test tweet content"
            };

            // Act
            var response = _tweetService.PostTweet(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual("Tweet posted successfully", response.Message);
        }
        [Test]
        public void PostTweet_TooLongMessage()
        {
            // Arrange
            var user = new GrpcServiceMTwitter.Models.User { UserName = "TestUser" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var request = new TweetRequest
            {
                UserName = "TestUser",
                Content = "Test tweet content, \nTest tweet content, \nTest tweet content, \nTest tweet content, \nTest tweet content, \nTest tweet content, \n"
            };

            // Act
            var response = _tweetService.PostTweet(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual("Tweet too long", response.Message);
        }

        [Test]
        public void GetLastNTweets_ReturnsCorrectNumberOfTweets()
        {
            // Arrange
            var user = new GrpcServiceMTwitter.Models.User { UserName = "TestUser" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var tweet1 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 1", User = user };
            var tweet2 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 2", User = user };
            _dbContext.Tweets.AddRange(tweet1, tweet2);
            _dbContext.SaveChanges();

            var request = new GetLastNTweetsRequest { Count = 2 };

            // Act
            var response = _tweetService.GetLastNTweets(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual(2, response.Tweets.Count);
            Assert.AreEqual("Tweet 2", response.Tweets[0].Content);
            Assert.AreEqual("TestUser", response.Tweets[0].UserName);
            Assert.AreEqual("Tweet 1", response.Tweets[1].Content);
            Assert.AreEqual("TestUser", response.Tweets[1].UserName);
        }

        [Test]
        public void GetLastNTweets_RequestTooManyTweets()
        {
            // Arrange
            var user = new GrpcServiceMTwitter.Models.User { UserName = "TestUser" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var tweet1 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 1", User = user };
            var tweet2 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 2", User = user };
            _dbContext.Tweets.AddRange(tweet1, tweet2);
            _dbContext.SaveChanges();

            var request = new GetLastNTweetsRequest { Count = 100000000 };

            // Act
            var response = _tweetService.GetLastNTweets(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual(2, response.Tweets.Count);
            Assert.AreEqual("Tweet 2", response.Tweets[0].Content);
            Assert.AreEqual("TestUser", response.Tweets[0].UserName);
            Assert.AreEqual("Tweet 1", response.Tweets[1].Content);
            Assert.AreEqual("TestUser", response.Tweets[1].UserName);
        }

        [Test]
        public void GetLastNTweets_PassWrongNumberOfTweets()
        {
            // Arrange
            var user = new GrpcServiceMTwitter.Models.User { UserName = "TestUser" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var tweet1 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 1", User = user };
            var tweet2 = new GrpcServiceMTwitter.Models.Tweet { Content = "Tweet 2", User = user };
            _dbContext.Tweets.AddRange(tweet1, tweet2);
            _dbContext.SaveChanges();

            var request = new GetLastNTweetsRequest { Count = -1 };

            // Act
            var response = _tweetService.GetLastNTweets(request, Mock.Of<ServerCallContext>()).Result;

            // Assert
            Assert.AreEqual(0, response.Tweets.Count);
        }
    }
}