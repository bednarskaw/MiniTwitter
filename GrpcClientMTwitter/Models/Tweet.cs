﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcClientMTwitter.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserName { get; set; }
    }
}
