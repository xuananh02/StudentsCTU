using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SocialMediaWisLam.Data;

namespace SocialMediaWisLam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLocationsController : ControllerBase
    {
        private readonly SocialMediaWisLamContext _context;

        public ApiLocationsController(SocialMediaWisLamContext context)
        {
            _context = context;
        }

        [HttpGet("{country}")]
        public async Task<ActionResult<IEnumerable<String>>> GetTodoItems(string? country)
        {
            return await _context.Location.Where(item => item.Country == country).Select(item => item.City)
                .ToListAsync();
        }
    }
}
