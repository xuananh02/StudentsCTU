using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaWisLam.Data;
using SocialMediaWisLam.Models;
using System.Security.Claims;
using static SocialMediaWisLam.Controllers.ApiPostsController;

namespace SocialMediaCTU.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiProfilesController : ControllerBase
    {
        private readonly SocialMediaWisLamContext _context;

        public ApiProfilesController(SocialMediaWisLamContext context) {
            _context = context;
        }

        [HttpGet()]
        public async Task<ActionResult<Profile>> GetUser() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.Profile.FindAsync(userId);

            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

    }
}
