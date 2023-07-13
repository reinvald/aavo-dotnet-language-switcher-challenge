## Exploring the Generated Code

Let's look around the code we just generated and ran:

1. Open `PicnicController.cs` in VS Code
2. In the `GetPicnics` method, find this line (near the bottom):
   ```C#
   var picnics = await _context.Picnics.ToListAsync();
   ```
3. Right-click on `Picnics` (in the fragment ` _context.Picnics.ToListAsync()`) and choose:
   `Go to implementations`
4. This will bring you to the `TeddyBearsContext.cs` file, at this line:
   ```C#
   public virtual DbSet<Picnic> Picnics { get; set; }
   ```

Those three `DbSet<T>` properties on the database context each represent the programmatic equivalent of their 
corresponding database tables: Picnic, PicnicLocation and TeddyBear.

Going back to the code in `PicnicController.cs`, the call to `await _context.Picnics.ToListAsync()` accesses
the `Picnics` property, and tells the system to create a database call to start to fetch the Picnic data
asynchronously.  When the data is accessed and returned (as a `List<Picnic>`), it is assigned to the `picnics`
variable and returned.  The .NET controller framework (largely resident in `ActionResult`) converts the
collection of picnic instances into JSON and returns that JSON to the caller with a status code of 200.

## Returning More Picnic Information

As mentioned earlier, it's possible to `Include` more Picnic information, but since the relationships to
TeddyBears and to PicnicLocations are bi-directional, this can result in cycles from a simple lookup.

Instead, let's create a separate class to use to return picnic information to the user.  In more complex system
these are often known as _Data Transfer Objects_ (or _DTOs_) and are managed separately from _Models_.  To
keep it simple, we'll just use the existing Models folder:

1. In VS Code, right-click the `Models` folder and choose `New File`
2. Name the file `PicnicReturn.cs`
3. Add the following code to the file:

```C#
using DotNetWebApi.Models;

public class PicnicReturn 
{
    /// <summary>
    /// The primary key in the database for this picnic
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The name of the picnic.  All picnics have unique names
    /// </summary>
    public string PicnicName { get; init; } = null!;

    /// <summary>
    /// A reference to where the picnic will be held (a foreign key into the PicnicLocation table)
    /// </summary>
    public PicnicLocation? Location { get; init; } = null;

    /// <summary>
    /// Picnics have a start time.  They always end at 6:00pm, 
    /// when the mommies and daddies take them home to bed because they are tired little teddy bears
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Will the picnic have music (default = true)
    /// </summary>
    public bool HasMusic { get; init; } = true;

    /// <summary>
    /// Will the picnic have food (default = true)
    /// </summary>
    public bool HasFood { get; init; } = true;

    /// <summary>
    /// A collection of Teddy Bear names attending the picnic
    /// </summary>
    public IEnumerable<string> TeddyBears { get; init; } = null!;
}
```

### A Few Things to Note in that Code

This code is substantially similar to the `Picnic` class, but with several differences.

* The normal `{ get; set; }` [_auto-property_](./Notes/3-Properties.md#auto-properties) 
  syntax has been replaced with `{ get; init; }`.  This tells the
  compiler that the property is _settable_, but only at object initialization time.  The compiler 
  [will flag any attempts to set these properties after initialization](./Notes/3-Properties.md#get-set-and-init).
* Most of the reference type instances are set to be 
  [non-nullable](./Notes/0-TypeSystem.md#nullable-reference-types) (they lack a `?` in the 
  type name) (the strings and the collection of teddy bear names).  They are all initialized 
  to `null!`.  That (the `null!`) tells the compiler: "Yes, I know that this property is 
  non-nullable, but believe me, it will 
  be non-null by the time anyone gets around to using it"
* The Location property is set to be nullable (the `?` in "`PicnicLocation? Location`").  You
  can create a picnic with no location.  It's initialized to `null`
* There's a `PicnicLocation`-typed property.  Its `Picnics` collection property will not be traversed, 
  so it will always be empty (preventing cycles in the fetch)
* The `TeddyBears` collection here is just a collection of strings representing the Teddy Bear names, not
  entire TeddyBear objects.

### Updating the Controller Code to use PicnicReturn

In `PicnicController.cs` change the definition of `GetPicnics` to the following:

```C#
[HttpGet("Picnics")]
public async Task<ActionResult<IEnumerable<PicnicReturn>>> GetPicnics()
{
    var picnics = from picnic in _context.Picnics
                  select new PicnicReturn () {
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
```

Press `F5` and give the new code a try.  You'll find a more complete definition of a picnic in the output,
each picnic looking something like this:

```JSON
{
    "id": 1,
    "picnicName": "Picnic At Oakwood",
    "location": {
      "id": 1,
      "locationName": "Big Wood",
      "capacity": 25,
      "municipality": "Oakwood",
      "picnics": []
    },
    "startTime": "2023-03-04T14:00:00",
    "hasMusic": true,
    "hasFood": true,
    "teddyBears": [
      "Teddy",
      "Suzie",
      "Nounours",
      "Winnie the Pooh"
    ]
}
 ```

This code constructs a new instance of a collection `PicnicReturn` objects using C# 
[_LINQ - Language INtegrated Query_](./Notes/2-AboutLinq.md) technology. In particular, this 
code uses the _Query Comprehension Syntax_ (or just the _Query Syntax_).  It has a strong 
resemblance to SQL script syntax (but with some major differences).  It's important 
to realize that this is not like a "query string" interface; the item variable (`picnic` here)
is strongly typed as a `Picnic` instance.  Notice that when you type "`picnic.`" in the
`select` part of the query, you get autocompletion behavior.  LINQ query item variables (like
`picnic`) are conceptually similar to the item variables in a C# `foreach` statement.

The `TeddyBears` property is set using the other LINQ syntax, the _Fluent_ syntax.  Here, the
`TeddyBears` property is set by calling `.Select()` on the full `picnic.TeddyBears` property, passing
in a _Lambda_ expression that selects only the name.  The result is a collection of TeddyBear names.

All this code creates an 
[_Expression Tree_](./Notes/2-AboutLinq.md#expression-trees-and-iqueryable) that can be 
translated into a SQL query when `ToListAsync` is called.  If you look at the 
`Debug Console` output in VS Code, you can see the SQL that it generates:

  ```SQL
SELECT 
    [p].[Id], 
    [p].[PicnicName], 
    [p0].[Id], 
    [p0].[Capacity], 
    [p0].[LocationName], 
    [p0].[Municipality], 
    [p].[StartTime], 
    [p].[HasMusic], 
    [p].[HasFood], 
    [t0].[Name], 
    [t0].[PicnicId], 
    [t0].[TeddyBearId], 
    [t0].[Id]
FROM [Picnic] AS [p]
LEFT JOIN [PicnicLocation] AS [p0] 
    ON [p].[LocationId] = [p0].[Id]
LEFT JOIN (
    SELECT 
        [t].[Name], 
        [p1].[PicnicId], 
        [p1].[TeddyBearId], 
        [t].[Id]
    FROM [PicnicParticipants] AS [p1]
    INNER JOIN [TeddyBear] AS [t] 
        ON [p1].[TeddyBearId] = [t].[Id]
) AS [t0] 
    ON [p].[Id] = [t0].[PicnicId]
ORDER BY 
    [p].[Id], 
    [p0].[Id], 
    [t0].[PicnicId], 
    [t0].[TeddyBearId]
  ```

## Getting a Picnic by Name

In order to pick out a single picnic, using the picnic's name, we use code that looks very similar.
Add the following code as a new method in `PicnicController.cs`:

  ```C#
[HttpGet("Picnics/{name}")]
public async Task<ActionResult<PicnicReturn>> GetPicnicByName(string name)
{
    var picnics = from picnic in _context.Picnics
                  where picnic.PicnicName == name
                  select new PicnicReturn () {
                        Id = picnic.Id,
                        PicnicName = picnic.PicnicName,
                        Location = picnic.Location,
                        StartTime = picnic.StartTime,
                        HasMusic = picnic.HasMusic == true,
                        HasFood = picnic.HasFood == true,
                        TeddyBears = picnic.TeddyBears.Select(tb => tb.Name)
                  };
    var picnicFound = await picnics.FirstOrDefaultAsync();
    if (picnicFound == null){
        return NotFound();
    }
    //otherwise
    return picnicFound;
}
```

Here there are only a few changes from the previous code:

* The picnic to return is determined through a fragment of the URL
* The function returns a single `PicnicReturn`, not a collection of them
* There is a `where` clause, used to pick out the one picnic instance we are interested in (remember
  there's a unique index on `PicnicName`) in the database, so this method will return either one picnic
  or no picnics.  There are a couple of things worth noting:
  * The `where` clause is before the `select` clause (the opposite of the SQL 
    language).  This is because the `where` will filter the number of possible results, so that the 
    `select` will not have to do as much work.  This ordering (first `where` then `select`) is 
    required when using the query comprehension syntax.
  * The `where` clause uses standard C# syntax (`where picnic.PicnicName == name`) to describe
    the predicate condition (that the picnic name is equal to the method parameter `name`)
* Rather than converting the results to a `List<Picnic>`, this code uses `.FirstOrDefaultAsync` to get
  either the first result (remember, there can be no more than one), or, if there is no result, the
  default result (`null`)
* If there are no results, the code returns a 404 (Not Found), otherwise, it returns a single Picnic
  instance as JSON.

### Try it Out

Fire up the debugger from VS Code and pick the `api/Picnics/{name}` endpoint.  Find a valid picnic name (you 
can get that from running the `api/Picnics` enpoint, or just use `"Picnic At Oakwood"`).  You should see 
a single picnic being returned.

If you use a bad picnic name (say `"bad picnic name"`), you will get a 404 returned

If you look at the SQL in the `Debug Console` output, the only difference from the previous SQL code 
is a parameterized `WHERE` clause:

```SQL
WHERE [p].[PicnicName] = @__name_0
```

## Continuing with the LINQ Fluent Syntax

The next step adds a new controller endpoint, this type implemented using the LINQ _Fluent_ syntax.  This
endpoint will return all picnics that occur at a single location:

```C#
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
    if (!picnicsFound.Any()){
        return NotFound();
    }
    //otherwise
    return picnicsFound;
}
```

Here, instead of using a syntax that looks like SQL script, we use functions that take expressions
in the form of _Lambda Expressions_.  Instead of having a general _item variable_ like `picnic` in 
`from picnic in _context.Picnics` and using it throughout the LINQ statement, each function has a 
lambda, and the variable associated with that lambda is used only in that function.

Consider `.Where(p => p.Location != null && p.Location.LocationName == locationName)`.  The lambda passed to 
`Where` describes a predicate filter.  It will return only those picnics that match the predicate rule. Also 
note that the predicate takes care of the case where the location is `null`.  It does that using standard
C# syntax.  Be aware, however, that not all C# expressions can be _translated_ to SQL script by the 
Entity Framework LINQ provider.

The `.Select` call is a projection.  It takes all of the input picnics and uses each of those to construct new
objects as described in the lambda passed.  It will return a collection containing the same number of
`PicnicReturn` objects as there were `Picnic` objects in the in original collection.

## _Exercises_

1. _Write data transfer object classes for TeddyBears and for PicnicLocations.  The 
   TeddyBearReturn class should include a list of Picnics that the TeddyBear is linked to, 
   while the PicnicLocationReturn class should also include a similar list of picnics._

2. _Add endpoints to the controller to allow obtaining TeddyBear and PicnicLocation information_

## Next Steps

[Database Update API](/docs/3-Database-Update.md)
