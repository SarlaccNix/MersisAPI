using MapsAPI.Characters;
using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MapsAPI.CharacterSheetsData;
using MapsAPI.CharacterSheets;

namespace MapsAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class CharacterSheetController : ControllerBase
{
    private string databaseName = "QuestHaven";

    private MongoClient client =
        new MongoClient(
            "mongodb+srv://doadmin:fuzs536H0R9P124y@db-mongodb-nyc1-91572-de834d8c.mongo.ondigitalocean.com/admin?tls=true&authSource=admin");


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
                creation_Date = sheet.creationDate,
                last_Update = sheet.lastUpdate
            };
        }
        return new OkObjectResult(response);
    }

    [HttpPost("searchCharacterSheetsByUserID")]
    public async Task<Object> SearchCharacterSheetsByUserID()
    {
        var database = client.GetDatabase(databaseName);
        var sheetTemplateCollection = database.GetCollection<CharacterSheetsTemplateModel>("QH_CharacterSheetsTemplates");
        var payload = String.Empty;

        object error = new
        {
            Error = "Error, no match found using the current searching criteria"
        };

        var filter = Builders<CharacterSheetsTemplateModel>.Filter.Empty;

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
            filter = Builders<CharacterSheetsTemplateModel>.Filter.Regex("creatorId", new(creatorId, "i"));
        }

        List<CharacterSheetsTemplateModel> SheetTemplateResult = await sheetTemplateCollection.Find(filter).Skip((currentPage - 1) * currentPagination)
            .Limit(currentPagination).ToListAsync();

        List<CharacterSheetsTemplateModel> searchedSheetTemplates = new List<CharacterSheetsTemplateModel>();

        if (SheetTemplateResult != null)
        {
            foreach (CharacterSheetsTemplateModel sheetTemplate in SheetTemplateResult)
            {
                searchedSheetTemplates.Add(new CharacterSheetsTemplateModel()
                {
                    id = sheetTemplate.id,
                    name = sheetTemplate.name,
                    lastUpdate = sheetTemplate.lastUpdate,
                    creationDate = sheetTemplate.creationDate,
                    characterSheetTemplate = sheetTemplate.characterSheetTemplate
                });
            }
        }


        if (SheetTemplateResult != null && SheetTemplateResult.Any())
        {
            return  new
            {
                // characters found on search
                sheetTemplates = searchedSheetTemplates,
                // Amount of characters in the current page
                Count = SheetTemplateResult.Count,
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

}