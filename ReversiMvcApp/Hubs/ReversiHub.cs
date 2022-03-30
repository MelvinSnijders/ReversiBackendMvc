using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using static ReversiMvcApp.Data.ApiAccessLayer;

namespace ReversiMvcApp.Hubs
{
    [Authorize(Roles = "Player,Mediator,Administrator")]
    public class ReversiHub : Hub
    {

        private readonly static ConnectionMapping<string> _connections =
        new ConnectionMapping<string>();
        public string API_BASE_URL { get; set; }
        private readonly ApiAccessLayer _api;

        public ReversiHub(IConfiguration configuration, ApiAccessLayer api)
        {
            API_BASE_URL = configuration.GetValue<string>("RestApiUrl");
            _api = api;
        }


        public async Task Move(int moveX, int moveY, bool pass)
        {

            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Game game = await _api.GetGameFromPlayer(userId);
            if (game == null) return;

            MoveResult result = await _api.Move(userId, moveX, moveY, pass);
            await Clients.Client(Context.ConnectionId).SendAsync("MoveResult", result);

            // Send new game state back to both players, do this regardless of the move result to provide some redundancy.
            var newGame = await _api.GetGameFromPlayer(userId);
            await Clients.Clients(_connections.GetConnections(game.Token)).SendAsync("GameState", JsonConvert.SerializeObject(newGame));

        }

        /*
         * Methods for mapping a user to a game connection.
         */

        public async override Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Game game = await _api.GetGameFromPlayer(userId);

            _connections.Add(game.Token, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Game game = await _api.GetGameFromPlayer(userId);

            _connections.Remove(game.Token, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
