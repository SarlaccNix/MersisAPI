using MapsAPI.Models;

namespace MapsAPI.Repositories;

public class DefaultMapsRepository : IDefaultMapsRepository
{
    public Task<IEnumerable<Maps>> Get()
    {
        throw new NotImplementedException();
    }

    public Task<Maps> Get(string id)
    {
        throw new NotImplementedException();
    }

    public Task<Maps> Create(Maps maps)
    {
        throw new NotImplementedException();
    }

    public Task Update(Maps maps)
    {
        throw new NotImplementedException();
    }

    public Task Delete(string id)
    {
        throw new NotImplementedException();
    }
}