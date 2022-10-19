using MapsAPI.Models;

namespace MapsAPI.Repositories;

public interface IMap
{
    Task<IEnumerable<Map>> Get();
    Task<Map> GetMapById(string id);
    Task<Map> CreateMap(Map maps);
    Task Update(Map maps);

    Task Delete(string id);
    // Task SearchMap()
}