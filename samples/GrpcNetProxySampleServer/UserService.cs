using GrpcNetProxySampleShared;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxySampleServer
{
    public class UserService : IUserService
    {
        public Task<User> GetUser(UserFilter filter, CancellationToken token)
        {
            return Task.FromResult(new User { Id = filter.Id, Name = $"User {filter.Id}" });
        }
    }
}
