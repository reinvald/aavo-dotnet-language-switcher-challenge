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