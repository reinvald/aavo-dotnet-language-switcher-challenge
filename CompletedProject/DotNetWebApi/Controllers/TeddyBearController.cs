using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetWebApi.Models;

namespace DotNetWebApi.Controllers;

[Route("api/")]
[ApiController]
public class TeddyBearController : ControllerBase
{
    private readonly TeddyBearsContext _context;

    public TeddyBearController(TeddyBearsContext context)
    {
        _context = context;
    }

    [HttpPost("TeddyBears")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TeddyBear>> CreateTeddyBear(TeddyBear teddyBear)
    {
        _context.TeddyBears.Add(teddyBear);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateTeddyBear), new { id = teddyBear.Id }, teddyBear);
    }
}