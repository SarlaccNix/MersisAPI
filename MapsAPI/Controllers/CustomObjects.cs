using MapsAPI.Models.CustomObjects;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class CustomObjectsController : ControllerBase
{
    // MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());
    public List<CustomObjectData> searchedCustomObjects = new List<CustomObjectData>();
    private FilterDefinitionBuilder<CustomObjectData> builder = Builders<CustomObjectData>.Filter;

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");
    
    [HttpPost("customObjects")]
    public string newCustomObject()
    {
        
        var database = client.GetDatabase("QuestHaven");
        var charactersCollection = database.GetCollection<CustomObjectData>("QH_CustomObjects");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        CustomObjectData customObjectData = JsonConvert.DeserializeObject<CustomObjectData>(payload);
        if (customObjectData == null)
        {
            return "Error: missing JSON data";
        } 
        customObjectData.name = customObjectData.customObject.prefabName;
        customObjectData.id = customObjectData.customObject.prefabId;
        customObjectData.creation_Date_Time = DateTime.Now;
        customObjectData.last_Edited_Date_Time = DateTime.Now;
        customObjectData.tags = customObjectData.customObject.keywords;
        try
        {
            charactersCollection.InsertOne(customObjectData);
            return "Custom Object added to Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }
        
    }
    [HttpPost("getCustomObjectById")]
    public async Task<OkObjectResult> getCustomObjectById()
    {
        var payload = String.Empty;
        object? response;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        var database = client.GetDatabase("QuestHaven");
        var charactersCollection = database.GetCollection<CustomObjectData>("QH_CustomObjects");
        var customObject = await charactersCollection
            .Find(Builders<CustomObjectData>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        if (customObject == null)
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
                id = customObject.id,
                name = customObject.name,
                creatorId = customObject.creatorId,
                creatorName = customObject.creatorName,
                customObject = customObject.customObject,
                downloads_quantity = customObject.downloads_quantity,
                creation_Date_Time = customObject.creation_Date_Time,
                last_Edited_Date_Time = customObject.last_Edited_Date_Time
            };
        }
        return new OkObjectResult(response);
    }
    
    // [HttpPut("updateCustomObject")]
    // public IEnumerable<CustomObject> updateCustomObject()
    // {
    //     yield break;
    // }
    
    [HttpPost("searchCustomObjects")]
    public async Task<Object> SearchCustomObjects()
    {
        var database = client.GetDatabase("QuestHaven");
        var customObjectCollection = database.GetCollection<CustomObjectData>("QH_CustomObjects");
        var payload = String.Empty;
        List<CustomObjectData> customObjectsResult = null;
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
        // List<string> tags = JsonConvert.DeserializeObject<dynamic>(payloadJson.Tags);
        string creatorName = payloadJson.creatorName;
        string creatorId = payloadJson.creatorId;
        var find = customObjectCollection.Find(filter);
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
            var searchTextFilter = builder.Regex("name", new BsonRegularExpression(searchText, "i"));
            filter &= searchTextFilter;
        }
        
        // if (tags.Count > 0)
        // {
        //     foreach (string tag in tags)
        //     {
        //         var tagsFilter = builder.Regex("tags", new BsonRegularExpression(tag, "i"));
        //         filter &= tagsFilter;
        //     }
        // }
    
        if (!string.IsNullOrEmpty(creatorName))
        {
            var userFilter = builder.Regex("creatorName", new(creatorName, "i"));
            filter &= userFilter;
        }
    
        customObjectsResult = await customObjectCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();
    
        searchedCustomObjects = new List<CustomObjectData>();
    
        if (customObjectsResult != null)
        {
            foreach (CustomObjectData customObject in customObjectsResult)
            {
                searchedCustomObjects.Add(new CustomObjectData()
                {
                    id = customObject.id,
                    name = customObject.name,
                    creatorId = customObject.creatorId,
                    creatorName = customObject.creatorName,
                    downloads_quantity =  customObject.downloads_quantity,
                    creation_Date_Time = customObject.creation_Date_Time,
                    last_Edited_Date_Time = customObject.last_Edited_Date_Time,
                    customObject = customObject.customObject,
                    tags = customObject.tags
                });
            }
        }
    
        
        if (customObjectsResult != null && customObjectsResult.Any())
        {
            return response = new
            {
                // custom objects found on search
                customObjects = searchedCustomObjects,
                // Amount of custom objects in the current page
                Count = customObjectsResult.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await customObjectCollection.Find(filter).CountDocumentsAsync(),
            };
        }
    
        return error;
    }
    
    [HttpDelete("customObjects")]
    public async Task<Object> DeleteCustomObject()
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
        var database = client.GetDatabase("QuestHaven");
        var charactersCollection = database.GetCollection<CustomObject>("QH_CustomObjects");
        var characterDelete =  charactersCollection
            .DeleteOne(Builders<CustomObject>.Filter.Eq("_id", ObjectId.Parse(id)));
        if (characterDelete.DeletedCount == 0)
        {
            return error;
        }
        var deleteResponse = new
        {
            success = $"Custom Object with ID: {id} deleted."
        };
        return new OkObjectResult(deleteResponse);
    }
}