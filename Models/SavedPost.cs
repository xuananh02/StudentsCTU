using Microsoft.EntityFrameworkCore;    
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaWisLam.Models
{

    [PrimaryKey(nameof(PostId), nameof(UserIdNotOwner))]
    public class SavedPost
    {
        [Key]
        [ForeignKey("PostSaved")]
        public int PostId { get; set; }

        [Key]
        [ForeignKey("ProfileNotOwner")]
        public string UserIdNotOwner { get; set; }

        public virtual Profile ProfileNotOwner { get; set; }

        public virtual Post PostSaved { get; set; }
    }
}
