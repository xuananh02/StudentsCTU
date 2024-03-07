using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaWisLam.Models
{
    [PrimaryKey(nameof(PostId), nameof(UserId))]
    public class Emotion
    {
        [Key]
        [ForeignKey("PostOwner")]
        public int PostId { get; set; }

        [Key]
        [ForeignKey("ProfileOwner")]
        public string UserId { get; set; }

        public int Emoji { get; set; }

        public virtual Post PostOwner { get; set; }

        public virtual Profile ProfileOwner { get; set; }
    }
}
