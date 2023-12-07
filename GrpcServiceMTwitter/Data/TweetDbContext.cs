using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServiceMTwitter.Data
{
    public class TweetDbContext : DbContext
    {
        public DbSet<Models.Tweet> Tweets { get; set; }
        public DbSet<Models.User> Users { get; set; }

        public TweetDbContext(DbContextOptions<TweetDbContext> options) : base(options)
        {
        }
    }
}
