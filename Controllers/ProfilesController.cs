using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using static SocialMediaWisLam.Controllers.SavedPostsController;

namespace SocialMediaWisLam.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly SocialMediaWisLamContext _context;
        

        public ProfilesController(SocialMediaWisLamContext context)
        {
            _context = context;

        }

        // GET: Profiles
        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 3;

            var profiles = from p in _context.Profile
                           select p;

            profiles = profiles.OrderByDescending(p => p.Id);

            return View(await PaginatedList<Profile>.CreateAsync(profiles.AsNoTracking(), pageNumber ?? 1, pageSize));
            //return View(await _context.Profile.ToListAsync());
        }

        public class DetailUser {

            public IEnumerable<PostIQueryable> PostsDetail { get; set; }

            public Profile ProfileDetail { get; set; }

            public DetailUser(IEnumerable<PostIQueryable> postsDetail, Profile profileDetail) {
                PostsDetail = postsDetail;
                ProfileDetail = profileDetail;
            }
        }

        // GET: Profiles/Details/5
        public async Task<IActionResult> Details(String? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.Profile
                .FirstOrDefaultAsync(m => m.Id.Equals(id));
            if (profile == null)
            {
                return NotFound();
            }
            var posts = await _context.Post.Where(item => item.ProfileOwner.Id == profile.Id)
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

            var detailUser = new DetailUser(posts, profile);

            return View(detailUser);
        }

        // GET: Profiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Birthday,FirstName,LastName,PictureUrl,AboutMe,Gender")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        // GET: Profiles/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profile.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Birthday,FirstName,LastName,PictureUrl,AboutMe,Gender")] Profile profile)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(profile.Id))
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
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profile = await _context.Profile.FindAsync(id);
            if (profile != null)
            {
                _context.Profile.Remove(profile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfileExists(string id)
        {
            return _context.Profile.Any(e => e.Id == id);
        }
    }
}
