using System.ComponentModel.DataAnnotations;

namespace SocialMediaWisLam.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdatedDate { get; set; }

        public virtual Profile? ProfileOwner { get; set; }

        public virtual ICollection<Video> Videos { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }
    }
}
