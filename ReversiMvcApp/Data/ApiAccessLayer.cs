using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReversiMvcApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReversiMvcApp.Data
{
    public class ApiAccessLayer
    {

        private readonly string API_BASE_URL;
        public HttpClient _client { get; private set; }

        public ApiAccessLayer(IConfiguration configuration)
        {
            API_BASE_URL = configuration.GetValue<string>("RestApiUrl");
            _client = new HttpClient();
        }

        public async Task<Game> GetGameFromPlayer(string userId)
        {
            var response = await _client.GetAsync($"{API_BASE_URL}/api/Game/gameplayer/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Game>(content);
        }

        public async Task<List<Game>> GetAvailableGames()
        {
            var response = await _client.GetAsync($"{API_BASE_URL}/api/Game");
            string apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Game>>(apiResponse);
        }

        public async Task CreateGame(string userId, string description)
        {
            var newGame = new { Description = description, PlayerToken = userId };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(newGame), Encoding.UTF8, "application/json");
            await _client.PostAsync($"{API_BASE_URL}/api/Game", stringContent);
        }

        public async Task<MoveResult> Move(string userId, int moveX, int moveY, bool pass)
        {
            Game game = await GetGameFromPlayer(userId);
            if (game == null) return MoveResult.Failed;

            var move = new { GameToken = game.Token, PlayerToken = userId, MoveX = moveX, MoveY = moveY, Pass = pass };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(move), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"{API_BASE_URL}/api/Game/move", stringContent);
            if(!response.IsSuccessStatusCode)
            {
                return MoveResult.Failed;
            }
            var message = await response.Content.ReadAsStringAsync();
            Enum.TryParse(message, out MoveResult result);
            return result;
        }

        public async Task<Game> JoinGame(int gameId, string userId)
        {
            var join = new { GameId = gameId, PlayerToken = userId };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(join), Encoding.UTF8, "application/json");
            var response =  await _client.PostAsync($"{API_BASE_URL}/api/Game/join", stringContent);
            string apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Game>(apiResponse);

        }

        public enum MoveResult
        {
            Success,
            Failed,
            InvalidMove,
            NotYourTurn
        }

    }
}