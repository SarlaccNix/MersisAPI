namespace MapsAPI.Models.Users;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId][BsonElement("id")] public string id { get; set; }
    [BsonElement("username")] public string username { get; set; }
    
    [BsonElement("likedMaps")] public string[] likedMaps { get; set; }
    
    [BsonElement("bookmarkedMaps")] public string[] bookmarkedMaps { get; set; }
    [BsonElement("qh_UserTag")] public string qh_UserTag { get; set; }
    [BsonElement("creation_Date_Time")] public DateTime? creation_Date_Time { get; set; }
    [BsonElement("last_login_Date_Time")] public DateTime? last_Login_Date_Time { get; set; }
    [BsonElement("profile_picture_index")] public int profilePicIndex { get; set; }

    [BsonElement("avatar_ID")] public string avatarID { get; set; }
    [BsonElement("enrolledCampaigns")] public string[] enrolledCampaignsID { get; set;}
    [BsonElement("invitedCampaigns")] public string[] invitedCampaignsID { get; set; }

}