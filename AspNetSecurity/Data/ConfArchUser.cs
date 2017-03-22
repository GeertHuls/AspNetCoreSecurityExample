using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AspNetSecurity.Data
{
    public class ConfArchUser : IdentityUser
    {
        public DateTime BirthDate { get; set; }
    }
}