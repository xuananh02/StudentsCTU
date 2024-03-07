﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;

namespace SocialMediaWisLam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsApiController : ControllerBase
    {
        private readonly SocialMediaWisLamContext _context;

        public LocationsApiController(SocialMediaWisLamContext context)
        {
            _context = context;
        }

        // GET: api/LocationsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocation()
        {
            return await _context.Location.ToListAsync();
        }


        // GET: api/LocationsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            var location = await _context.Location.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // PUT: api/LocationsApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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

        public class LocationForm {
            public string country { get; set; }

            public string city { get; set; }

            public string userId { get; set; }
        }
        
        // POST: api/LocationsApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<String>> PostLocation([FromBody] LocationForm locationForm)
        {
            var user = _context.Profile.FirstOrDefault(item => item.Id == locationForm.userId);
            if (user == null)
                return BadRequest();

            var location = _context.Location.FirstOrDefault(item => item.Country == locationForm.country && item.City == locationForm.city);
            if (location != null)
            {
                user.LocationOwner = location;
                _context.SaveChanges();
                return NoContent();
            }
            return BadRequest("Loi Khong Tai Duoc");
        }

        // DELETE: api/LocationsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Location.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            _context.Location.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationExists(int id)
        {
            return _context.Location.Any(e => e.Id == id);
        }
    }
}
