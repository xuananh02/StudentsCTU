using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Models;

namespace SocialMediaWisLam.Data
{
    public class SocialMediaWisLamContext : IdentityDbContext
    {
        public SocialMediaWisLamContext (DbContextOptions<SocialMediaWisLamContext> options)
            : base(options)
        {
        }

        public DbSet<SocialMediaWisLam.Models.Profile> Profile { get; set; } = default!;
        public DbSet<SocialMediaWisLam.Models.Location> Location { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.FriendRelation> FriendRelation { get; set; } = default!;
        public DbSet<SocialMediaWisLam.Models.Post> Post { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.Photo> Photo { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.Video> Video { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.Notification> Notification { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.Messenger> Messenger { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.Emotion> Emotion { get; set; } = default!;

        public DbSet<SocialMediaWisLam.Models.SavedPost> SavedPost { get; set; } = default!;

    }
}
