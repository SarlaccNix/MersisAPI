using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Campaigns;

public class CampaignData
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }
    [BsonElement("name")] public string name {get; set;}
    [BsonElement("description")] public string description { get; set; }
    [BsonElement("creatorId")] public string creatorId {get; set;}
    [BsonElement("creatorName")] public string creatorName { get; set; }
    [BsonElement("creation_Date_Time")] public DateTime? creation_Date_Time { get; set; }
    [BsonElement("last_Edited_Date_Time")] public DateTime? last_Edited_Date_Time { get; set; }
    [BsonElement("Dungeon_Masters")] public string[] dungeonMastersID {  get; set; }
    [BsonElement("enrolled_Players_ID")] public string[] enrolledPlayersID { get; set; }
    [BsonElement("invited_Players_ID")] public string[] invitedPlayersID { get; set; }

}
