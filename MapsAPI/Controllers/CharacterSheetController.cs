using MapsAPI.Characters;
using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MapsAPI.CharacterSheets;
using MapsAPI.Models;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class CharacterSheetController : ControllerBase
{
    private string databaseName = "QuestHaven";

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");



    [HttpPost("characterSheetTemplate")]
    public string NewCharacterSheetTemplate()
    {

        var database = client.GetDatabase(databaseName);
        var templateCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        CharacterSheetsTemplateModel sheetTemplateData = JsonConvert.DeserializeObject<CharacterSheetsTemplateModel>(payload);

        if (sheetTemplateData == null)
        {
            return "Error: missing JSON data";
        }

        sheetTemplateData.creationDate= DateTime.Now;
        sheetTemplateData.lastUpdate = DateTime.Now;
         

        try
        {
            templateCollection.InsertOne(sheetTemplateData);
            return "Character Sheet Template added to Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }

    }

    [HttpPatch("characterSheetTemplate")]
    public async Task<string> UpdateCharacterSheetTemplate()
    {

        var database = client.GetDatabase(databaseName);
        var templateCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        CharacterSheetsTemplateModel sheetsTemplateModel = JsonConvert.DeserializeObject<CharacterSheetsTemplateModel>(payload);

        if (sheetsTemplateModel == null)
        {
            return "Error: missing JSON data";
        }


        sheetsTemplateModel.lastUpdate = DateTime.Now;  

        var sheetTemplate = Builders<CharacterSheetsTemplateModel>.Filter.Eq("id", sheetsTemplateModel.id);
        var updateDefinition = Builders<CharacterSheetsTemplateModel>.Update
            .Set(s => s.characterSheetTemplate, sheetsTemplateModel.characterSheetTemplate)
            .Set(s => s.lastUpdate, sheetsTemplateModel.lastUpdate);

        try
        {
            var update = await templateCollection.UpdateOneAsync(sheetTemplate, updateDefinition);
            
            return "Character sheet Template updated in Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }

    }

    [HttpPost("characterSheetUserData")]
    public string NewCharacterSheetUserData()
    {

        var database = client.GetDatabase(databaseName);
        var sheetUserDataCollection = database.GetCollection<CharacterSheetData>("QH_CharacterSheetsData");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        CharacterSheetData sheetUserData = JsonConvert.DeserializeObject<CharacterSheetData>(payload);

        if (sheetUserData == null)
        {
            return "Error: missing JSON data";
        }

        sheetUserData.creationDate = DateTime.Now;
        sheetUserData.lastUpdate = DateTime.Now;


        try
        {
            sheetUserDataCollection.InsertOne(sheetUserData);
            return "Character Sheet user Data added to Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }

    }

    [HttpPatch("characterSheetUserData")]
    public async Task<string> UpdateCharacterSheetUserData()
    {

        var database = client.GetDatabase(databaseName);
        var userDataCollection = database.GetCollection<CharacterSheetData>("QH_CharacterSheetsData");
        var payload = String.Empty;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        CharacterSheetData sheetData = JsonConvert.DeserializeObject<CharacterSheetData>(payload);

        if (sheetData == null)
        {
            return "Error: missing JSON data";
        }


        sheetData.lastUpdate = DateTime.Now;

        if (string.IsNullOrEmpty(sheetData.characterID))
            sheetData.characterID = "";


        var sheetTemplate = Builders<CharacterSheetData>.Filter.Eq("id", sheetData.id);
        var updateDefinition = Builders<CharacterSheetData>.Update
            .Set(s => s.characterSheetUserData, sheetData.characterSheetUserData)
            .Set(s=> s.name, sheetData.name)
            .Set(s => s.lastUpdate, sheetData.lastUpdate)
            .Set(s => s.characterID, sheetData.characterID)
            .Set(s => s.characterSheetTemplateID, sheetData.characterSheetTemplateID);


        try
        {
            var update = await userDataCollection.UpdateOneAsync(sheetTemplate, updateDefinition);

            return "Character sheet data updated in Database";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
            return "Error: " + e;
        }

    }

    [HttpPost("getCharacterSheetTemplateById")]
    public async Task<OkObjectResult> getCharacterSheetTemplate()
    {
        //Se definen variables y se recibe la data llamado al endpoint
        var payload = String.Empty;
        object? response;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        //Aqui se conecta al indice de la base de datos correspondiente
        var database = client.GetDatabase(databaseName);
        var sheetsCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        //Aqui se busca en la base datos la data que se requiere
        var sheet = await sheetsCollection
            .Find(Builders<CharacterSheetsTemplateModel>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();
        if (sheet == null)
        {
            response = new
            {
                error = "ID does not match any file in the database."
            };
        }
        else
        {
            //Aqui se arma la respuesta que se le devuelve al juego
            response = new
            {
                id = sheet.id,
                name = sheet.name,
                character_Sheet_Template = sheet.characterSheetTemplate,
                creatorID = sheet.creatorID,
                creation_Date = sheet.creationDate,
                last_Update = sheet.lastUpdate
            };
        }
        return new OkObjectResult(response);
    }

    [HttpPost("getCharacterSheetDataById")]
    public async Task<OkObjectResult> getCharacterSheetUser()
    {
        //Se definen variables y se recibe la data llamado al endpoint
        var payload = String.Empty;
        object? response;
        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadData = JsonConvert.DeserializeObject<dynamic>(payload);
        String id = payloadData.id;
        //Aqui se conecta al indice de la base de datos correspondiente
        var database = client.GetDatabase(databaseName);
        var sheetsDataCollection = database.GetCollection<CharacterSheetData>("QH_CharacterSheetsData");
        //Aqui se busca en la base datos la data que se requiere
        var sheetData = await sheetsDataCollection
            .Find(Builders<CharacterSheetData>.Filter.Eq("_id", ObjectId.Parse(id)))
            .FirstOrDefaultAsync();

        if (sheetData == null)
        {
            response = new
            {
                error = "ID does not match any file in the database."
            };
        }
        else
        {
            //Aqui se arma la respuesta que se le devuelve al juego
            response = new
            {
                id = sheetData.id,
                name = sheetData.name,
                characterSheetTemplateID = sheetData.characterSheetTemplateID,
                characterSheetUserData = sheetData.characterSheetUserData,
                creatorID = sheetData.creatorID,
                characterID = sheetData.characterID,
                creation_Date = sheetData.creationDate,
                last_Update = sheetData.lastUpdate
            };
        }
        return new OkObjectResult(response);
    }


    [HttpPost("searchCharacterSheetsByUserID")]
    public async Task<Object> SearchCharacterSheetsByUserID()
    {
        var database = client.GetDatabase(databaseName);
        var sheetTemplateCollection = database.GetCollection<CharacterSheetData>("QH_CharacterSheetsData");
        var payload = String.Empty;

        object error = new
        {
            Error = "Error, no match found using the current searching criteria"
        };

        var filter = Builders<CharacterSheetData>.Filter.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
        string creatorId = payloadJson.creatorId;

        var find = sheetTemplateCollection.Find(filter);
        int currentPage = 1, currentPagination = 10;

        if (payloadJson.Pagination != null)
        {
            currentPagination = payloadJson.Pagination == 0 ? 10 : payloadJson.Pagination;
        }

        if (payloadJson.Page != null)
        {
            currentPage = payloadJson?.Page == 0 ? 1 : payloadJson.Page;
        }


        if (!string.IsNullOrEmpty(creatorId))
        {
            filter = Builders<CharacterSheetData>.Filter.Regex(c => c.creatorID, new(creatorId, "i"));
        }

        List<CharacterSheetData> SheetDataResult = await sheetTemplateCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();


        if (SheetDataResult != null && SheetDataResult.Any())
        {
            return  new
            {
                // Character Sheets found for the current user ID search
                characterSheetsData = SheetDataResult,
                // Amount of characters in the current page
                Count = SheetDataResult?.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                Hits = await sheetTemplateCollection.Find(filter).CountDocumentsAsync(),
            };
        }

        return error;
    }

    [HttpPost("SearchSheetTemplates")]
    public async Task<Object> SearchMaps()
    {
        var database = client.GetDatabase(databaseName);
        var characterSheetTemplateCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        var payload = String.Empty;
        object response;
        object error = new
        {
            Error = "Error, no match found using the current searching criteria"
        };

        FilterDefinitionBuilder<CharacterSheetsTemplateModel> builder = Builders<CharacterSheetsTemplateModel>.Filter;
        var filter = builder.Empty;
        List<CharacterSheetsTemplateModel> sheetTemplatesResults = null;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }

        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
        string searchText = payloadJson.SearchText;
        string userId = payloadJson.creatorId;
        var find = characterSheetTemplateCollection.Find(filter);
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


        if (!string.IsNullOrEmpty(userId))
        {
            var userFilter = builder.Regex("creator_ID", new(userId, "i"));
            filter &= userFilter;
        }

        sheetTemplatesResults = await characterSheetTemplateCollection.Find(filter).Skip((currentPage - 1) * currentPagination).Limit(currentPagination).Sort("{last_Edited_Date_Time: -1}").ToListAsync();
        var searchedSheetTemplates = new List<SearchSheetTemplate>();

        if (sheetTemplatesResults != null)
        {
            foreach (CharacterSheetsTemplateModel SheetTemplate in sheetTemplatesResults)
            {
                searchedSheetTemplates.Add(new SearchSheetTemplate()
                {
                        id = SheetTemplate.id,
                        name = SheetTemplate.name,
                        lastUpdate = SheetTemplate.lastUpdate,
                        creationDate = SheetTemplate.creationDate,
                        creatorID = SheetTemplate.creatorID,
                });
            }
        }

        // return new OkObjectResult(wrappedMapResults);
        if (sheetTemplatesResults != null && sheetTemplatesResults.Any())
        {
            return response = new
            {
                // maps found on search
                characterSheetTemplates = searchedSheetTemplates,
                // Amount of maps in the current page
                Count = sheetTemplatesResults.Count,
                // Selected page number
                Page = currentPage,
                // Selected amount of items per page
                Pagination = currentPagination,
                // Amount of matches for current search
                hits = await characterSheetTemplateCollection.Find(filter).CountDocumentsAsync(),
            };
        }

        return error;
    }

    [HttpDelete("DeleteSheetTemplate")]
    public async Task<string> DeleteSheetTemplate()
    {
        var database = client.GetDatabase(databaseName);
        var characterSheetTemplateCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
        string id = payloadJson.id;

        var filter = Builders<CharacterSheetsTemplateModel>.Filter.Eq(s => s.id, id);
        var sheetTemplate = await characterSheetTemplateCollection.Find(filter).FirstOrDefaultAsync();

        if (sheetTemplate != null) 
        {
            characterSheetTemplateCollection.DeleteOne(filter);
            return sheetTemplate.name + " successfully deleted";
        }else
        {
            return "Error: This template doesn't exist in DB";
        }
    }

    [HttpDelete("DeleteSheetUserData")]
    public async Task<string> DeleteSheetUserData()
    {
        var database = client.GetDatabase(databaseName);
        var characterSheetDataCollection = database.GetCollection<CharacterSheetData>("QH_CharacterSheetsData");
        var payload = String.Empty;

        using (StreamReader reader = new StreamReader(Request.Body))
        {
            payload = reader.ReadToEndAsync().Result;
        }
        var payloadJson = JsonConvert.DeserializeObject<dynamic>(payload);
        string id = payloadJson.id;

        var filter = Builders<CharacterSheetData>.Filter.Eq(s => s.id, id);
        var sheetData = await characterSheetDataCollection.Find(filter).FirstOrDefaultAsync();

        if (sheetData != null)
        {
            characterSheetDataCollection.DeleteOne(filter);
            return sheetData.name + " successfully deleted";
        }
        else
        {
            return "Error: This sheet Data doesn't exist in DB";
        }
    }

}