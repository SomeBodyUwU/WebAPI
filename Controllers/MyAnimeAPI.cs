using System.Data;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Net;
using API;
using API.Models;
using JikanDotNet;
using Microsoft.EntityFrameworkCore.Update;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Portable;
using static API.Constants;
using Anime = API.Models.Anime;
using Manga = API.Models.Manga;
using SearchByImageModel = API.Models.SearchByImageModel;
using RestRequest = RestSharp.RestRequest;
using RestResponse = RestSharp.RestResponse;

namespace MyAnimeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnimeController : ControllerBase
    {
        

        [HttpGet("Anime")]
        public async Task<List<Anime>> SearchAnime(string name)
        {
            IJikan jikan = new Jikan();
            var animeSearchResult = await jikan.SearchAnimeAsync(name);
            var animeList = new List<Anime>();

            foreach (var anime in animeSearchResult.Data)
            {
                animeList.Add(new Anime()
                {
                    mal_id = Convert.ToInt32(anime.MalId),
                    title = anime.Title,
                    synopsis = anime.Synopsis,
                    url = anime.Url
                });
            }

            return animeList;
        }
        
        [HttpGet("Random Anime")]
        public Anime RandomAnime()
        {
            var client = new RestClient(URL);
            var request = new RestRequest($"/random/anime");

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = response.Content;
            var animeData = JObject.Parse(content)["data"];
            var anime = new Anime();
            anime.mal_id = Convert.ToInt32(animeData["mal_id"]);
            anime.title = (string)animeData["title"];
            anime.synopsis = (string)animeData["synopsis"];
            anime.url = (string)animeData["url"];
            return anime;
            
        }
        
        [HttpGet("Manga")]
        public async Task<List<Anime>> SearchManga(string name)
        {
            IJikan jikan = new Jikan();
            var mangaSearchResult = await jikan.SearchMangaAsync(name);
            var mangaList = new List<Anime>();

            foreach (var manga in mangaSearchResult.Data)
            {
                mangaList.Add(new Anime()
                {
                    mal_id = manga.MalId,
                    title = manga.Title,
                    synopsis = manga.Synopsis,
                    url = manga.Url
                });
            }

            return mangaList;
        }
        
        [HttpGet("Random Manga")]
        public Manga RandomManga()
        {
            var client = new RestClient(URL);
            var request = new RestRequest($"/random/manga");

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = response.Content;
            var mangaData = JObject.Parse(content)["data"];
            var manga = new Manga();
            manga.mal_id = Convert.ToInt32(mangaData["mal_id"]);
            manga.title = (string)mangaData["title"];
            manga.synopsis = (string)mangaData["synopsis"];
            manga.url = (string)mangaData["url"];
            return manga;
            
        }
        
        [HttpGet("Public Top Anime")]
        public async Task<string> ShowTopAnime()
        {
            

            List<DatabaseColumns> table = new List<DatabaseColumns>();
            var database = new DB();
            
            database.GetConnection().Open();
            
            var command = new MySqlCommand("SELECT * FROM users_top_anime",
                database.GetConnection());
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var dataColumns = new DatabaseColumns
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                                   
                };
                table.Add(dataColumns);
            }

            database.GetConnection().Close();
                
            

            var json = System.Text.Json.JsonSerializer.Serialize(table);
            return json;

        }
        
        [HttpPost("PostAsync")]
        public async Task<Anime> PostAsync( int id)
        {
            var client = new RestClient(URL);
            var request = new RestRequest($"/anime/{id}/full");

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = response.Content;
            var animeData = JObject.Parse(content)["data"];
            var anime = new Anime();
            anime.mal_id = Convert.ToInt32(animeData["mal_id"]);
            anime.title = (string)animeData["title"];
            
            //Entering data into database

            DataTable table = new DataTable();
            
            DB database = new DB();
            MySqlCommand command = new MySqlCommand("INSERT IGNORE INTO users_top_anime(id, name) VALUES (@id, @name)",
                database.GetConnection());

            command.Parameters.Add("@id", MySqlDbType.Int32).Value = anime.mal_id;
            command.Parameters.Add("@name", MySqlDbType.VarChar).Value = anime.title;
            
            

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            adapter.SelectCommand = command;
            adapter.Fill(table);


            return anime;
            
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                DB database = new DB();
                await database.GetConnection().OpenAsync();
                MySqlCommand command = new MySqlCommand("SELECT * FROM users_top_anime WHERE id = @id",
                    database.GetConnection());
                command.Parameters.AddWithValue("@id", id);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                
                if (count == 0)
                {
                    return NotFound();
                }
                
                MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM users_top_anime WHERE id = @id",
                    database.GetConnection());
                deleteCommand.Parameters.AddWithValue("@id", id);
                int rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, "Deletion successful");
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, "No anime with such id was found in the top.");
                }
            }
            catch (MySqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while executing the database command.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }

        [HttpGet("NEKO")]
        public async Task<string> GetNSFWNeko()
        {
            var client = new RestClient("https://api.waifu.pics");
            var request = new RestRequest("/nsfw/neko");

            var response = client.Execute(request);
            var content = response.Content;
            return content;
        }
        
        [HttpGet("WAIFU")]
        public async Task<string> GetNSFWWaifu()
        {
            var client = new RestClient("https://api.waifu.pics");
            var request = new RestRequest("/nsfw/waifu");

            var response = client.Execute(request);
            var content = response.Content;
            return content;
        }
        
        [HttpGet("bl")]
        public async Task<string> GetNSFWBlow()
        {
            var client = new RestClient("https://api.waifu.pics");
            var request = new RestRequest("/nsfw/blowjob");

            var response = client.Execute(request);
            var content = response.Content;
            return content;
        }



    }
}