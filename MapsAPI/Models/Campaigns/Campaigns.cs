using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Campaigns;

public class Campaign
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }
    
    [BsonElement("sessionNumber")] public int sessionNumber {get; set;}

    [BsonElement("name")] public string name {get; set;}
    
    [BsonElement("creatorId")] public string creatorId {get; set;}
    
    [BsonElement("ruleset")] public string ruleset { get; set; }
    
    [BsonElement("ruleSetID")] public int ruleSetID { get; set; }
    
    [BsonElement("campaign_LengthID")] public int campaign_LengthID { get; set; }
    
    [BsonElement("length")] public string length { get; set; }
    
    [BsonElement("startTime")] public string startTime { get; set; }
    
    [BsonElement("endTime")] public string endTime { get; set; }
    
    [BsonElement("daysOfWeek")] public string daysOfWeek { get; set; }
    
    [BsonElement("currentPlayers")] public string currentPlayers { get; set; }

    [BsonElement("description")] public string description { get; set; }
    
    [BsonElement("roleplaying")] public int roleplaying { get; set; }
    
    [BsonElement("combat")] public int combat { get; set; }
    
    [BsonElement("playerAgency")] public int playerAgency { get; set; }
    
    [BsonElement("story")] public int story { get; set; }
    
    [BsonElement("world")] public int world { get; set; }
    
    [BsonElement("rules")] public int rules { get; set; }
    
    [BsonElement("newPlayers")] public int newPlayers { get; set; }
    
    [BsonElement("campaignOwner")] public string campaignOwner { get; set; }

    [BsonElement("campaignPlayers")] public string campaignPlayers { get; set; }
    
    [BsonElement("premadeCharacters")] public int premadeCharacters { get; set; }
    
    [BsonElement("matureContent")] public int matureContent { get; set; }
    
    [BsonElement("homebrew")] public int homebrew { get; set; }
    
    [BsonElement("languagesID")] public int languagesID { get; set; }
    
    [BsonElement("maxPlayers")] public int maxPlayers { get; set; }
    
    [BsonElement("gameRoom")] public string gameRoom { get; set; }
    
    [BsonElement("playstyle")] public string playstyle { get; set; }
    
    [BsonElement("playStyleID")] public string playStyleID { get; set; }
    
    [BsonElement("frequencyID")] public string frequencyID { get; set; }

}

public class CampaignsList
{
    [JsonProperty("CampaignsList")]
    public List<Campaign> CharactersList { get; set; }
}



public class CampaignData
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }

    [BsonElement("campaign")] public Campaign campaign { get; set; }

    [BsonElement("name")] public string name {get; set;}
    
    [BsonElement("creatorId")] public string creatorId {get; set;}

    [BsonElement("creation_Date_Time")] public DateTime? creation_Date_Time { get; set; }
    
    [BsonElement("last_Edited_Date_Time")] public DateTime? last_Edited_Date_Time { get; set; }
    
}
