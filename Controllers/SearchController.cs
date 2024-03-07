using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace SocialMediaWisLam.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly SocialMediaWisLamContext _context;

        public SearchController(SocialMediaWisLamContext context) {
            _context = context;
        }

        public class ViewModel {
            public IEnumerable<Models.Post> Posts { get; set; }

            public IEnumerable<Models.Profile> Profiles { get; set; }

            public IEnumerable<FriendModel> Friends { get; set; }

            public string UserId { get; set; }


            public ViewModel(List<Models.Post> _Posts, List<Models.Profile> _Profiles, IEnumerable<FriendModel> _Friends, string _UserId)
            {
                Posts = _Posts;
                Profiles = _Profiles;
                UserId = _UserId;
                Friends = _Friends;
            }
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var posts = from pst in _context.Post
                            select pst;
            var profiles = from pr in _context.Profile
                           where pr.Id != userId
                           select pr;
            var friendsQuery = from fr in _context.FriendRelation
                                    where fr.User2ID == userId || fr.User1ID == userId
                                    where fr.User2ID != fr.User1ID && (fr.AreFriend == 1 || fr.AreFriend == 2)
                                    join pr1 in _context.Profile on fr.User1ID equals pr1.Id
                                    join pr2 in _context.Profile on fr.User2ID equals pr2.Id
                                        select new {
                                            profile1 = pr1,
                                            profile2 = pr2,
                                            AreFriend = fr.AreFriend
                                        };


            if (!string.IsNullOrEmpty(searchString))
            {
                Console.WriteLine("searchString here:" + searchString);
                profiles = profiles.Where(item => item.LastName!.Contains(searchString) || item.FirstName!.Contains(searchString));

                friendsQuery = friendsQuery.Where(item => item.profile1.Id != userId && (item.profile1.FirstName!.Contains(searchString) || item.profile1.LastName!.Contains(searchString))
                    || item.profile2.Id != userId && (item.profile2.FirstName!.Contains(searchString) || item.profile2.LastName!.Contains(searchString)));

                posts = posts.Where(item => item.Description!.Contains(searchString));
            }
            else {
                string previousUrl = Request.Headers["Referer"].ToString();
                return Redirect(previousUrl);
            }

            var newPosts = posts.ToList();
            newPosts = newPosts.Select(item => {
                item.Photos = _context.Photo.Where(item1 => item1.PostOwner.Id == item.Id).ToList();
                item.Videos = _context.Video.Where(item1 => item1.PostOwner.Id == item.Id).ToList();
                return item;
            }).ToList();


            var friends = friendsQuery.ToList();
            foreach (var item in friends) {
                profiles = profiles.Where(item1 => !(item1.Id == item.profile1.Id || item1.Id == item.profile2.Id));
            }


            var newProfiles = profiles.ToList();
            var newFriendProfiles = friends.Select(item => {
                return new FriendModel(item.profile1, item.profile2, item.AreFriend);
            });

            var viewModel = new ViewModel(newPosts, newProfiles,
                newFriendProfiles, userId);

            return View(viewModel);

        }
    }
}
