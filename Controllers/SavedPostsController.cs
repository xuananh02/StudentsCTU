using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using System.Linq;
using System.Security.Claims;

namespace SocialMediaWisLam.Controllers
{
    [Authorize]
    public class SavedPostsController : Controller
    {
        private readonly SocialMediaWisLamContext _context;

        public SavedPostsController(SocialMediaWisLamContext context) {
            _context = context;
        }

        public class PostIQueryable {
            public IEnumerable<Video> Videos { get; set; }

            public IEnumerable<Photo> Photos { get; set; }

            public DateTime CreatedDate { get; set; }

            public DateTime UpdatedDate { get; set; }

            public string Description { get; set; }

            public Profile Profile { get; set; }

            public int NumOfLike { get; set; }

            public bool IsLike { get; set; }

            public int Id { get; set; }

            public PostIQueryable (IEnumerable<Video> videos, IEnumerable<Photo> photos, DateTime createdDate, DateTime updatedDate, string description, int id, Profile profile, int numOfLike, bool isLike)
            {
                Videos = videos;
                Photos = photos;
                CreatedDate = createdDate;
                UpdatedDate = updatedDate;
                Description = description;
                Id = id;
                Profile = profile;
                NumOfLike = numOfLike;
                IsLike = isLike;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? pageIndex)
        {
            var pageSize = 3;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var savedPosts = from savedPost in _context.SavedPost
                             join post in _context.Post on savedPost.PostId equals post.Id
                             select post;

            /*
                var count = await source.CountAsync();
                var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
                return new PaginatedList<T>(items, count, pageIndex, pageSize);
             */

            var item = await savedPosts
                .Skip((pageIndex ?? 1 - 1) * pageSize)
                .Take(pageSize)
                .Select(item => new PostIQueryable(
                    _context.Video.Where(item1 => item1.PostOwner.Id == item.Id).ToList(), 
                    _context.Photo.Where(item1 => item1.PostOwner.Id == item.Id).ToList(),
                    item.CreatedDate,
                    item.UpdatedDate,
                    item.Description,
                    item.Id,
                    item.ProfileOwner,
                    _context.Emotion.Where(item1 => item1.PostId == item.Id).Count(),
                    _context.Emotion.Where(item1 => item1.PostId == item.Id && item1.UserId == userId).FirstOrDefault() != null
                    ))
                .ToListAsync();
            

            return View(item);
        }
    }
}
