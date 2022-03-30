using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReversiMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReversiDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ApiAccessLayer _api;
        private string API_BASE_URL { get; set; }

        public HomeController(ReversiDbContext context, ILogger<HomeController> logger, IConfiguration configuration, ApiAccessLayer api)
        {
            _logger = logger;
            _context = context;
            _api = api;
            API_BASE_URL = configuration.GetValue<string>("RestApiUrl");
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Check if a Player exists in database.
            if (!_context.Players.Any(p => p.Guid == currentUserID))
            {
                Player newPlayer = new Player();
                newPlayer.Guid = currentUserID;
                _context.Players.Add(newPlayer);
                _context.SaveChanges();
            }

            // Check if the current player has a game running
            Game game = await _api.GetGameFromPlayer(currentUserID);
            if(game == null)
            {
                return Redirect("/Game/Index");
            }
            return Redirect("/Game/Play");

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
