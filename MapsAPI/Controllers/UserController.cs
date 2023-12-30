namespace MapsAPI.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Models.Users;
using System.Text;

[ApiController]
[Route("[controller]")]

public class UserController : ControllerBase

{
    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");
    private string databaseName = "QuestHaven";
    [HttpPost("login")]
    public async Task<OkObjectResult> Login()
    {

        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        User loginData = JsonConvert.DeserializeObject<User>(payload);
        if (loginData == null || string.IsNullOrEmpty(loginData.id))
        {
            var error = new
            {
                error = "Error: missing id"
            };
            return new OkObjectResult(error);
        }

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", loginData.id);
        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq("_id", loginData.id))
            .FirstOrDefaultAsync();
        if (currentUser == null)
        {
            try
            {
                string[] defaultAvatarsID = { "default_A", "default_B", "default_C" };
                var newUser = loginData;
                Random randomizer = new Random();
                // string tagValue = randomizer.Next(1000, 9999).ToString();
                // newUser.qh_UserTag = newUser.username + "#" + tagValue;
                newUser.profilePicIndex = randomizer.Next(0, 6);
                newUser.creation_Date_Time = DateTime.Now;
                newUser.last_Login_Date_Time = DateTime.Now;
                newUser.avatarID = defaultAvatarsID[randomizer.Next(0, defaultAvatarsID.Length)];
                usersCollection.InsertOne(newUser);
                var newUserAdded = new
                {
                    id = newUser.id,
                    username = newUser.username,
                    qh_UserTag = newUser.qh_UserTag,
                    creation_Date_Time = newUser.creation_Date_Time,
                    last_Login_Date_Time = newUser.last_Login_Date_Time,
                    profilePicIndex = newUser.profilePicIndex,
                    avatarId = newUser.avatarID
                };
                return new OkObjectResult(newUserAdded);
            }
            catch (Exception e)
            {
                var error = new
                {
                    error = "Error: " + e
                };
                return new OkObjectResult(error);
            }
        }

        var update = Builders<User>.Update.Set("last_login_Date_Time", DateTime.Now);
        var fetchUpdate = await usersCollection.UpdateOneAsync(filter, update);
        var updatedLogin = new
        {
            id = currentUser.id,
            username = currentUser.username,
            qh_UserTag = currentUser.qh_UserTag,
            creation_Date_Time = currentUser.creation_Date_Time,
            last_Login_Date_Time = currentUser.last_Login_Date_Time,
            profilePicIndex = currentUser.profilePicIndex,
            avatarId = currentUser.avatarID
        };
        return new OkObjectResult(updatedLogin);
    }

    [HttpPost("username")]
    public async Task<OkObjectResult> CreateQhUsername()
    {
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;
        User updatedData = new User();

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        User userData = JsonConvert.DeserializeObject<User>(payload);
        if (userData == null || string.IsNullOrEmpty(userData.id) || string.IsNullOrEmpty(userData.username))
        {
            var error = new
            {
                error = "Error: missing id and/or User Tag"
            };
            return new OkObjectResult(error);
        }

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", userData.id);
        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq("_id", userData.id))
            .FirstOrDefaultAsync();
        if (currentUser == null)
        {
            var error = new
            {
                error = "Error: user does not exist, please login the user in order to create its index."
            };
            return new OkObjectResult(error);
        }

        if (!string.IsNullOrEmpty(userData.username))
        {
            Random randomizer = new Random();
            string tagValue = randomizer.Next(1000, 9999).ToString();
            currentUser.qh_UserTag = userData.username + "#" + tagValue;
            var update = Builders<User>.Update.Set("qh_UserTag", currentUser.qh_UserTag)
                .Set("username", userData.username);
            var fetchUpdate = await usersCollection.UpdateOneAsync(filter, update);
            if (fetchUpdate.MatchedCount == 1)
            {
                updatedData.id = userData.id;
                updatedData.username = userData.username;
                updatedData.qh_UserTag = currentUser.qh_UserTag;
            }
            else
            {
                var error = new
                {
                    error = "Error: user does not exist, please login the user in order to create its index."
                };
                return new OkObjectResult(error);
            }


        }

        var updatedUser = new
        {
            id = updatedData.id,
            username = updatedData.username,
            qh_UserTag = updatedData.qh_UserTag,
        };
        return new OkObjectResult(updatedUser);
    }

    [HttpPatch("profilePicture")]
    public async Task<string> UpdateProfilePicture()
    {
        string error = string.Empty;
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;
        User updatedData = new User();

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        User user = JsonConvert.DeserializeObject<User>(payload);

        if (user == null || string.IsNullOrEmpty(user.id))
        {

            error = "Error: missing id";

            return error;
        }

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", user.id);
        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq("_id", user.id))
            .FirstOrDefaultAsync();

        var update = Builders<User>.Update.Set("profile_picture_index", user.profilePicIndex);
        var fetchUpdate = await usersCollection.UpdateOneAsync(filter, update);
        return "Profile picture updated.";
    }

    [HttpPatch("avatarID")]
    public async Task<string> UpdateAvatarID()
    {
        string error = string.Empty;
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;
        User updatedData = new User();

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        User user = JsonConvert.DeserializeObject<User>(payload);

        if (user == null || string.IsNullOrEmpty(user.id))
        {

            error = "Error: missing id";

            return error;
        }

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", user.id);
        var currentUser = await usersCollection
            .Find(Builders<User>.Filter.Eq("_id", user.id))
            .FirstOrDefaultAsync();

        var update = Builders<User>.Update.Set("avatar_ID", user.avatarID);
        var fetchUpdate = await usersCollection.UpdateOneAsync(filter, update);
        return "Avatar ID updated.";
    }

    [HttpPatch("addField")]
    public async void AddField()
    {
        Random random = new Random();

        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var filterDefinition = Builders<User>.Filter.Where(w => w.qh_UserTag != null);
        var users = usersCollection.Find(filterDefinition).ToList();
        foreach( var user in users)
        {
            var index = random.Next(0, 6);
            var userfilter = Builders<User>.Filter.Eq(doc => doc.qh_UserTag, user.qh_UserTag);
            var updateDefinition = Builders<User>.Update
                .Set(d => d.profilePicIndex, index);

            usersCollection.UpdateOne(userfilter, updateDefinition);
        }

    }

    [HttpPost("likedMaps")]
    public async Task<OkObjectResult> GetLikedMaps()
    {
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;
        var error = new
        {
            error = "Error: missing id"
        };
        var idNotFound = new
        {
            error = "Error: ID does not match any user in the DB"
        };
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        User userData = JsonConvert.DeserializeObject<User>(payload);

        if (userData == null || string.IsNullOrEmpty(userData.id))
        {
            return new OkObjectResult(error);
        }

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", userData.id);
        var currentUser = await usersCollection
            .Find(filter)
            .FirstOrDefaultAsync();
        if (currentUser != null)
        {
            var userLikedMaps = new
            {
                id = currentUser.id,
                username = currentUser.username,
                likedMaps = currentUser.likedMaps,
            };
            return new OkObjectResult(userLikedMaps);
        }

        return new OkObjectResult(idNotFound);
    }

    [HttpPost("FetchUserName")]
    public async Task<string> GetUserName()
    {
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        var id = JsonConvert.DeserializeObject<dynamic>(payload);

        FilterDefinition<User> filter = Builders<User>.Filter.Eq("_id", id);

        var user = await usersCollection.Find(filter).FirstOrDefaultAsync();

        if (user != null) 
        { 
            return user.username;
        }else
        {
            return null;
        }

    }

    [HttpPost("FetchUserWithUserTag")]
    public async Task<OkObjectResult> GetUserInfoWithUserTag()
    {
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var serverUser = JsonConvert.DeserializeObject<User>(payload);
        FilterDefinition<User> filter = Builders<User>.Filter.Eq(user => user.qh_UserTag, serverUser.qh_UserTag);

        var user = await usersCollection.Find(filter).FirstOrDefaultAsync();

        if (user != null)
        {
            var ServerUserData = new
            {
                username = user.username,
                profilePicIndex = user.profilePicIndex,
                qh_UserTag = user.qh_UserTag,
                id = user.id
            };

            return new OkObjectResult(ServerUserData);
        }
        else
            return null;
    }

    [HttpPost("FetchUsersWithUsersID")]
    public async Task<User[]> GetUsersInfoWithUserID()
    {
        var database = client.GetDatabase(databaseName);
        var usersCollection = database.GetCollection<User>("Users");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        
        var UsersIDsArray = JsonConvert.DeserializeObject<string[]>(payload);

        if(UsersIDsArray != null) 
        {
            var filter = Builders<User>.Filter.In(u => u.id, UsersIDsArray);
            var users = await usersCollection.Find(filter).ToListAsync();
            return users.ToArray();
        }
        else
        {
            return null;
        }

    }
}