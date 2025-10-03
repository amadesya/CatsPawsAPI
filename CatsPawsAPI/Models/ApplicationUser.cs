using Microsoft.AspNetCore.Identity;

namespace CatsPawsAPI.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int? RoleId { get; set; }
    }
}
