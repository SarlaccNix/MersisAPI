using MapsAPI.Characters;

namespace MapsAPI.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;

[ApiController]
[Route("[controller]")]

public class CharactersController
{
    [HttpPost("Characters")]
    public IEnumerable<Character> newCharacter()
    {
        yield break;
    }
    [HttpGet("Characters")]
    public IEnumerable<Character> getCharacters()
    {
        yield break;
    }
    [HttpPut("Characters")]
    public IEnumerable<Character> updateCharacter()
    {
        yield break;
    }
    [HttpPost("SearchCharacters")]
    public IEnumerable<Character> searchCharacter()
    {
        yield break;
    }
}