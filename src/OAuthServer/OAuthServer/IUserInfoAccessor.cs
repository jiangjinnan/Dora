using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    public interface IUserInfoAccessor<TUser>
    {
        Task<TUser> GetUserInfoAsync(UserInfoAccessingContext context);
    }
}
