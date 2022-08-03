using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MapsAPI.Models;
using Newtonsoft.Json;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class MapsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<MapsController> _logger;

    public MapsController(ILogger<MapsController> logger)
    {
        _logger = logger;
    }

    // [HttpGet("getMaps")]
    // public IEnumerable<Maps> Get()
    // {
    //     return Enumerable.Range(1, 12).Select(index => new Maps
    //         {
    //             Date = DateTime.Now.AddDays(index),
    //             TemperatureC = Random.Shared.Next(-20, 55),
    //             Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //         })
    //         .ToArray();
    // }

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

        var binary = new BsonElement();
        var defaultMapsDb = database.GetCollection<Maps>("QH_Maps");
        var map = new Maps() { MapName = payloadData.mapName, CreatorId = payloadData.creatorId, Id = payloadData.id, MapFile = fileByteArray };
        System.IO.File.WriteAllBytes(@"F:\Repo\Mersis\OutputFolder\Port_City_output.zip", (fileByteArray));
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

    // [HttpGet("{id}")]
    // public async Task<ActionResult<Maps>> Get()
    // {
    //     return await 
    // }
}