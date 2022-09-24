using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MapsAPI.Models;
using Newtonsoft.Json.Linq;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class MapsController : ControllerBase
{
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
            MapFile = payloadData.mapFile
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
    public async Task<List<MapsList>> GetMaps()
    {
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        List<MapsList> mapListData;
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<MapsList>("QH_Maps");
        var filter = Builders<MapsList>.Projection;
        var fields = filter.Exclude(x => x.MapFile);
        mapListData = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        // mapsList[Maps] = defaultMapsDb.Find(x => true).Project<MapsList>(fields).ToList();
        return mapListData;
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
        var filter = Builders<Maps>.Filter;
        // string searchText = String.Empty;
        object? mapResults = null;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadJSON = JsonConvert.DeserializeObject<dynamic>(payload);
        if (payloadJSON.SearchText != null && payloadJSON.SearchText != "")
        {
            string searchText = payloadJSON.SearchText;
            // filter = filter.Eq("mapName", payloadJSON.SearchText.ToString());
            mapResults = await defaultMapsDb.Find(Builders<Maps>.Filter.Eq("mapName", searchText))
                .ToListAsync();
        }

        var wrappedMapResults = new
        {
            maps = mapResults
        };
        // return new OkObjectResult(wrappedMapResults);
        if (mapResults != null)
        {
            return mapResults;
        }
        else
        {
            return "Error, no map found";
        }
    }
}