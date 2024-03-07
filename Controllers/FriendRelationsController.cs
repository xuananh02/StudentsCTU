using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;

namespace SocialMediaWisLam.Controllers
{
    public class FriendRelationsController : Controller
    {
        private readonly SocialMediaWisLamContext _context;

        public FriendRelationsController(SocialMediaWisLamContext context)
        {
            _context = context;
        }

        // GET: FriendRelations
        public async Task<IActionResult> Index()
        {
            return View(await _context.FriendRelation.ToListAsync());
        }

        // GET: FriendRelations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var friendRelation = await _context.FriendRelation
                .FirstOrDefaultAsync(m => m.User1ID == id);
            if (friendRelation == null)
            {
                return NotFound();
            }

            return View(friendRelation);
        }

        // GET: FriendRelations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FriendRelations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("User1ID,User2ID,AreFriend")] FriendRelation friendRelation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(friendRelation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(friendRelation);
        }

        // GET: FriendRelations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var friendRelation = await _context.FriendRelation.FindAsync(id);
            if (friendRelation == null)
            {
                return NotFound();
            }
            return View(friendRelation);
        }

        // POST: FriendRelations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("User1ID,User2ID,AreFriend")] FriendRelation friendRelation)
        {
            if (id != friendRelation.User1ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(friendRelation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FriendRelationExists(friendRelation.User1ID))
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
            return View(friendRelation);
        }

        // GET: FriendRelations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var friendRelation = await _context.FriendRelation
                .FirstOrDefaultAsync(m => m.User1ID == id);
            if (friendRelation == null)
            {
                return NotFound();
            }

            return View(friendRelation);
        }

        // POST: FriendRelations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var friendRelation = await _context.FriendRelation.FindAsync(id);
            if (friendRelation != null)
            {
                _context.FriendRelation.Remove(friendRelation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FriendRelationExists(string id)
        {
            return _context.FriendRelation.Any(e => e.User1ID == id);
        }
    }
}
