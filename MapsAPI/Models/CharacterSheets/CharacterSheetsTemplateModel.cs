using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace MapsAPI.CharacterSheets;

public class CharacterSheetsTemplateModel
{
    [BsonId][BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }

    [BsonElement("name")] public string name { get; set; }
    [BsonElement("last_Update")] public DateTime lastUpdate { get; set; }
    [BsonElement("creation_Date")] public DateTime creationDate { get; set;}
    [BsonElement("creator_ID")] public string creatorID { get; set; }
    [BsonElement("character_Sheet_Template")] public byte[] characterSheetTemplate { get; set;}

}

