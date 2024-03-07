using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SocialMediaWisLam.Hubs
{
    public class ChatHub : Hub
    {
        private readonly Data.SocialMediaWisLamContext _context;

        public ChatHub(Data.SocialMediaWisLamContext context) {
            _context = context;
        }

    }
}
