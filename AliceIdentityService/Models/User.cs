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

        public string Name => $"{FirstName} {LastName}";
    }
}
