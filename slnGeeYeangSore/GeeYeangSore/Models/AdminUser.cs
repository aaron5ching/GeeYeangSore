using Microsoft.AspNetCore.Identity;

namespace GeeYeangSore.Models
{
    public class AdminUser : IdentityUser
    {
        public string RoleLevel { get; set; }
    }
}