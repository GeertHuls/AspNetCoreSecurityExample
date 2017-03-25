using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AspNetSecurity.Services
{
    public class TopSecurityStampBasedTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser>
        where TUser : class
    {
        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}