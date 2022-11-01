using MapsAPI.Characters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Models.CustomObjects;

public class CustomObjectData
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }
    [BsonElement("prefabName")] public string name { get; set; }
    [BsonElement("customObject")] public CustomObject customObject { get; set; }

    [BsonElement("creatorId")] public string creatorId { get; set; }
    
    [BsonElement("creatorName")] public string creatorName { get; set; }
    
    [BsonElement("tags")] public List<string> tags { get; set; }

    [BsonElement("privacy")] public bool privacy { get; set; }
    
    [BsonElement("downloads_quantity")] public int downloads_quantity { get; set; }
    [BsonElement("last_Edited_Date_Time")] public  DateTime last_Edited_Date_Time { get; set; }
    [BsonElement("creation_Date_Time")] public DateTime creation_Date_Time { get; set; }
    
    
}

public class CustomObject
{
    [BsonElement("prefabName")] public string prefabName { get; set; }
    
    [BsonElement("prefabId")] public string prefabId { get; set; }
    
    [BsonElement("keywords")] public List<string> keywords { get; set; }
    
    [BsonElement("scale")] public XYZ scale { get; set; }

    [BsonElement("position")] public XYZ position { get; set; }

    [BsonElement("rotation")] public XYZ rotation { get; set; }

    [BsonElement("children")] public CustomObject[] children { get; set; }
}

public class CustomObjectList
{
    [JsonProperty("CustomObjectsList")]
    public List<CustomObject> CustomObjectsList { get; set; }
}