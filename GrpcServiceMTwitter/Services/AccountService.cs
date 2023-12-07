using Grpc.Core;
using GrpcServiceMTwitter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServiceMTwitter.Models
{
    public class AccountService : Account.AccountBase
    {
        private readonly TweetDbContext _dbContext;

        public AccountService(TweetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task<UserExistsResponse> CheckUserExists(UserExistsRequest request, ServerCallContext context)
        {
            // Check if the user exists in the database
            bool userExists = _dbContext.Users.Any(u => u.UserName == request.Username);

            return Task.FromResult(new UserExistsResponse
            {
                Exists = userExists
            });
        }
    }


}
