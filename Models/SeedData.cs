
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialMediaWisLam.Data;
using System;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace SocialMediaWisLam.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new SocialMediaWisLamContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<SocialMediaWisLamContext>>()))
        {
            // Look for any Location.
            if (context.Location.Any())
            {
                return;   // DB has been seeded
            }

            var locations = new List<Location> {
                new Location { Country = "Viet Nam", City = "Sai Gon", ZipCode = "0"},
                new Location { Country = "VietNam", City = "Ha Noi", ZipCode = "1"},
                new Location { Country = "Australia", City = "Sydney", ZipCode = "0"},
                new Location { Country = "Australia", City = "Melbourne", ZipCode = "1"},
                new Location { Country = "Australia", City = "Brisbane", ZipCode = "2"},
            };
            context.Location.AddRange(
                locations
            );
            context.SaveChanges();
        }
    }
}