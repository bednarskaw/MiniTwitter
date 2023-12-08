using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcClientMTwitter.Models
{
    public class TweetsViewModel
    {
        public IEnumerable<Tweet> LastNTweets { get; set; }
    }

}
