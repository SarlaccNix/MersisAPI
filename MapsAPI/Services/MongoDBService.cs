 using MapsAPI.Repositories;
 using MongoDB.Driver;
 using MongoDB.Bson;
 using MapsAPI.Models;
using AutoMapper;
 
 namespace MapsAPI.Services;

 public class MapsService : IMap
 {
     private IMongoCollection<Map> mapsCollection;
     private IMapper _mapper;

     public MapsService(IMapper _mapper, IConfiguration config)
     {
         // _mapper = mapper;
         var mongoClient = new MongoClient(config.GetConnectionString("ConnectionURI"));
         var mongoDatabase =  mongoClient.GetDatabase("QH_Maps_Default");
         mapsCollection = mongoDatabase.GetCollection<Map>("QH_Maps");
     }
     public Task<IEnumerable<Map>> Get()
     {
         throw new NotImplementedException();
     }

     public Task<Map> GetMapById(string id)
     {
         throw new NotImplementedException();
     }

     public Task<Map> CreateMap(Map maps)
     {
         throw new NotImplementedException();
     }

     public Task Update(Map maps)
     {
         throw new NotImplementedException();
     }

     public Task Delete(string id)
     {
         throw new NotImplementedException();
     }
 }
 

 