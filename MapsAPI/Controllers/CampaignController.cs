using MapsAPI.Campaigns;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Microsoft.AspNetCore.ResponseCompression;
using MapsAPI.Models.Users;
using System.Runtime.CompilerServices;

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
    public async Task<string> NewCampaign()
    {

        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");


        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var campaignData = JsonConvert.DeserializeObject<CampaignData>(payload);

        if (campaignData == null) 
        {
            return "Error: Json missing.";
        }

        var campaignCreatorFilter = Builders<User>.Filter.Eq(u => u.id, campaignData.creatorId);
        var campaignCreatorUpdateDef = Builders<User>.Update.Push(u => u.enrolledCampaignsID, campaignData.id);
        await usersCollection.UpdateOneAsync(campaignCreatorFilter, campaignCreatorUpdateDef);



        if (campaignData.invitedPlayersID != null && campaignData.invitedPlayersID.Any()) 
        {
            var userFilter = Builders<User>.Filter.In(u => u.id, campaignData.invitedPlayersID);

            var UpdateDefinition = Builders<User>.Update.Push(u => u.invitedCampaignsID, campaignData.id);
            await usersCollection.UpdateManyAsync(userFilter, UpdateDefinition);
        }


        campaignData.creation_Date_Time = DateTime.Now;
        campaignData.last_Edited_Date_Time = DateTime.Now;


        try
        {
            campaignsCollection.InsertOne(campaignData);
            return "Success";
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
                creatorname = campaign.creatorName,
                description = campaign.description,
                creation_Date_Time = campaign.creation_Date_Time,
                last_Edited_Date_Time = campaign.last_Edited_Date_Time,
                enrolledPlayersID = campaign.enrolledPlayersID,
                invitedPlayersID = campaign.invitedPlayersID,
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
                    creatorName = campaign.creatorName,
                    description = campaign.description,
                    name = campaign.name,
                    creation_Date_Time = campaign.creation_Date_Time,
                    last_Edited_Date_Time = campaign.last_Edited_Date_Time,
                    dungeonMastersID = campaign.dungeonMastersID,
                    enrolledPlayersID = campaign.enrolledPlayersID,
                    invitedPlayersID = campaign.invitedPlayersID
                }) ;
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

    [HttpPost("searchEnrolledInvitedCampaigns")]
    public async Task<Object> SearchCampaignsEnrolled()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");
        object response;

        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String userID = payloadData.id;
        int Page = payloadData.Page;
        int Pagination = payloadData.Pagination;
        bool searchInvitation = payloadData.SearchInvitation;
        int currentPage = 1, currentPagination = 10;

        var filter = builder.Empty;

        List<CampaignData> campaignResults = null;

        var user = await usersCollection.Find(Builders<User>.Filter.Eq(u => u.id, userID)).FirstOrDefaultAsync();

        if (user != null)
        {
            if (user.invitedCampaignsID != null || user.enrolledCampaignsID != null) 
            {
                string[] campaignIDs;

                if (searchInvitation)
                    campaignIDs = user.invitedCampaignsID;
                else
                    campaignIDs = user.enrolledCampaignsID;

                filter = builder.In(c => c.id, campaignIDs);

                campaignResults = await campaignsCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
                .Limit(currentPagination).ToListAsync();
            }
        }


        if (campaignResults != null && campaignResults.Any())
        {
            foreach (CampaignData campaign in campaignResults)
            {
                searchedCampaigns.Add(new CampaignData()
                {
                    id = campaign.id,
                    creatorId = campaign.creatorId,
                    creatorName = campaign.creatorName,
                    name = campaign.name,
                    description = campaign.description,
                    creation_Date_Time = campaign.creation_Date_Time,
                    last_Edited_Date_Time = campaign.last_Edited_Date_Time,
                    dungeonMastersID = campaign.dungeonMastersID,
                    enrolledPlayersID = campaign.enrolledPlayersID,
                    invitedPlayersID = campaign.invitedPlayersID,
                });
            }

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

        return null;
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
            .Find(Builders<CampaignData>.Filter.Eq(c=> c.id, campaignID))
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
            FilterDefinition<CampaignData> filterDefinition = Builders<CampaignData>.Filter.Eq(c => c.id, campaignID);

            List<string> invitedPlayers = new List<string>();
            List<string> invitedCampaigns = new List<string>();

            if(campaignData.enrolledPlayersID != null) 
            {
                if (campaignData.enrolledPlayersID.Contains(currentUser.id))
                    return "Player is already enrolled in this campaign";
            }
           

            if (campaignData.invitedPlayersID != null) 
            {
                if (campaignData.invitedPlayersID.Contains(currentUser.id))
                    return "Player already Invited";

                invitedPlayers = new List<string>(campaignData.invitedPlayersID);
            }

            if (currentUser.invitedCampaignsID != null)
            {
                invitedCampaigns = new List<string>(currentUser.invitedCampaignsID);
            }

            invitedCampaigns.Add(campaignData.id);
            invitedPlayers.Add(currentUser.id);

            campaignData.invitedPlayersID = invitedPlayers.ToArray();
            currentUser.invitedCampaignsID = invitedCampaigns.ToArray();

            var userUpdateDef = Builders<User>.Update.Set(u => u.invitedCampaignsID, currentUser.invitedCampaignsID);
            var updateDefinition = Builders<CampaignData>.Update.Set(c => c.invitedPlayersID, campaignData.invitedPlayersID);

            await usersCollection.UpdateOneAsync(userFilterDef, userUpdateDef);
            var update = await campaignsCollection.UpdateOneAsync(filterDefinition, updateDefinition);

            return "Success";

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
            .Find(Builders<CampaignData>.Filter.Eq(C=> C.id, campaignID))
            .FirstOrDefaultAsync();

        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq(u=> u.qh_UserTag, userTag))
            .FirstOrDefaultAsync();

        if (currentUser == null)
        {
            return "Error: Invalid player tag";
        }

        if (campaignData != null && currentUser != null)
        {
            FilterDefinition<User> userFilterDef = Builders<User>.Filter.Eq(user => user.qh_UserTag, userTag);
            FilterDefinition<CampaignData> filterDefinition = Builders<CampaignData>.Filter.Eq(c=> c.id, campaignID);

            List<string> invitedPlayers = new List<string>(campaignData.invitedPlayersID);
            List<string> enrolledPlayers = new List<string>();

            if (campaignData.enrolledPlayersID != null) 
            {
                enrolledPlayers = new List<string>(campaignData.enrolledPlayersID);
            }

            invitedPlayers.Remove(currentUser.id);
            enrolledPlayers.Add(currentUser.id);


            campaignData.invitedPlayersID = invitedPlayers.ToArray();
            campaignData.enrolledPlayersID = enrolledPlayers.ToArray();

            List<string> invitedCampaigns = new List<string>(currentUser.invitedCampaignsID);
            List<string> enrolledCampaigns = new List<string>();


            if (currentUser.enrolledCampaignsID != null)
            {
                enrolledCampaigns = new List<string>(currentUser.enrolledCampaignsID);
            }

            invitedCampaigns.Remove(campaignData.id);
            enrolledCampaigns.Add(campaignData.id);

            currentUser.invitedCampaignsID = invitedCampaigns.ToArray();
            currentUser.enrolledCampaignsID = enrolledCampaigns.ToArray();

            var userUpdateDef = Builders<User>.Update.Set(u => u.enrolledCampaignsID, currentUser.enrolledCampaignsID).Set(u => u.invitedCampaignsID, currentUser.invitedCampaignsID);
            var updateDefinition = Builders<CampaignData>.Update.Set(c => c.invitedPlayersID, campaignData.invitedPlayersID).Set(c => c.enrolledPlayersID, campaignData.enrolledPlayersID);

            var update = await campaignsCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            await usersCollection.UpdateOneAsync(userFilterDef, userUpdateDef);

            return "Success";

        }
        else
        {
            return "Error: No campaign found.";
        }
    }

    [HttpPatch("DeclineInvitation")]
    public async Task<string> DeclineInvitation()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");


        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var campaign = JsonConvert.DeserializeObject<CampaignData>(payload);


        if (campaign != null)
        {
            var filter = Builders<CampaignData>.Filter.Eq(c => c.id, campaign.id);
            var userFilter = Builders<User>.Filter.Eq(u => u.id, campaign.creatorId);

            var updateDef = Builders<CampaignData>.Update.Pull(c=> c.invitedPlayersID, campaign.creatorId);
            var userUpdateDef = Builders<User>.Update.Pull(u => u.invitedCampaignsID, campaign.id);

            await campaignsCollection.UpdateOneAsync(filter, updateDef);
            await usersCollection.UpdateOneAsync(userFilter, userUpdateDef);

            return "Success";
        }
        else
            return "Error: campaign Empty";
    }


    [HttpPatch("UpdateCampaignDescription")]
    public async Task<string> UpdateCampaignDescription()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");

        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var campaign = JsonConvert.DeserializeObject<CampaignData>(payload);


        if (campaign != null) 
        {
            var filter = Builders<CampaignData>.Filter.Eq(c=> c.id, campaign.id);
            var updateDef = Builders<CampaignData>.Update.Set(c=>c.description, campaign.description);

            await campaignsCollection.UpdateOneAsync(filter, updateDef);

            return "Success";

        }
        else
            return "Error: campaign Empty";
    }

    [HttpPatch("AbandonCampaign")]
    public async Task<string> AbandonCampaign()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");

        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var campaign = JsonConvert.DeserializeObject<CampaignData>(payload);


        if (campaign != null)
        {
            var campaignFilter = Builders<CampaignData>.Filter.Eq(c => c.id, campaign.id);
            var userFilter = Builders<User>.Filter.Eq(u => u.id, campaign.creatorId);

            var userUpdateDef = Builders<User>.Update.Pull(u => u.enrolledCampaignsID, campaign.id);
            var campaignUpdateDef = Builders<CampaignData>.Update.Pull(c => c.enrolledPlayersID, campaign.creatorId).Pull(c => c.dungeonMastersID, campaign.creatorId);

            await usersCollection.UpdateOneAsync(userFilter, userUpdateDef);
            await campaignsCollection.UpdateOneAsync(campaignFilter, campaignUpdateDef);

            return "Success";
        }
        else
            return "Error: Json Empty";
    }

    [HttpDelete("DeleteCampaign")]
    public async Task<string> DeleteCampaign()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");
        var usersCollection = database.GetCollection<User>("Users");

        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<CampaignData>(payload);

        if (payloadData?.id != null)
        {
            var campaignToDeleteFilter = Builders<CampaignData>.Filter.Eq(c => c.id, payloadData.id);
            var campaignToDelete = await campaignsCollection.Find(campaignToDeleteFilter).FirstOrDefaultAsync();

            if (campaignToDelete.creatorId == payloadData.creatorId && campaignToDelete != null) 
            {
                var usersInvitedFilter = Builders<User>.Filter.In(u => u.id, campaignToDelete.invitedPlayersID);
                var usersEnrolledFilter = Builders<User>.Filter.In(u => u.id, campaignToDelete.enrolledPlayersID);

                var updateDefInvited = Builders<User>.Update.Pull(u => u.invitedCampaignsID, campaignToDelete.id);
                var updateDefEnrolled = Builders<User>.Update.Pull(u => u.enrolledCampaignsID, campaignToDelete.id);

                await usersCollection.UpdateManyAsync(usersInvitedFilter, updateDefInvited);
                await usersCollection.UpdateManyAsync(usersEnrolledFilter, updateDefEnrolled);

                await campaignsCollection.DeleteOneAsync(campaignToDeleteFilter);

                return "Success";
            }
            else
            {
                return "Error: You are not the owner of this campaign";
            }
        }
        else
            return "Error: Json Missing";
    }

    [HttpPatch("DungeonMaster")]
    public async Task<string> AscendToDungeonMaster()
    {
        var database = client.GetDatabase(databaseName);
        var campaignsCollection = database.GetCollection<CampaignData>("QH_Campaigns");

        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<CampaignData>(payload);

        string campaignID = payloadData.id;
        string playerToAscend = payloadData.creatorId;

        if (campaignID != null && playerToAscend != null)
        {
            var campaignFilter = builder.Eq(c => c.id, campaignID);
            var updateDef = Builders<CampaignData>.Update.Push(c => c.dungeonMastersID, playerToAscend);

            await campaignsCollection.UpdateOneAsync(campaignFilter, updateDef);

            return "Success";

        }
        else
            return "Error: json is missing";

    }
}