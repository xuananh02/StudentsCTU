using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaWisLam.Models
{
    [PrimaryKey(nameof(UserId), nameof(PostId))]
    public class Notification
    {
        [Key]
        public string UserId { get; set; }

        [Key]
        public int PostId { get; set; }

        public string Description { get; set; }

    }
}
