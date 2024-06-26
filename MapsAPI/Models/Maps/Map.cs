using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Models;

public class Map
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; }
    [BsonElement("mapName")] public string MapName { get; set; } = null!;
    [BsonElement("creatorId")] public string CreatorId { get; set; }
    [BsonElement("creatorName")] public string CreatorName { get; set; }
    [BsonElement("mapFile")] public byte[] MapFile { get; set; } = null!;

    [BsonElement("likes")] public int Likes { get; set; }

    [BsonElement("downloaded_qty")] public int Downloads_Quantity { get; set; }

    [BsonElement("creation_Date_Time")] public DateTime? Creation_Date_Time { get; set; }

    [BsonElement("last_Edited_Date_Time")] public DateTime? Last_Edited_Date_Time { get; set; }
    
    [BsonElement("mapPreviewImage")] public byte[] MapPreview { get; set; } 
    
    [BsonElement("mapDescription")] public string MapDescription { get; set; }
    
    [BsonElement("tags")] public List<string> tags { get; set; }

    [BsonElement("privacy")] public  bool privacy { get; set; }
    [BsonElement("MapVersion")] public int MapVersion { get; set; }
}

public class MapList
{
    [JsonProperty("Maps")]
    public List<Map> Maps { get; set; }
}

public class SearchMap
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; }
    
    [BsonElement("mapName")] public string MapName { get; set; } = null!;
    
    [BsonElement("creatorId")] public string CreatorId { get; set; }
    
    [BsonElement("creatorName")] public string CreatorName { get; set; }

    [BsonElement("likes")] public int Likes { get; set; }

    [BsonElement("downloaded_qty")] public int Downloads_Quantity { get; set; }
    
    [BsonElement("creation_Date_Time")] public DateTime? Creation_Date_Time { get; set; }

    [BsonElement("last_Edited_Date_Time")] public DateTime? Last_Edited_Date_Time { get; set; }
    
    [BsonElement("mapPreviewImage")] public byte[] MapPreview { get; set; } 
    
    [BsonElement("mapDescription")] public string MapDescription { get; set; }
    
    [BsonElement("tags")] public List<string> tags { get; set; }
    
    [BsonElement("MapVersion")] public int MapVersion { get; set; }
}