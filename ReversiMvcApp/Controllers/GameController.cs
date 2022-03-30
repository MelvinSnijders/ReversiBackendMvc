using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReversiMvcApp.Controllers
{
    public class GameController : Controller
    {

        public string API_BASE_URL { get; set; }
        public ReversiDbContext _context { get; set; }
        private readonly ApiAccessLayer _api;

        public GameController(IConfiguration configuration, ReversiDbContext context, ApiAccessLayer api)
        {
            API_BASE_URL = configuration.GetValue<string>("RestApiUrl");
            _context = context;
            _api = api;
        }

        // GET: GameController
        [Authorize(Roles = "Player,Mediator,Administrator")]
        public async Task<ActionResult> Index()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Game game = await _api.GetGameFromPlayer(currentUserID);

            if (game != null)
            {
                return Redirect("/Game/Play");
            }

            List<Game> games = await _api.GetAvailableGames();
            return View(games);
        }

        // GET: GameController/Create
        [Authorize(Roles = "Player,Mediator,Administrator")]
        public async Task<ActionResult> Create()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Game game = await _api.GetGameFromPlayer(currentUserID);

            if (game != null)
            {
                return Redirect("/Game/Play");
            }

            return View();
        }

        // POST: GameController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Player,Mediator,Administrator")]
        public async Task<ActionResult> Create(string description)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Game game = await _api.GetGameFromPlayer(currentUserID);

            if (game != null)
            {
                return Redirect("/Game/Play");
            }

            await _api.CreateGame(currentUserID, description);

            return RedirectToAction(nameof(Index));
        }

        // PUT: GameController/Join
        [HttpPost]
        public async Task<ActionResult> Join(int gameId)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            await _api.JoinGame(gameId, currentUserID);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Player,Mediator,Administrator")]
        public async Task<ActionResult> Play()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Game game = await _api.GetGameFromPlayer(currentUserID);

            if (game == null)
            {
                return Redirect("/Game");
            }

            return View(game);
        }

        public class NewGame
        {
            public string PlayerToken { get; set; }
            public string Description { get; set; }
        }

    }
}
