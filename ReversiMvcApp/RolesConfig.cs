using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReversiMvcApp.Data;
using System;
using System.Threading.Tasks;

namespace ReversiMvcApp
{
    public static class RolesConfig
    {
        public static async Task InitialiseAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Player", "Mediator", "Administrator" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

    }
}
