using SocialMediaWisLam.Models;

namespace SocialMediaWisLam
{
    public class FriendModel
    {
        public Profile Profile1 { get; set; }

        public Profile Profile2 { get; set; }

        public int AreFriend { get; set; }

        public FriendModel(Profile profile1, Profile profile2, int areFriend)
        {
            Profile1 = profile1;
            Profile2 = profile2;
            AreFriend = areFriend;
        }
    }
}
