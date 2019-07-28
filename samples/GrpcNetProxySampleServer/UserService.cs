using GrpcNetProxySampleShared;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxySampleServer
{
    /// <summary>
    /// User service implementation
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<User> GetUser(UserFilter filter, CancellationToken token)
        {
            return Task.FromResult(new User { Id = filter.Id, Name = $"User {filter.Id}" });
        }
    }
}
