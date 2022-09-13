using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MapsAPI.Models;

public class Maps
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string Id { get; set; }
    [BsonElement("mapName")] public string MapName { get; set; } = null!;
    [BsonElement("creatorId")] public string CreatorId { get; set; }
    [BsonElement("mapFile")] public byte[] MapFile { get; set; } = null!;

    [BsonElement("favorites")] public int Favorites { get; set; }

    [BsonElement("downloaded_qty")] public int Downloaded_Quantity { get; set; }

    [BsonElement("creation_Date_Time")] public DateTime Creation_Date_Time { get; set; }

    [BsonElement("last_Edited_Date_Time")] public DateTime Last_Edited_Date_Time { get; set; }
    
    [BsonElement("mapPreviewImage")] public byte[] MapPreview { get; set; } 
    
    [BsonElement("mapDescription")] public string MapDescription { get; set; }

}