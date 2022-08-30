using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MapsAPI.Models;
using Newtonsoft.Json;
using System.Linq;
using MongoDB.Bson.Serialization;

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
    public List<MapsList> GetMaps()
    {
        var client = new MongoClient("mongodb+srv://whiteRabbit:MaudioTest@cluster0.u4jq0.mongodb.net/test");
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<MapsList>("QH_Maps");
        var filter = Builders<MapsList>.Projection;
        var fields = filter.Exclude(x => x.MapFile);
        var mapsList = defaultMapsDb.Find(x => true).Project(fields).ToList();
        return mapsList;
    }
    
    // [HttpGet("getMapById")]
    // public IEnumerable<Maps> GetMapById()
    // {
    //     var payload = String.Empty;
    //     using (StreamReader reader = new StreamReader(Request.Body))
    //     {
    //         payload = reader.ReadToEndAsync().Result;
    //     }
    //
    //     dynamic payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
    // }
}