using Microsoft.EntityFrameworkCore;

namespace SocialMediaWisLam.Models
{
    public class Location
    {
        public int Id { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public virtual ICollection<Profile> Profiles { get; set; }
    }
}
