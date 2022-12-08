﻿namespace MapsAPI.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Models.Users;

[ApiController]
[Route("[controller]")]

public class UserController : ControllerBase

{
    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");

    [HttpPost("login")]
    public async Task<OkObjectResult> Login()
    {

        var database = client.GetDatabase("QuestHaven");
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
                var newUser = loginData;
                // Random randomizer = new Random();
                // string tagValue = randomizer.Next(1000, 9999).ToString();
                // newUser.qh_UserTag = newUser.username + "#" + tagValue;
                newUser.creation_Date_Time = DateTime.Now;
                newUser.last_Login_Date_Time = DateTime.Now;
                usersCollection.InsertOne(newUser);
                var newUserAdded = new
                {
                    id = newUser.id,
                    username = newUser.username,
                    qh_UserTag = newUser.qh_UserTag,
                    creation_Date_Time = newUser.creation_Date_Time,
                    last_Login_Date_Time = newUser.last_Login_Date_Time
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
            last_Login_Date_Time = currentUser.last_Login_Date_Time
        };
        return new OkObjectResult(updatedLogin);
    }

    [HttpPost("username")]
    public async Task<OkObjectResult> CreateQhUsername()
    {
        var database = client.GetDatabase("QuestHaven");
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
            var update = Builders<User>.Update.Set("qh_UserTag", currentUser.qh_UserTag).Set("username", userData.username);
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
}