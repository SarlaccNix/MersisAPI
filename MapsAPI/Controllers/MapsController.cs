using System.Text;
using System.Collections.Generic;
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
    public Dictionary<string, Maps> selectedMaps = new Dictionary<string, Maps>();
    private FilterDefinitionBuilder<Maps> builder = Builders<Maps>.Filter;
    private readonly ILogger<MapsController> _logger;

    public MapsController(ILogger<MapsController> logger)
    {
        _logger = logger;
    }


    [HttpPost("createNewMap")]
    public IEnumerable<Maps> Post()
    {
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        dynamic payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var filePath = @"F:\Repo\Mersis\Port_City.zip";
        byte[] fileByteArray = System.IO.File.ReadAllBytes(filePath);
        string fileString = Encoding.Default.GetString(fileByteArray);
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var map = new Maps()
        {
            MapName = payloadData.mapName, CreatorId = payloadData.creatorId, Id = payloadData.id,
            MapFile = payloadData.mapFile,
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

        return Enumerable.Range(1, 1).Select(index => new Maps
            {
                MapName = payloadData.mapName,
                CreatorId = payloadData.creatorId,
                Id = payloadData.id,
                MapFile = fileByteArray
            })
            .ToArray();
    }

    [HttpGet("getMaps")]
    public async Task<List<Maps>> GetMaps()
    {
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        List<Maps> mapListData;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var filter = Builders<Maps>.Projection;
        var fields = filter.Exclude(x => x.MapFile);
        mapListData = defaultMapsDb.Find(x => true).Project<Maps>(fields).ToList();
        selectedMaps = new Dictionary<string, Maps>();
        foreach (Maps map in mapListData)
        {
            selectedMaps[map.Id] = map;
        }

        // mapsList[Maps] = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        return mapListData;
    }

    [HttpGet("updateTableField")]
    public async Task<string> UpdateTableField()
    {
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        // List<Maps> mapListData;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var update = Builders<Maps>.Update.Unset("Tags");
        FilterDefinition<Maps> filter = Builders<Maps>.Filter.Empty;
        await defaultMapsDb.UpdateManyAsync(filter, update);
        // var fields = filter.Exclude(x => x.MapFile);
        // mapListData = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        // mapsList[Maps] = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        return "Updated";
    }

    [HttpGet("getMapById")]
    public async Task<OkObjectResult> GetMapById()
    {
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        Console.WriteLine("String id", id, payloadData);
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var map = await defaultMapsDb
            .Find(Builders<Maps>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        var mapResponse = new
        {
            id = map.Id,
            mapName = map.MapName,
            creatorId = map.CreatorId,
            version = map.MapVersion,
            mapFile = map.MapFile
        };
        return new OkObjectResult(mapResponse);
    }

    [HttpPost("SearchMaps")]
    public async Task<Object> SearchMaps()
    {
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var payload = String.Empty;
        object response;
        object error = new
        {
            Error = "Error, no match found using the current searching criteria",
            status = 200
        };
        var filter = builder.Empty;
        List<Maps> mapResults = null;

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

        selectedMaps = new Dictionary<string, Maps>();

        if (mapResults != null)
        {
            foreach (Maps map in mapResults)
            {
                selectedMaps[map.Id] = map;
            }
        }


        // return new OkObjectResult(wrappedMapResults);
        if (mapResults != null && mapResults.Any())
        {
            return response = new
            {
                // maps found on search
                Maps = mapResults,
                // Amount of maps in the current page
                Count = mapResults.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await defaultMapsDb.Find(filter).CountDocumentsAsync(),
                Status = "200"
            };
        }

        return error;
    }
}