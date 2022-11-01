using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MapsAPI.Models;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class MapsController : ControllerBase
{
    public Dictionary<string, Map> selectedMaps = new Dictionary<string, Map>();
    public List<SearchMap> searchedMaps = new List<SearchMap>();
    private FilterDefinitionBuilder<Map> builder = Builders<Map>.Filter;
    private readonly ILogger<MapsController> _logger;

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");


    public MapsController(ILogger<MapsController> logger)
    {
        _logger = logger;
    }


    [HttpPost("createNewMap")]
    public async Task<OkObjectResult> Post()
    {
        Map uploadedMap;
        byte[] emptyByte = null;
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        
        dynamic payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        var database = client.GetDatabase("QuestHaven");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var map = new Map()    
        {
            MapName = payloadData.mapName, CreatorId = payloadData.creatorId, CreatorName = payloadData.creatorName,Id = payloadData.id,
            MapFile = payloadData.mapFile, MapDescription = payloadData.description,
            MapVersion = 1, Creation_Date_Time = DateTime.Now, Last_Edited_Date_Time = DateTime.Now
        };
        
        try
        {
            defaultMapsDb.InsertOne(map);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            throw;
        }
        uploadedMap = await defaultMapsDb.Find(Builders<Map>.Filter.Eq("_id", ObjectId.Parse(map.Id)))
            .FirstOrDefaultAsync();
        
        var mapResponse = new
        {
            id = uploadedMap.Id,
            mapName = uploadedMap.MapName,
            mapFile = emptyByte,
            creatorId = uploadedMap.CreatorId,
            creatorName = uploadedMap.CreatorName,
            creation_Date_Time = uploadedMap.Creation_Date_Time,
            last_Edited_Date_Time = uploadedMap.Last_Edited_Date_Time,
            favorites = uploadedMap.Favorites,
            downloaded_quantity = uploadedMap.Downloads_Quantity
        };
        return new OkObjectResult(mapResponse);
    }

    [HttpGet("getMaps")]
    public async Task<List<Map>> GetMaps()
    {
        Debug.WriteLine("Get Maps protocol started");
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        List<Map> mapListData;
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
        var database = client.GetDatabase("QH_Maps_Default");
        var defaultMapsDb = database.GetCollection<Map>("QH_Maps");
        var payload = String.Empty;
        string response;
        byte[] newMapFile = Array.Empty<byte>();
        string mapToUpdate = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        try
        {
            mapToUpdate = payloadData.id;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Id is missing. Exception: " + e);
            response = "Map id and/or map file missing.";
            return response;
        }
        
        if (payloadData.mapFile != null)
        {
            newMapFile = payloadData.mapFile.Equals(typeof(string))
                ? (byte[])Convert.FromBase64String(payloadData.mapFile)
                : (byte[])payloadData.mapFile;
        }
        if (!string.IsNullOrEmpty(mapToUpdate)&& newMapFile != Array.Empty<byte>()){
            FilterDefinition<Map> filter = Builders<Map>.Filter.Eq("_id", ObjectId.Parse(mapToUpdate));
            var update = Builders<Map>.Update.Set("mapFile", newMapFile);
            var fetchUpdate = await defaultMapsDb.UpdateOneAsync(filter, update);
            if (fetchUpdate.MatchedCount == 1)
            {
                Builders<Map>.Update.Set("last_Edited_Date_Time", DateTime.Now);
                response = $"Map file for ID: {mapToUpdate} updated.";
            }
            else
            {
                response = "ID does not match any file in the database.";
            }
        }
        else 
        {
            response= "Map ID and/or map file missing.";
        }

        return response;
    }

    [HttpPut("updateTableField")]
    public async Task<string> UpdateTableField()
    {
        // var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        // List<Maps> mapListData;
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
            var userFilter = builder.Regex("creatorName", new(userId, "i"));
            filter &= userFilter;
        }

        mapResults = await defaultMapsDb.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();

        searchedMaps = new List<SearchMap>();

        if (mapResults != null)
        {
            foreach (Map map in mapResults)
            {
                searchedMaps.Add(new SearchMap()
                {
                    Id = map.Id, MapName = map.MapName,
                    CreatorId = map.CreatorId,
                    tags = map.tags,
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