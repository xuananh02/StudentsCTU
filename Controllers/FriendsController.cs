using Elfie.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using System.Linq;
using System.Security.Claims;

namespace SocialMediaWisLam.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly SocialMediaWisLamContext _context;
        private readonly IHubContext<Hubs.ChatHub> _hubContext;

        public FriendsController(SocialMediaWisLamContext context, IHubContext<Hubs.ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }


        public class ViewModel {

            public string UserId { get; set; }

            public IEnumerable<FriendModel> Friends { get; set; }

            public ViewModel(string userId, IEnumerable<FriendModel> friends)
            {
                UserId = userId;
                Friends = friends;
            }
        }

        // GET: Friends
        public async Task<IActionResult> Index(string? searchString)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var friendsQuery = from fr in _context.FriendRelation
                               where fr.User2ID == userId || fr.User1ID == userId
                               where fr.User2ID != fr.User1ID && (fr.AreFriend == 1 || fr.AreFriend == 2)
                               join pr1 in _context.Profile on fr.User1ID equals pr1.Id
                               join pr2 in _context.Profile on fr.User2ID equals pr2.Id
                               select new
                               {
                                   profile1 = pr1,
                                   profile2 = pr2,
                                   AreFriend = fr.AreFriend
                               };

            friendsQuery = friendsQuery.Where(item => item.profile1.Id != userId && (item.profile1.FirstName!.Contains(searchString ?? "") || item.profile1.LastName!.Contains(searchString ?? ""))
                || item.profile2.Id != userId && (item.profile2.FirstName!.Contains(searchString ?? "") || item.profile2.LastName!.Contains(searchString ?? "")));


            var friends = (await friendsQuery.ToListAsync()).Select(item => new FriendModel(item.profile1, item.profile2, item.AreFriend));

            var model = new ViewModel(userId, friends);

            return View(model);
        }

        public class FriendRelationForm
        {
            public string UserId1 { get; set; }

            public string UserId2 { get; set; }

            public int AreFriend { get; set; }
        }

        [HttpGet("GetFriends")]
        public async Task<ActionResult<IEnumerable<FriendModel>>> GetFriends(string? userIdTest, string? searchString, int? pageNumber) {

            var userId = userIdTest ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pageSize = 3;
            var friendsQuery = from fr in _context.FriendRelation
                               where fr.User2ID == userId || fr.User1ID == userId
                               where fr.User2ID != fr.User1ID && fr.AreFriend == 1
                               join pr1 in _context.Profile on fr.User1ID equals pr1.Id
                               join pr2 in _context.Profile on fr.User2ID equals pr2.Id
                               select new
                               {
                                   profile1 = pr1,
                                   profile2 = pr2,
                                   AreFriend = fr.AreFriend
                               };

            var items = friendsQuery.Where(
                    item => item.profile1.Id != userId && (item.profile1.FirstName!.Contains(searchString ?? "") || item.profile1.LastName!.Contains(searchString ?? ""))
                    || item.profile2.Id != userId && (item.profile2.FirstName!.Contains(searchString ?? "") || item.profile2.LastName!.Contains(searchString ?? ""))
                ).AsNoTracking().Skip(((pageNumber ?? 1) - 1) * pageSize).Take(pageSize).Select(item => item.profile1.Id != userId ? item.profile1 : item.profile2);

            return Ok(items);
        }


        [HttpPost("AddFriend")]
        public async Task<ActionResult<string>> AddFriend([FromForm] FriendRelationForm friendRelationForm)
        {
            var user1 = _context.Profile.FirstOrDefault(item => item.Id == friendRelationForm.UserId1);
            var user2 = _context.Profile.FirstOrDefault(item => item.Id == friendRelationForm.UserId2);


            if (user1 == null || user2 == null)
            {
                return BadRequest("Invalid UserId1=" + friendRelationForm.UserId1 + ", UserId2=" + friendRelationForm.UserId2);
            }

            var friendRelation = _context.FriendRelation.FirstOrDefault(item =>
                (item.User1ID == friendRelationForm.UserId1 && item.User2ID == friendRelationForm.UserId2) ||
                (item.User1ID == friendRelationForm.UserId2 && item.User2ID == friendRelationForm.UserId1)
            );

            if (friendRelation == null)
            {
                var newFriendRelation = Activator.CreateInstance<FriendRelation>();
                newFriendRelation.User1ID = user1.Id;
                newFriendRelation.User2ID = user2.Id;
                newFriendRelation.AreFriend = friendRelationForm.AreFriend;
                _context.FriendRelation.Add(newFriendRelation);
            }
            else if (friendRelationForm.AreFriend == 0)
            {
                _context.FriendRelation.Remove(friendRelation);
            }
            else {
                friendRelation.AreFriend = 1;
                _context.FriendRelation.Update(friendRelation);
            }

            _context.SaveChanges();
            string previousUrl = Request.Headers["Referer"].ToString();
            return Redirect(previousUrl);
        }

        public class ModelMessage {
            public string UserId { get; set; }

            public string Message { get; set; }

        }

        [HttpPost("SendChat")]
        public async Task<ActionResult<Messenger>> SendChat([FromBody] ModelMessage modelMessage) {
            if (modelMessage.UserId.IsNullOrEmpty() || modelMessage.Message.IsNullOrEmpty()) {
                return BadRequest();
            }

            string userId1 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messenger = Activator.CreateInstance<Messenger>();

            messenger.UserId1 = userId1;
            messenger.UserId2 = modelMessage.UserId;
            messenger.Message = modelMessage.Message;

            _context.Add(messenger);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.User(modelMessage.UserId).SendAsync("ReceiveMessageVer2", userId1, modelMessage.Message);

            return Ok(messenger);
        }

        [HttpGet("GetChats")]
        public async Task<ActionResult<IEnumerable<Messenger>>> GetChats(string userId)
        {
            string userId1 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messengers = _context.Messenger.Where(item => item.UserId2 == userId && item.UserId1 == userId1 || item.UserId2 == userId1 && item.UserId1 == userId).ToList();

            return Ok(messengers);
        }

        [HttpGet("GetNotificationPost")]
        public async Task<ActionResult<IEnumerable<Models.Notification>>> GetNotificationPost(string? userIdTest) {
            var userId = (userIdTest.IsNullOrEmpty()) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : userIdTest;

            if (userId.IsNullOrEmpty())
            {
                return BadRequest();
            }
            var notifications = _context.Notification.Where(item => item.UserId == userId).ToList();
            return Ok(notifications);
        }

        [HttpDelete("DeleteNotificationPost/{userIdUrl}-{postIdUrl}")]
        public async Task<ActionResult<Notification>> DeleteNotificationPost(string userIdUrl, int postIdUrl, string? userIdTest) {
            var userId = (userIdTest.IsNullOrEmpty()) ? User.FindFirstValue(ClaimTypes.NameIdentifier) : userIdTest;
            var notification = await _context.Notification.Where(item => item.UserId == userIdUrl && item.PostId == postIdUrl).FirstOrDefaultAsync();

            if (notification == null) {
                return BadRequest();
            }
            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
    }
}
