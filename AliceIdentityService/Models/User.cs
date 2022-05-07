using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AliceIdentityService.Models
{
    public class User : IdentityUser
    {
        [Required, MaxLength(255), PersonalData]
        public string FirstName { get; set; }

        [Required, MaxLength(255), PersonalData]
        public string LastName { get; set; }

        [Required, MaxLength(255)]
        public string ScreenName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
