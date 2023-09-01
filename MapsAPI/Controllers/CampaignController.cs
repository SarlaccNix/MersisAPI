using MapsAPI.Campaigns;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Microsoft.AspNetCore.ResponseCompression;
using MapsAPI.Models.Users;

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

    [HttpPatch("InvitePlayerToCampaign")]
    public async Task<string> InvitePlayerToCampaign()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");


        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String campaignID = payloadData.campaignID;
        String userTag = payloadData.userTag;

        var campaignData = await campaignsCollection
            .Find(Builders<CampaignData>.Filter.Eq(c=> c.campaign.id, campaignID))
            .FirstOrDefaultAsync();

        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq(user => user.qh_UserTag, userTag))
            .FirstOrDefaultAsync();

        if (currentUser == null)
        {
            return "Error: Invalid player tag";
        }


        if (campaignData != null && currentUser != null)  
        {
            Console.WriteLine("Current Campaign: " + campaignData.name);
            Console.WriteLine("Current User: " + currentUser.qh_UserTag);
            Console.WriteLine("Payload Tag: " + userTag + " Payload Campaign: " + campaignID);

            FilterDefinition<User> userFilterDef = Builders<User>.Filter.Eq(user => user.qh_UserTag, userTag);
            FilterDefinition<CampaignData> filterDefinition = Builders<CampaignData>.Filter.Eq(c => c.campaign.id, campaignID);

            List<string> invitedPlayers = new List<string>();
            List<string> invitedCampaigns = new List<string>();


            if (campaignData.campaign.invitedPlayersID != null) 
            {
                if (campaignData.campaign.invitedPlayersID.Contains(currentUser.id))
                    return "Player already Invited";

                invitedPlayers = new List<string>(campaignData.campaign.invitedPlayersID);
            }

            if (currentUser.invitedCampaignsID != null)
            {
                invitedCampaigns = new List<string>(currentUser.invitedCampaignsID);
            }

            invitedCampaigns.Add(campaignData.campaign.id);
            invitedPlayers.Add(currentUser.id);

            campaignData.campaign.invitedPlayersID = invitedPlayers.ToArray();
            currentUser.invitedCampaignsID = invitedCampaigns.ToArray();

            var userUpdateDef = Builders<User>.Update.Set(u => u.invitedCampaignsID, currentUser.invitedCampaignsID);
            var updateDefinition = Builders<CampaignData>.Update.Set(c => c.campaign, campaignData.campaign);

            await usersCollection.UpdateOneAsync(userFilterDef, userUpdateDef);
            var update = await campaignsCollection.UpdateOneAsync(filterDefinition, updateDefinition);

            return "Success: Player Invitation send";

        }else
        {
            return "Error: No campaign found.";
        }
    }

    [HttpPatch("AddPlayerToCampaign")]
    public async Task<string> AddPlayerToCampaign()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");


        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String campaignID = payloadData.campaignID;
        String userTag = payloadData.userTag;

        var campaignData = await campaignsCollection
            .Find(Builders<CampaignData>.Filter.Eq("campaign._id", ObjectId.Parse(campaignID)))
            .FirstOrDefaultAsync();

        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq("qh_UserTag", userTag))
            .FirstOrDefaultAsync();

        if (currentUser == null)
        {
            return "Error: Invalid player tag";
        }

        if (campaignData != null && currentUser != null)
        {
            FilterDefinition<User> userFilterDef = Builders<User>.Filter.Eq(user => user.qh_UserTag, userTag);
            FilterDefinition<CampaignData> filterDefinition = Builders<CampaignData>.Filter.Eq(c=> c.campaign.id, campaignID);

            List<string> invitedPlayers = new List<string>(campaignData.campaign.invitedPlayersID);
            List<string> enrolledPlayers = new List<string>();

            if (campaignData.campaign.enrolledPlayersID != null) 
            {
                enrolledPlayers = new List<string>(campaignData.campaign.enrolledPlayersID);
            }

            invitedPlayers.Remove(currentUser.id);
            enrolledPlayers.Add(currentUser.id);


            campaignData.campaign.invitedPlayersID = invitedPlayers.ToArray();
            campaignData.campaign.enrolledPlayersID = enrolledPlayers.ToArray();

            List<string> invitedCampaigns = new List<string>(currentUser.invitedCampaignsID);
            List<string> enrolledCampaigns = new List<string>();


            if (currentUser.enrolledCampaignsID != null)
            {
                enrolledCampaigns = new List<string>(currentUser.enrolledCampaignsID);
            }

            invitedCampaigns.Remove(campaignData.campaign.id);
            enrolledCampaigns.Add(campaignData.campaign.id);

            currentUser.invitedCampaignsID = invitedCampaigns.ToArray();
            currentUser.enrolledCampaignsID = enrolledCampaigns.ToArray();

            var userUpdateDef = Builders<User>.Update.Set(u => u.enrolledCampaignsID, currentUser.enrolledCampaignsID).Set(u => u.invitedCampaignsID, currentUser.invitedCampaignsID);
            var updateDefinition = Builders<CampaignData>.Update.Set(c=> c.campaign , campaignData.campaign);

            var update = await campaignsCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            await usersCollection.UpdateOneAsync(userFilterDef, userUpdateDef);

            return "Success: invitation accepted";

        }
        else
        {
            return "Error: No campaign found.";
        }
    }

}