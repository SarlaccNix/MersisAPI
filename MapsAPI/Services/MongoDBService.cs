 using MapsAPI.Models;
 using MongoDB.Driver;
 using MongoDB.Bson;
 
 namespace MapsAPI.Services;

 public class MongoDBService
 {
     private readonly IMongoCollection<Maps> _mapsCollection;
 }