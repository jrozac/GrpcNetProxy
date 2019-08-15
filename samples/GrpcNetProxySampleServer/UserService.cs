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
        public Task<User> GetUser(UserFilter filter, CancellationToken token = default)
        {
            return Task.FromResult(new User { Id = filter.Id, Name = $"User {filter.Id}" });
        }

        /// <summary>
        /// Get user and wait 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> GetUserAndWait(UserFilter filter, CancellationToken token = default)
        {
            try
            {
                await Task.Delay(5 * 60 * 1000, token);
            }
            catch (TaskCanceledException)
            {
                return await Task.FromResult<User>(null);
            }
            return await Task.FromResult(new User { Id = filter.Id, Name = $"User {filter.Id}" });
        }

    }
}
