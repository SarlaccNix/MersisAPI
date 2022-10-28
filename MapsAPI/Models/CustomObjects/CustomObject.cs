using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Models.CustomObjects;

public class CustomObject
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string prefabId { get; set; }

    [BsonElement("prefabName")] public string prefabName { get; set; }

    [BsonElement("keywords")] public int keywords { get; set; }
    
    [BsonElement("scale")] public Object scale { get; set; }

    [BsonElement("position")] public Object position { get; set; }

    [BsonElement("rotation")] public Object rotation { get; set; }

    [BsonElement("previewImage")] public byte[] previewImage { get; set; } 
    
    [BsonElement("description")] public string description { get; set; }

    [BsonElement("privacy")] public  bool privacy { get; set; }
    
    [BsonElement("children")] public Object[] children { get; set; }
}

public class CustomObjectList
{
    [JsonProperty("CustomObjectsList")]
    public List<CustomObject> CustomObjectsList { get; set; }
}