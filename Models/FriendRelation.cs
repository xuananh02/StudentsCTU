using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaWisLam.Models
{
    [PrimaryKey(nameof(User1ID), nameof(User2ID))]
    public class FriendRelation
    {
        [Key]
        public string User1ID { get; set; }

        [Key]
        public string User2ID { get; set; }


        public int AreFriend { get; set; }
    }
}
