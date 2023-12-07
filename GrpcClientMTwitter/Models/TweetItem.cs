using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcClientMTwitter.Models
{
    public class TweetItem
    {
        public string Content { get; set; }
        public string UserName { get; set; }

        public int Id { get; set; }

    }
}
