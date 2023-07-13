# Database Create API

Inserting records into the database is simpler than doing updates. Lets create a new Teddy Bear.

To do this, we will do it in a new controller: `TeddyBearController.cs`

1. Create a new controller (in the Controllers folder) named `TeddyBearController.cs`
2. Add this code to the file to create the shell of the controller:

    ```C#
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
    }
    ```

3. Now add the following to the newly created TeddyBearController:

    ```C#
    [HttpPost("TeddyBears")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TeddyBear>> CreateTeddyBear(TeddyBear teddyBear)
    {
        _context.TeddyBears.Add(teddyBear);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateTeddyBear), new { id = teddyBear.Id }, teddyBear);
    }
    ```

Note that we simply accept a `TeddyBear` object, Add it to the `TeddyBears` DB Context and save.

> ---
> **Note**: The POST does not require that the new TeddyBear's ID be set; it will get set in
> the database automatically as part of the insert.  It will be returned to the caller in the 
> response headers.  If you need programmatic access to that newly assigned ID, it is available 
> in the `teddyBear.Id` member property _after_ the awaited call to `SaveChangesAsync`, i.e.,
> after the program makes a round-trip to the database.
>
> ---

The curious part here is the `CreatedAtAction` function. This is a simple way to return a `201` status code back to the consumer, and populate the location header. Here we reference the `GetTeddyBear` function, and specify that the Id parameter of that function should be the Id of the newly created Teddy Bear. .NET will do the rest for us.  This includes adding a URL to 
the header of the response that includes `api/TeddyBears/{theIdOfTheNewBear}`. This allows the
client to immediately know how to query for this new teddy bear (assuming we had implemented
the expected GET endpoint on this controller).

For more information on `CreatedAtAction` and similar available functions, read Microsoft's [ControllerBase Class Methods](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-7.0#methods)

## A More Involved Insert Operation - Creating a New Picnic

Looking back at the data model for this system, a Picnic references the PicnicLocation table
(in a one-to-many relationship), and it references the TeddyBears table in a many-to-many
relationship.

However, inserting a new Picnic only requires that you set the Picnic's location and that
you provide a list of TeddyBears that will attend the Picnic.  This is complicated by the
fact that we probably want to describe the location by name (rather than by ID) and we
certainly want to describe the Teddy Bears by name and not ID.

To manage this, we need to create another DTO to pass the Location information by name, and
the list of Teddy Bears by name along with the Picnic information.  Again, we'll put this 
DTO into the Models folder (file: PicnicCreate.cs):

```C#
namespace DotNetWebApi.Models;

public class PicnicCreate 
{
    public string PicnicName { get; set; } = null!;
    public string LocationName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public bool HasMusic { get; set; }
    public bool HasFood { get; set; }
    public virtual List<string> TeddyBears { get; set; } = new List<string>();
}
```
With that in place, we can update the controller. Add the following code to the 
`PicnicController`:

```C#
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
```
This code is more involved.  It fishes out the PicnicLocation by location name and a list of
TeddyBears by teddy bear name.  Then it creates a new Picnic using all this information. The
TeddyBears can't be added _en masse_, so they are added one by one.  Finally, the new picnic 
entry is added to the database with a call to `SaveChangesAsync`.  That's the third round 
trip to the database done in this method (behind the location and teddy bear lookups).

In order to return the newly created picnic to the caller, we need to overcome the same 
circular reference issue we faced when we were querying picnics at the start of this exercise.
The solution is the same: use the `PicnicReturn` DTO class we created at that time.

It's the `Picnics` property of the  `Picnic.Location` entry that is the source of the circular 
reference issues; we want it to be explicitly clear that list, so we do that in the second to
last line of code.  The `?.` operator is called a 
[null conditional operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators).
That line of code says _"Look at the location property.  If its Picnics property is not null,
call `Clear()` on it, if it is null, don't go any further executing this line of code"_

The call to `CreatedAtAction` returns the newly constructed `PicnicReturn` object, not the 
object in the database, but it's a pretty good facsimile.

## Next Steps

[External Service](/docs/5-External-Service.md)