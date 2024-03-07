namespace SocialMediaWisLam.Models
{
    public class Video
    {
        public int Id { get; set; }

        public string Caption { get; set; }

        public string Url { get; set; }

        public virtual Post PostOwner { get; set; }

        public virtual Profile ProfileOwner { get; set; }
    }
}
