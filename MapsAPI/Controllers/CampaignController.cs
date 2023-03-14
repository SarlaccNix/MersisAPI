using MapsAPI.Campaigns;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class CampaignsController : ControllerBase
{
    public List<CampaignData> searchedCampaigns = new List<CampaignData>();
    private FilterDefinitionBuilder<CampaignData> builder = Builders<CampaignData>.Filter;
    private string databaseName = "QuestHaven";

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");
    
    [HttpPost("campaigns")]
    public string NewCampaign([FromForm] Campaign campaignForm)
    {
        
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        CampaignData newCampaign = new CampaignData();
        if (campaignForm == null)
        {
            return "Error: missing campaign form data";
        }
        newCampaign.creation_Date_Time = DateTime.Now;
        newCampaign.last_Edited_Date_Time = DateTime.Now;
        newCampaign.campaign = campaignForm;
        newCampaign.name = campaignForm.name;
        newCampaign.creatorId = campaignForm.creatorId;
        try
        {
            campaignsCollection.InsertOne(newCampaign);
            return "Campaign added to Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }
        
    }
    
    [HttpPost("getCampaignById")]
    public async Task<OkObjectResult> GetCampaign()
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
        var charactersCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var campaign = await charactersCollection
            .Find(Builders<CampaignData>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        if (campaign == null)
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
                id = campaign.id,
                name = campaign.name,
                creatorId = campaign.creatorId,
                campaign = campaign.campaign,
                creation_Date_Time = campaign.creation_Date_Time,
                last_Edited_Date_Time = campaign.last_Edited_Date_Time
            };
        }
        return new OkObjectResult(response);
    }

    [HttpPost("search")]
    public async Task<Object> SearchCampaigns()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var payload = String.Empty;
        List<CampaignData> campaignResults = null;
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
        string creatorId = payloadJson.creatorId;
        var find = campaignsCollection.Find(filter);
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

        campaignResults = await campaignsCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();

        searchedCampaigns = new List<CampaignData>();

        if (campaignResults != null)
        {
            foreach (CampaignData campaign in campaignResults)
            {
                searchedCampaigns.Add(new CampaignData()
                {
                    id = campaign.id,
                    creatorId = campaign.creatorId,
                    name = campaign.name,
                    creation_Date_Time = campaign.creation_Date_Time,
                    last_Edited_Date_Time = campaign.last_Edited_Date_Time,
                    campaign = campaign.campaign,
                });
            }
        }
        if (campaignResults != null && campaignResults.Any())
        {
            return response = new
            {
                // characters found on search
                campaign = searchedCampaigns,
                // Amount of characters in the current page
                Count = campaignResults.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await campaignsCollection.Find(filter).CountDocumentsAsync(),
            };
        }

        return error;
    }

    // [HttpPut("characters")]
    // public IEnumerable<Character> updateCharacter()
    // {
    //     yield break;
    // }
    //
    // [HttpPost("searchCharacters")]
    // public async Task<Object> SearchMaps()
    // {
    //     var database = client.GetDatabase(databaseName);
    //     var charactersCollection = database.GetCollection<CharacterData>("QH_Characters");
    //     var payload = String.Empty;
    //     List<CharacterData> charactersResult = null;
    //     object response;
    //     object error = new
    //     {
    //         Error = "Error, no match found using the current searching criteria"
    //     };
    //     var filter = builder.Empty;
    //
    //     using (StreamReader reader = new StreamReader(Request.Body))
    //     {
    //         payload = reader.ReadToEndAsync().Result;
    //     }
    //
    //     var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
    //     string searchText = payloadJson.SearchText;
    //     string[] tags = payloadJson.Tags.ToObject<string[]>();
    //     string creatorName = payloadJson.creatorName;
    //     string creatorId = payloadJson.creatorId;
    //     var find = charactersCollection.Find(filter);
    //     int currentPage = 1, currentPagination = 10;
    //
    //     if (payloadJson.Pagination != null)
    //     {
    //         currentPagination = payloadJson.Pagination == 0 ? 10 : payloadJson.Pagination;
    //     }
    //
    //     if (payloadJson.Page != null)
    //     {
    //         currentPage = payloadJson?.Page == 0 ? 1 : payloadJson.Page;
    //     }
    //
    //     if (!string.IsNullOrEmpty(searchText))
    //     {
    //         var searchTextFilter = builder.Regex("characterName", new BsonRegularExpression(searchText, "i"));
    //         filter &= searchTextFilter;
    //     }
    //
    //     if (tags is { Length: > 0 })
    //     {
    //         filter = (from tag in tags where tag != "" select builder.Regex("tags", new BsonRegularExpression(tag, "i"))).Aggregate(filter, (current, tagsFilter) => current & tagsFilter);
    //     }
    //
    //     if (!string.IsNullOrEmpty(creatorName))
    //     {
    //         var userFilter = builder.Regex("creatorName", new(creatorName, "i"));
    //         filter &= userFilter;
    //     }
    //
    //     charactersResult = await charactersCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
    //         .Limit(currentPagination).ToListAsync();
    //
    //     searchedCharacters = new List<CharacterData>();
    //
    //     if (charactersResult != null)
    //     {
    //         foreach (CharacterData character in charactersResult)
    //         {
    //             searchedCharacters.Add(new CharacterData()
    //             {
    //                 id = character.id,
    //                 name = character.name,
    //                 CreatorId = character.CreatorId,
    //                 CreatorName = character.CreatorName,
    //                 tags = character.tags,
    //                 Downloads_Quantity = character.Downloads_Quantity,
    //                 Creation_Date_Time = character.Creation_Date_Time,
    //                 Last_Edited_Date_Time = character.Last_Edited_Date_Time,
    //                 Character = character.Character
    //             });
    //         }
    //     }
    //
    //     
    //     if (charactersResult != null && charactersResult.Any())
    //     {
    //         return response = new
    //         {
    //             // characters found on search
    //             characters = searchedCharacters,
    //             // Amount of characters in the current page
    //             Count = charactersResult.Count,
    //             // Selected page number
    //             Page = currentPage,
    //             // Selected amount of items per page
    //             Pagination = currentPagination,
    //             // Amount of matches for current search
    //             Hits = await charactersCollection.Find(filter).CountDocumentsAsync(),
    //         };
    //     }
    //
    //     return error;
    // }
}