using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaWisLam.Models
{

    public class Profile : IdentityUser
    { 

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? PictureUrl { get; set; }

        public string? CoverPictureUrl { get; set; }

        public string? AboutMe { get; set; }

        public int Gender { get; set; }

        public string? street { get; set; }

        public virtual Location? LocationOwner { get; set; }


    }
}
