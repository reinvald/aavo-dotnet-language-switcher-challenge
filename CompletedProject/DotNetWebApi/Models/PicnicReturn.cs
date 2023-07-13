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