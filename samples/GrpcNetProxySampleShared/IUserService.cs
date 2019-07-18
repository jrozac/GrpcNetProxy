using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxySampleShared
{

    public interface IUserService
    {
        Task<User> GetUser(UserFilter filter, CancellationToken token);
    }
}
