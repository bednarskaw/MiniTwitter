using Grpc.Net.Client;
using System;
using System.Threading.Tasks;
using GrpcServiceMTwitter;

namespace ConsoleAppClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Tweet.TweetClient(channel);

            while (true)
            {
                Console.WriteLine("Choose option: ");
                Console.WriteLine("1. Write Tweet");
                Console.WriteLine("2. Get Tweets");
                string option = Console.ReadLine();

                if (option == "1")
                {
                    Console.WriteLine("What is your Tweet: ");
                    string msg = Console.ReadLine();
                    var input = new TweetRequest { UserName= "Anonymous", Content= msg};
                    var reply = await client.PostTweetAsync(input);
                    Console.WriteLine(reply.Message);
                }
                else if (option == "2")
                {
                    Console.WriteLine("How many Tweets you want to see: ");
                    int count = int.Parse(Console.ReadLine());
                    var input = new GetLastNTweetsRequest { Count = count };
                    var reply = await client.GetLastNTweetsAsync(input);
                    foreach (var msg in reply.Tweets)
                    {
                        Console.WriteLine("\"" + msg.Content + "\" by " + msg.UserName);
                    }
                }
            }

        }
    }
}
