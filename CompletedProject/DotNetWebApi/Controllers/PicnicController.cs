using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetWebApi.Models;

namespace DotNetWebApi.Controllers;

[Route("api/")]
[ApiController]
public class PicnicController : ControllerBase
{
    private readonly TeddyBearsContext _context;

    public PicnicController(TeddyBearsContext context)
    {
        _context = context;
    }

    [HttpGet("Picnics")]
    public async Task<ActionResult<IEnumerable<PicnicReturn>>> GetPicnics()
    {
        var picnics = from picnic in _context.Picnics
                      select new PicnicReturn()
                      {
                          Id = picnic.Id,
                          PicnicName = picnic.PicnicName,
                          Location = picnic.Location,
                          StartTime = picnic.StartTime,
                          HasMusic = picnic.HasMusic == true,
                          HasFood = picnic.HasFood == true,
                          TeddyBears = picnic.TeddyBears.Select(tb => tb.Name)
                      };
        return await picnics.ToListAsync();
    }

    [HttpGet("Picnics/{name}")]
    public async Task<ActionResult<PicnicReturn>> GetPicnicByName(string name)
    {
        var picnics = from picnic in _context.Picnics
                      where picnic.PicnicName == name
                      select new PicnicReturn()
                      {
                          Id = picnic.Id,
                          PicnicName = picnic.PicnicName,
                          Location = picnic.Location,
                          StartTime = picnic.StartTime,
                          HasMusic = picnic.HasMusic == true,
                          HasFood = picnic.HasFood == true,
                          TeddyBears = picnic.TeddyBears.Select(tb => tb.Name)
                      };
        var picnicFound = await picnics.FirstOrDefaultAsync();
        if (picnicFound == null)
        {
            return NotFound();
        }
        //otherwise
        return picnicFound;
    }

    [HttpGet("Picnics/Locations/{locationName}")]
    public async Task<ActionResult<IEnumerable<PicnicReturn>>> GetPicnicsByLocation(string locationName)
    {
        var picnics = _context.Picnics
                                .Where(p => p.Location != null && p.Location.LocationName == locationName)
                                .Select(p => new PicnicReturn
                                {
                                    Id = p.Id,
                                    PicnicName = p.PicnicName,
                                    Location = p.Location,
                                    StartTime = p.StartTime,
                                    HasMusic = p.HasMusic == true,
                                    HasFood = p.HasFood == true,
                                    TeddyBears = p.TeddyBears.Select(tb => tb.Name)
                                });
        var picnicsFound = await picnics.ToListAsync();
        if (!picnicsFound.Any())
        {
            return NotFound();
        }
        //otherwise
        return picnicsFound;
    }

    [HttpPost("Picnics")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PicnicCreate>> CreatePicnic(PicnicCreate picnic)
    {
        var location = await _context.PicnicLocations.Where(p => p.LocationName == picnic.LocationName).FirstOrDefaultAsync();
        var teddyBears = await _context.TeddyBears.Where(t => picnic.TeddyBears.Contains(t.Name)).ToListAsync();
        var newPicnic = new Picnic
        {
            PicnicName = picnic.PicnicName,
            Location = location,
            StartTime = picnic.StartTime,
            HasFood = picnic.HasFood,
            HasMusic = picnic.HasMusic,
        };
        // add in the teddy bears
        foreach (var bear in teddyBears){
            newPicnic.TeddyBears.Add(bear);
        }
        _context.Picnics.Add(newPicnic);
        await _context.SaveChangesAsync();

        //We can't just return the new picnic, we'll get circular JSON issues, so...
        var picnicToReturn = new PicnicReturn
        {
            Id = newPicnic.Id,
            PicnicName = newPicnic.PicnicName,
            Location = newPicnic.Location,
            StartTime = newPicnic.StartTime,
            HasFood = newPicnic.HasFood ?? false,
            HasMusic = newPicnic.HasMusic ?? false,
            TeddyBears = newPicnic.TeddyBears.Select(t => t.Name),
        };
        picnicToReturn.Location?.Picnics?.Clear();     //more cleanup for circular JSON issues

        return CreatedAtAction(nameof(CreatePicnic), new { id = newPicnic.Id }, picnicToReturn);
    }
}