using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetWebApi.Models;

namespace DotNetWebApi.Controllers;

[Route("api/")]
[ApiController]
public class LocationController : ControllerBase
{
    private readonly TeddyBearsContext _context;

    public LocationController(TeddyBearsContext context)
    {
        _context = context;
    }

    [HttpPut("Locations/{Id}")]
    public async Task<ActionResult<PicnicLocation>> UpdatePicnicLocation(int Id, PicnicLocation location)
    {
        if (Id != location.Id)
        {
            return BadRequest("URL Id must match the object Id");
        }

        var locationToUpdate = await _context.PicnicLocations.FindAsync(Id);
        if (locationToUpdate == null)
        {
            return NotFound();
        }

        locationToUpdate.LocationName = location.LocationName;
        locationToUpdate.Capacity = location.Capacity;
        locationToUpdate.Municipality = location.Municipality;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("Locations2/{Id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PicnicLocation>> UpdatePicnicLocation2(int Id, PicnicLocation location)
    {
        if (Id != location.Id)
        {
            return BadRequest("URL Id must match the object Id");
        }

        var newLocationValue = new PicnicLocation
        {
            Id = Id,
            LocationName = location.LocationName,
            Capacity = location.Capacity,
            Municipality = location.Municipality
        };

        _context.PicnicLocations.Update(newLocationValue);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // This method is not in the instructions, but is useful
    [HttpGet("Locations")]
    public async Task<ActionResult<List<PicnicLocation>>> GetLocations()
    {
        return await _context.PicnicLocations.ToListAsync();
    }
}

