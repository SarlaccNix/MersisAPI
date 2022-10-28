using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MapsAPI.Characters;

public class Character
{
    [BsonElement("figureName")] public string figureName { get; set; }

    [BsonElement("characterGender")] public int characterGender { get; set; }
    
    [BsonElement("primaryColor")] public object primaryColor { get; set; }

    [BsonElement("secondaryColor")] public object secondaryColor { get; set; }

    [BsonElement("hairColor")] public object hairColor { get; set; }

    [BsonElement("skinColor")] public object skinColor { get; set; } 
    
    [BsonElement("stubbleColor")] public object stubbleColor { get; set; }
    
    [BsonElement("scarColor")] public  object scarColor { get; set; }
    
    [BsonElement("hair")] public string hair { get; set; }
    
    [BsonElement("HeadCovering")] public string HeadCovering { get; set; }
    
    [BsonElement("earType")] public string earType { get; set; }
    
    [BsonElement("torso")] public string torso { get; set; }
    
    [BsonElement("hips")] public string hips { get; set; }
    
    [BsonElement("eyebrows")] public string eyebrows { get; set; }
    
    [BsonElement("face")] public string face { get; set; }
    
    [BsonElement("nose")] public string nose { get; set; }
    
    [BsonElement("helmet")] public string helmet { get; set; }
    
    [BsonElement("facialHair")] public string facialHair { get; set; }
    
    [BsonElement("upperRightArm")] public string upperRightArm { get; set; }
    
    [BsonElement("upperLeftArm")] public string upperLeftArm { get; set; }
    
    [BsonElement("lowerRightArm")] public string lowerRightArm { get; set; }
    
    [BsonElement("lowerLeftArm")] public string lowerLeftArm { get; set; }
    
    [BsonElement("handLeft")] public string handLeft { get; set; }
    
    [BsonElement("handRight")] public string handRight { get; set; }
    
    [BsonElement("lowerLegRight")] public string lowerLegRight { get; set; }
    
    [BsonElement("lowerLegLeft")] public string lowerLegLeft { get; set; }
    
    [BsonElement("rightHandAttachment")] public string rightHandAttachment { get; set; }
    
    [BsonElement("leftHandAttachment")] public string leftHandAttachment { get; set; }
    
    [BsonElement("elbowAttachmentRight")] public string elbowAttachmentRight { get; set; }
    
    [BsonElement("elbowAttachmentLeft")] public string elbowAttachmentLeft { get; set; }
    
    [BsonElement("hipAttachment")] public string hipAttachment { get; set; }
    
    [BsonElement("kneeAttachmentRight")] public string kneeAttachmentRight { get; set; }
    
    [BsonElement("shoulderAttachmentRight")] public string shoulderAttachmentRight { get; set; }
    
    [BsonElement("shoulderAttachmentLeft")] public string shoulderAttachmentLeft { get; set; }
    
    [BsonElement("backAttachment")] public string backAttachment { get; set; }
    
    [BsonElement("helmetAttachment")] public string helmetAttachment { get; set; }
    
    [BsonElement("figureBase")] public string figureBase { get; set; }
    
    [BsonElement("idleAnimationController")] public string idleAnimationController { get; set; }
    
    [BsonElement("figureBaseScale")] public object figureBaseScale { get; set; }
    
    [BsonElement("figureScale")] public object figureScale { get; set; }
    
    [BsonElement("eyesColor")] public object eyesColor { get; set; }
    
    [BsonElement("bodyArtColor")] public object bodyArtColor { get; set; }
    
    [BsonElement("leatherPrimaryColor")] public object leatherPrimaryColor { get; set; }
    
    [BsonElement("leatherSecondaryColor")] public object leatherSecondaryColor { get; set; }
    
    [BsonElement("metalPrimaryColor")] public object metalPrimaryColor { get; set; }
    
    [BsonElement("metalSecondaryColor")] public object metalSecondaryColor { get; set; }
    
    [BsonElement("metalDarkColor")] public object metalDarkColor { get; set; }
}

public class CharacterList
{
    [JsonProperty("CharactersList")]
    public List<Character> CharactersList { get; set; }
}

public class RGBa
{
    public double r;
    public double g;
    public double b;
    public double a;
}

public class XYZ
{
    public double x;
    public double y;
    public double z;
}

public class CharacterData
{
    [BsonId] [BsonRepresentation(BsonType.ObjectId)] public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)] public string parentCharacterId { get; set; }
    
    [BsonElement("character")] public Character Character { get; set; }
    
    [BsonElement("characterName")] public string name {get; set;}
    [BsonElement("creatorId")] public string CreatorId { get; set; }
    
    [BsonElement("creatorName")] public string CreatorName { get; set; }
    [BsonElement("downloaded_qty")] public int Downloads_Quantity { get; set; }
    [BsonElement("creation_Date_Time")] public DateTime? Creation_Date_Time { get; set; }
    [BsonElement("last_Edited_Date_Time")] public DateTime? Last_Edited_Date_Time { get; set; }
    
    [BsonElement("favorites")] public int Favorites { get; set; }
    
    [BsonElement("privacy")] public  bool privacy { get; set; }
    [BsonElement("tags")] public List<string> tags { get; set; }
}