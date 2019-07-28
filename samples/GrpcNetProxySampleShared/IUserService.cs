using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxySampleShared
{

    /// <summary>
    /// User service interface
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> GetUser(UserFilter filter, CancellationToken token);
    }
}
