
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using static SocialMediaWisLam.Controllers.ApiPostsController;
using System.Security.Claims;



namespace SocialMediaWisLam.Controllers
{
    public class PostsController : Controller
    {
        private readonly SocialMediaWisLamContext _context;

        public PostsController(SocialMediaWisLamContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Post.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";
            Post post = await _context.Post.FirstOrDefaultAsync(m => m.Id == id);
            int countEmotion = await _context.Emotion.Where(item1 => item1.PostId == post.Id).CountAsync();
            Emotion emotion = await _context.Emotion.Where(item1 => item1.UserId == userId && item1.PostId == post.Id).FirstOrDefaultAsync();
            bool isLike = emotion != null;

            post.Videos = await _context.Video.Where(item => item.PostOwner.Id == post.Id).ToListAsync();
            post.Photos = await _context.Photo.Where(item => item.PostOwner.Id == post.Id).ToListAsync();
            post.ProfileOwner = await _context.Profile.Where(item => item.Id == userId).FirstAsync();

            var model = new ViewModelPost(post.Id, post.Description, post.UpdatedDate, post.ProfileOwner, post.Videos, post.Photos, countEmotion, isLike);

            if (post == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Description,CreatedDate,UpdatedDate")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Description,CreatedDate,UpdatedDate")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Post.FindAsync(id);
            if (post != null)
            {
                List<Photo> photos = _context.Photo.Where(item => item.PostOwner.Id == post.Id).ToList();
                foreach (var photo in photos) {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.Url);
                    Console.WriteLine("Photo "+ filePath);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }
    }
}
