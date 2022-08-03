using MapsAPI.Models;

namespace MapsAPI.Repositories;

public interface IDefaultMapsRepository
{
    Task<IEnumerable<Maps>> Get();
    Task<Maps> Get(string id);
    Task<Maps> Create(Maps maps);
    Task Update(Maps maps);
    Task Delete(string id);
}