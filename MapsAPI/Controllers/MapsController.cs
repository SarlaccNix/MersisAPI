using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MapsAPI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class MapsController : ControllerBase
{
    public Dictionary<string, Map> selectedMaps = new Dictionary<string, Map>();
    public List<SearchMap> searchedMaps = new List<SearchMap>();
    private FilterDefinitionBuilder<Map> builder = Builders<Map>.Filter;
    private readonly ILogger<MapsController> _logger;

    public MapsController(ILogger<MapsController> logger)
    {
        _logger = logger;
    }


    [HttpPost("createNewMap")]
    public IEnumerable<Map> Post()
    {
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        dynamic payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        // var filePath = @"F:\Repo\Mersis\Port_City.zip";
        // byte[] fileByteArray = System.IO.File.ReadAllBytes(filePath);
        // byte[] fileByteArray = payloadData.mapFile;
        // string fileString = Encoding.Default.GetString(fileByteArray);
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var map = new Map()    
        {
            MapName = payloadData.mapName, CreatorId = payloadData.creatorId, Id = payloadData.id,
            MapFile = payloadData.mapFile, MapDescription = payloadData.description,
            MapVersion = 1
        };
        // System.IO.File.WriteAllBytes(@"F:\Repo\Mersis\OutputFolder\" + payloadData.mapName + ".zip", payloadData.mapFile);
        try
        {
            defaultMapsDb.InsertOne(map);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            throw;
        }

        return Enumerable.Range(1, 1).Select(index => new Map
            {
                MapName = payloadData.mapName,
                CreatorId = payloadData.creatorId,
                Id = payloadData.id,
                MapFile = payloadData.mapFile
            })
            .ToArray();
    }

    [HttpGet("getMaps")]
    public async Task<List<Map>> GetMaps()
    {
        Debug.WriteLine("Get Maps protocol started");
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        List<Map> mapListData;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var filter = Builders<Map>.Projection;
        var fields = filter.Exclude(x => x.MapFile);
        mapListData = defaultMapsDb.Find(x => true).Project<Map>(fields).ToList();
        selectedMaps = new Dictionary<string, Map>();
        foreach (Map map in mapListData)
        {
            selectedMaps[map.Id] = map;
        }

        // mapsList[Maps] = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        return mapListData;
    }

    [HttpPut("updateMap")]
    public async Task<string> UpdateMap()
    {
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var payload = String.Empty;
        string response;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        string mapToUpdate = payloadData.id;
        string newMapFile = payloadData.mapFile;
        if (!string.IsNullOrEmpty(mapToUpdate) && !string.IsNullOrEmpty(newMapFile)){
            FilterDefinition<Map> filter = Builders<Map>.Filter.Eq("_id", ObjectId.Parse(mapToUpdate));
            var update = Builders<Map>.Update.Set("mapFile", newMapFile);
            var fetchUpdate = await defaultMapsDb.UpdateOneAsync(filter, update);
            if (fetchUpdate.MatchedCount == 1)
            {
                response = $"Map file for id: {mapToUpdate} updated.";
            }
            else
            {
                response = "Id does not match any file in the database.";
            }
        }
        else 
        {
            response= "Map id and/or map file missing.";
        }

        return response;
    }

    [HttpPut("updateTableField")]
    public async Task<string> UpdateTableField()
    {
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        // List<Maps> mapListData;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var update = Builders<Map>.Update.Set("MapVersion", "1");
        FilterDefinition<Map> filter = Builders<Map>.Filter.Empty;
        await defaultMapsDb.UpdateManyAsync(filter, update);
        // var fields = filter.Exclude(x => x.MapFile);
        // mapListData = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        // mapsList[Maps] = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        return "Updated";
    }

    [HttpPost("getMapById")]
    public async Task<OkObjectResult> GetMapById()
    {
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var map = await defaultMapsDb
            .Find(Builders<Map>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        var mapResponse = new
        {
            id = map.Id,
            mapName = map.MapName,
            version = map.MapVersion,
            mapFile = map.MapFile
        };
        return new OkObjectResult(mapResponse);
    }

    [HttpDelete("deleteMap")]
    public async Task<Object> DeleteMap()
    {
        var payload = String.Empty;
        object error = new
        {
            Error = "Error, no match found using the current id. No maps were deleted."
        };
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var map =  defaultMapsDb
            .DeleteOne(Builders<Map>.Filter.Eq("_id", ObjectId.Parse(id)));
        if (map.DeletedCount == 0)
        {
            return error;
        }
        var deleteResponse = new
            {
                success = $"Map with id {id} deleted."
            };
            return new OkObjectResult(deleteResponse);
    }

    [HttpPost("SearchMaps")]
    public async Task<Object> SearchMaps()
    {
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var payload = String.Empty;
        object response;
        object error = new
        {
            Error = "Error, no match found using the current searching criteria"
        };
        var filter = builder.Empty;
        List<Map> mapResults = null;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);


        string searchText = payloadJson.SearchText;
        string tags = payloadJson.Tags;
        string userId = payloadJson.UserId;
        var find = defaultMapsDb.Find(filter);
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
            var searchTextFilter = builder.Regex("mapName", new BsonRegularExpression(searchText, "i"));
            filter &= searchTextFilter;
        }

        if (!string.IsNullOrEmpty(tags))
        {
            var tagsFilter = builder.Regex("tags", new BsonRegularExpression(tags, "i"));
            filter &= tagsFilter;
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var userFilter = builder.Regex("creatorId", new(userId, "i"));
            filter &= userFilter;
        }
        // else if (string.IsNullOrEmpty(searchText))
        // {
        //     mapResults = defaultMapsDb.Find(x => true).ToList();
        // }

        mapResults = await defaultMapsDb.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();

        searchedMaps = new List<SearchMap>();

        if (mapResults != null)
        {
            foreach (Map map in mapResults)
            {
                searchedMaps.Add(new SearchMap()
                {
                    Id = map.Id, MapName = map.MapName, CreatorId = map.CreatorId, tags = map.tags,
                    MapDescription = map.MapDescription,
                    MapVersion = map.MapVersion,
                    Favorites = map.Favorites,
                    Downloads_Quantity = map.Downloads_Quantity,
                    Creation_Date_Time = map.Creation_Date_Time,
                    Last_Edited_Date_Time = map.Last_Edited_Date_Time,
                    MapPreview = map.MapPreview
                });
            }
        }


        // return new OkObjectResult(wrappedMapResults);
        if (mapResults != null && mapResults.Any())
        {
            return response = new
            {
                // maps found on search
                Maps = searchedMaps,
                // Amount of maps in the current page
                Count = mapResults.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await defaultMapsDb.Find(filter).CountDocumentsAsync(),
            };
        }

        return error;
    }
}