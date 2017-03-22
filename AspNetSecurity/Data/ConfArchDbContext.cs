using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetSecurity.Data
{
    /// <summary>
    /// After updating the user, remove the old context, then re-create the new one, using:
    /// > dotnet ef migrations add initial --context ConfArchDbContext
    /// 
    /// > dotnet ef migrations remove
    /// 
    /// > dotnet ef database update
    /// </summary>
    public class ConfArchDbContext : IdentityDbContext<ConfArchUser>
    {
        public ConfArchDbContext(DbContextOptions<ConfArchDbContext> options)
            : base(options)
        {
        }
    }
}