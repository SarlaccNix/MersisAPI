using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Models;

public class MapsList
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; }
    

    [BsonElement("mapName")] public string MapName { get; set; } = null!;

    [BsonElement("creatorId")] public string CreatorId { get; set; }

    [BsonElement("favorites")] public int Favorites { get; set; }

    [BsonElement("downloaded_qty")] public int Downloaded_Quantity { get; set; }
    
    [BsonElement("mapPreviewImage")] public byte[] MapPreview { get; set; } 
    
    [BsonElement("mapDescription")] public string MapDescription { get; set; }
    
    [BsonElement("tags")] public string Tags { get; set; }
    
    [BsonElement("MapVersion")] public int MapVersion { get; set; }

    [JsonIgnore]
    [BsonElement("mapFile")] public byte[] MapFile { get; set; } = null!;
    
    [JsonIgnore]
    [BsonElement("creation_Date_Time")] public DateTime Creation_Date_Time { get; set; }
    
    [JsonIgnore]
    [BsonElement("last_Edited_Date_Time")] public DateTime Last_Edited_Date_Time { get; set; }

}