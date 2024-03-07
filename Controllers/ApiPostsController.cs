using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;

namespace SocialMediaWisLam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPostsController : ControllerBase
    {
        private readonly SocialMediaWisLamContext _context;
        private readonly IHubContext<SocialMediaWisLam.Hubs.ChatHub> _hubContext;

        public ApiPostsController(SocialMediaWisLamContext context, IHubContext<Hubs.ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }


        public class ViewModelPost {

            public int Id { get; set; }

            public string Description { get; set; }

            public DateTime UpdatedDate { get; set; }

            public Profile ProfileOwner { get; set; }

            public ICollection<Video> Videos { get; set; }

            public ICollection<Photo> Photos { get; set; }

            public int NumOfLike { get; set; }

            public bool IsLike { get; set; }

            public ViewModelPost(int id, string description, DateTime updatedDate, Profile profileOwner, ICollection<Video> videos, ICollection<Photo> photos, int numOfLike, bool isLike) {
                Id = id;
                Description = description;
                UpdatedDate = updatedDate;
                ProfileOwner = profileOwner;
                Photos = photos;
                Videos = videos;
                NumOfLike = numOfLike;
                IsLike = isLike;
            }

        }

        // GET: api/ApiPosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewModelPost>>> GetPost(int? pageNumber)
        {
            int pageSize = 3;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";
            var posts = from p in _context.Post
                        join pr in _context.Profile on p.ProfileOwner equals pr
                        select new {
                            Id = p.Id,
                            Description = p.Description,
                            UpdatedDate = p.UpdatedDate,
                            ProfileOwner = pr,
                            CreatedDate = p.CreatedDate,
                            Videos = (
                                from v in _context.Video
                                where v.PostOwner.Id == p.Id
                                select v
                            ).ToList(),
                            Photos = (
                                from pt in _context.Photo
                                where pt.PostOwner.Id == p.Id
                                select pt
                            ).ToList()
                        };

            posts = posts.OrderByDescending(p => p.Id);

            var postsVer = await posts.AsNoTracking().Skip(((pageNumber ?? 1) - 1) * pageSize).Take(pageSize).Select(item =>
                    (new ViewModelPost(item.Id, 
                    item.Description, 
                    item.UpdatedDate, 
                    item.ProfileOwner, 
                    item.Videos, 
                    item.Photos, 
                    _context.Emotion.Where(item1 => item1.PostId == item.Id).Count(),
                    _context.Emotion.Where(item1 => item1.UserId == userId && item.Id == item1.PostId).FirstOrDefault() != null)))
                .ToListAsync();

            return postsVer;
        }

        // GET: api/ApiPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Post.FindAsync(id);


            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/ApiPosts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        public class PostForm {
        
            public string Description { set; get; }

        }

        // POST: api/ApiPosts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost([FromBody] PostForm post)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profileOwner = _context.Profile.Find(userId);
            var friendsQuery = from fr in _context.FriendRelation
                               where fr.User2ID == userId || fr.User1ID == userId
                               where fr.User2ID != fr.User1ID && (fr.AreFriend == 1 || fr.AreFriend == 2)
                               join pr1 in _context.Profile on fr.User1ID equals pr1.Id
                               join pr2 in _context.Profile on fr.User2ID equals pr2.Id
                               select new
                               {
                                   Friend = (fr.User1ID != userId) ? pr1 : pr2,
                                   AreFriend = fr.AreFriend
                               };
            var friends = await friendsQuery.ToListAsync();

            DateTime createDate = DateTime.UtcNow;
            Post postInstance = Activator.CreateInstance<Post>();

            postInstance.CreatedDate = createDate;
            postInstance.UpdatedDate = createDate;
            postInstance.ProfileOwner = profileOwner;
            postInstance.Description = post.Description;

            _context.Post.Add(postInstance);
            await _context.SaveChangesAsync();

            foreach (var friend in friends) {
                // Ghi postId len co so du lieu notication neu la ban be cua uerid
                Notification notication = Activator.CreateInstance<Notification>();
                notication.PostId = postInstance.Id;
                notication.UserId = friend.Friend.Id;
                notication.Description = "New Post of " + profileOwner.FirstName + " " + profileOwner.LastName;
                _context.Notification.Add(notication);
                _context.SaveChanges();

                await _hubContext.Clients.User(friend.Friend.Id).SendAsync("NotificationPost", notication.UserId, notication.PostId, notication.Description);
            }

            

            return CreatedAtAction("GetPost", new { id = postInstance.Id }, postInstance);
        }

        [HttpPost("Images")]
        public async Task<ActionResult<String>> Images()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var idPost = int.Parse(Request.Form["id"]);
            var post = _context.Post.Find(idPost);
            var user = _context.Profile.Find(userId);

            var files = Request.Form.Files;
            var countFile = 1;

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files selected.");
            }

            foreach (var file in files) {
                if (file.Length > 0)
                {
                    var photo = Activator.CreateInstance<Photo>();
                    photo.Caption = file.FileName;
                    photo.ProfileOwner = user;
                    photo.PostOwner = post;
                    

                    var nameFile = userId + "-" + idPost + "-" + countFile.ToString() + "-" + file.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "source", "images", nameFile);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); // Save the file to disk
                    }

                    int charPos = filePath.IndexOf("source\\images\\");
                    filePath = filePath.Substring(charPos);
                    photo.Url = filePath;

                    countFile += 1;

                    await _context.AddAsync(photo);
                    await _context.SaveChangesAsync();
                    

                    /*var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "source", "images", countFile.ToString() + file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); // Save the file to disk
                    }

                    countFile += 1;*/
                }
            }

            return Ok("File uploaded successfully. With PostId = " + idPost + " UserId = " + userId);
        }

        [HttpPost("Videos")]
        public async Task<ActionResult<String>> Videos()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var idPost = int.Parse(Request.Form["id"]);
            var post = _context.Post.Find(idPost);
            var user = _context.Profile.Find(userId);

            var files = Request.Form.Files;
            var countFile = 1;

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files selected.");
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var video = Activator.CreateInstance<Video>();
                    video.Caption = file.FileName;
                    video.ProfileOwner = user;
                    video.PostOwner = post;
                    video.Url = "";


                    var nameFile = userId + "-" + idPost + "-" + countFile.ToString() + "-" + file.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "source", "videos", nameFile);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); // Save the file to disk
                    }

                    int charPos = filePath.IndexOf("source\\videos\\");
                    filePath = filePath.Substring(charPos);
                    video.Url = filePath;

                    countFile += 1;
                    await _context.Video.AddAsync(video);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok("File uploaded successfully. With PostId = " + idPost);
        }

        // DELETE: api/ApiPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            List<Photo> photos = _context.Photo.Where(item => item.PostOwner.Id == post.Id).ToList();
            foreach (var photo in photos)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.Url);
                Console.WriteLine("Photo " + filePath);
                if (System.IO.File.Exists(filePath))
                {
                    // Delete the file
                    System.IO.File.Delete(filePath);
                    Console.WriteLine(filePath + " deleted successfully.");
                }
                else
                {
                    Console.WriteLine(filePath + " does not exist.");
                }
            }
            List<Video> videos = _context.Video.Where(item => item.PostOwner.Id == post.Id).ToList();
            foreach (var video in videos)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.Url);
                Console.WriteLine("Video " + filePath);
                if (System.IO.File.Exists(filePath))
                {
                    // Delete the file
                    System.IO.File.Delete(filePath);
                    Console.WriteLine(filePath + " deleted successfully.");
                }
                else
                {
                    Console.WriteLine(filePath + " does not exist.");
                }
            }
            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class FormEmotion {
            public int PostId { get; set; }

            public string UserId { get; set; }

            public int Emoji { get; set; }
        }


        [HttpPost("Emotion")]
        public async Task<ActionResult<Emotion>> PostEmotion([FromBody]FormEmotion emotionPara) {
            var emotionExist = await _context.Emotion.Where(item => item.UserId == emotionPara.UserId && item.PostId == emotionPara.PostId).FirstOrDefaultAsync();
            if (emotionExist == null)
            {
                var emotion = Activator.CreateInstance<Emotion>();
                emotion.PostId = emotionPara.PostId;
                emotion.PostOwner = await _context.Post.FindAsync(emotionPara.PostId);
                emotion.UserId = emotionPara.UserId;
                emotion.ProfileOwner = await _context.Profile.FindAsync(emotionPara.UserId);
                emotion.Emoji = emotionPara.Emoji;
                await _context.Emotion.AddAsync(emotion);
                await _context.SaveChangesAsync();
                return emotion;
            }
            else {
                _context.Emotion.Remove(emotionExist);
                await _context.SaveChangesAsync();
            }

            emotionExist.Emoji = -1;
            return emotionExist;
        }

        public class FormSavedPost {
            public int PostId { get; set; }

            public string UserIdNotOwner { get; set; }

            public FormSavedPost(int postId, string userIdNotOwner) {
                PostId = postId;
                UserIdNotOwner = userIdNotOwner;
            }
        }

        [HttpPost("SavedPost")]
        public async Task<ActionResult<SavedPost>> SavedPost([FromBody] FormSavedPost formSavedPost) {
            var savePostExist = await _context.SavedPost
                .Where(item => item.PostId ==  formSavedPost.PostId && item.UserIdNotOwner == formSavedPost.UserIdNotOwner).FirstOrDefaultAsync();
            if (savePostExist == null)
            {
                var savedPost = Activator.CreateInstance<SavedPost>();
                savedPost.PostId = formSavedPost.PostId;
                savedPost.UserIdNotOwner = formSavedPost.UserIdNotOwner;
                savedPost.ProfileNotOwner = await _context.Profile.FindAsync(savedPost.UserIdNotOwner);
                savedPost.PostSaved = await _context.Post.FindAsync(savedPost.PostId);
                await _context.AddAsync(savedPost);
                await _context.SaveChangesAsync();
                return savedPost;
            }
            else {
                _context.SavedPost.Remove(savePostExist);
                await _context.SaveChangesAsync();
            }

            return savePostExist;
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }
    }
}
