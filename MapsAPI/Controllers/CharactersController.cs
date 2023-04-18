using MapsAPI.Characters;
using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class CharactersController : ControllerBase
{
    // MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());
    public List<CharacterData> searchedCharacters = new List<CharacterData>();
    private FilterDefinitionBuilder<CharacterData> builder = Builders<CharacterData>.Filter;
    private string databaseName = "QuestHaven";

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");
    
    [HttpPost("characters")]
    public string newCharacter()
    {
        
        var database = client.GetDatabase(databaseName);
        var charactersCollection = database.GetCollection<CharacterData>("QH_Characters");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        CharacterData characterData = JsonConvert.DeserializeObject<CharacterData>(payload);
        if (characterData == null)
        {
            return "Error: missing JSON data";
        } 
        characterData.name = characterData.Character.figureName;
        characterData.Creation_Date_Time = DateTime.Now;
        characterData.Last_Edited_Date_Time = DateTime.Now;
        characterData.parentCharacterId = !string.IsNullOrEmpty(characterData.parentCharacterId) ? characterData.parentCharacterId : null;
        try
        {
            charactersCollection.InsertOne(characterData);
            return "Character added to Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }
        
    }
    [HttpPost("getCharacterById")]
    public async Task<OkObjectResult> getCharacter()
    {
        var payload = String.Empty;
        object? response;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        var database = client.GetDatabase(databaseName);
        var charactersCollection = database.GetCollection<CharacterData>("QH_Characters");
        var character = await charactersCollection
            .Find(Builders<CharacterData>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        if (character == null)
        {
            response = new
            {
                error = "ID does not match any file in the database."
            };
        }
        else
        {
            
            response = new
            {
                id = character.id,
                name = character.name,
                creatorId = character.CreatorId,
                creatorName = character.CreatorName,
                character = character.Character,
                tags = character.tags,
                downloads_quantity = character.Downloads_Quantity,
                creation_Date_Time = character.Creation_Date_Time,
                last_Edited_Date_Time = character.Last_Edited_Date_Time
            };
        }
        return new OkObjectResult(response);
    }
    
    [HttpPut("characters")]
    public IEnumerable<Character> updateCharacter()
    {
        yield break;
    }
    
    [HttpPost("searchCharacters")]
    public async Task<Object> SearchCharacters()
    {
        var database = client.GetDatabase(databaseName);
        var charactersCollection = database.GetCollection<CharacterData>("QH_Characters");
        var payload = String.Empty;
        List<CharacterData> charactersResult = null;
        object response;
        object error = new
        {
            Error = "Error, no match found using the current searching criteria"
        };
        var filter = builder.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
        string searchText = payloadJson.SearchText;
        string[] tags = payloadJson.Tags.ToObject<string[]>();
        string creatorId = payloadJson.creatorId;
        var find = charactersCollection.Find(filter);
        int currentPage = 1, currentPagination = 10;

        if (payloadJson.Pagination != null)
        {
            currentPagination = payloadJson.Pagination == 0 ? 10 : payloadJson.Pagination;
        }

        if (payloadJson.Page != null)
        {
            currentPage = payloadJson?.Page == 0 ? 1 : payloadJson.Page;
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            var searchTextFilter = builder.Regex("characterName", new BsonRegularExpression(searchText, "i"));
            filter &= searchTextFilter;
        }

        if (tags is { Length: > 0 })
        {
            filter = (from tag in tags where tag != "" select builder.Regex("tags", new BsonRegularExpression(tag, "i"))).Aggregate(filter, (current, tagsFilter) => current & tagsFilter);
        }

        if (!string.IsNullOrEmpty(creatorId))
        {
            var userFilter = builder.Regex("creatorId", new(creatorId, "i"));
            filter &= userFilter;
        }

        charactersResult = await charactersCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();

        searchedCharacters = new List<CharacterData>();

        if (charactersResult != null)
        {
            foreach (CharacterData character in charactersResult)
            {
                searchedCharacters.Add(new CharacterData()
                {
                    id = character.id,
                    name = character.name,
                    CreatorId = character.CreatorId,
                    CreatorName = character.CreatorName,
                    tags = character.tags,
                    Downloads_Quantity = character.Downloads_Quantity,
                    Creation_Date_Time = character.Creation_Date_Time,
                    Last_Edited_Date_Time = character.Last_Edited_Date_Time,
                    Character = character.Character
                });
            }
        }

        
        if (charactersResult != null && charactersResult.Any())
        {
            return response = new
            {
                // characters found on search
                characters = searchedCharacters,
                // Amount of characters in the current page
                Count = charactersResult.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await charactersCollection.Find(filter).CountDocumentsAsync(),
            };
        }

        return error;
    }
    
    [HttpDelete("characters")]
    public async Task<Object> DeleteCharacter()
    {
        var payload = String.Empty;
        object error = new
        {
            Error = "Error, no match found using the current id. No characters were deleted."
        };
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        if (id.Equals(""))
        {
            return error;
        }
        var database = client.GetDatabase(databaseName);
        var charactersCollection = database.GetCollection<CharacterData>("QH_Characters");
        var characterDelete =  charactersCollection
            .DeleteOne(Builders<CharacterData>.Filter.Eq("_id", ObjectId.Parse(id)));
        if (characterDelete.DeletedCount == 0)
        {
            return error;
        }
        var deleteResponse = new
        {
            success = $"Character with ID: {id} deleted."
        };
        return new OkObjectResult(deleteResponse);
    }
    
    [HttpPatch("updateFieldName")]
    public async void UpdateFieldName()
    {
        var database = client.GetDatabase(databaseName);
        var mapsCollection = database.GetCollection<Character>("QH_Characters");
        var update = Builders<Character>.Update.Rename("favorites", "likes");
        mapsCollection.UpdateMany(new BsonDocument(), update);
    }
}