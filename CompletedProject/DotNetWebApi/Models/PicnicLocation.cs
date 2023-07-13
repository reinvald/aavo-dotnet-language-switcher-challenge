using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApi.Models;

/// <summary>
/// The location of one or more picnics.  Every picnic must have a location
/// </summary>
[Table("PicnicLocation")]
[Index("LocationName", Name = "IX_PicnicLocationName", IsUnique = true)]
public partial class PicnicLocation
{
    /// <summary>
    /// The Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The name of the location (must be unique)
    /// </summary>
    [StringLength(50)]
    public string LocationName { get; set; } = null!;

    /// <summary>
    /// How many teddy bears can be accommodated at this location
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// In what village, town or city is this location
    /// </summary>
    [StringLength(50)]
    public string Municipality { get; set; } = null!;

    [InverseProperty("Location")]
    public virtual ICollection<Picnic> Picnics { get; } = new List<Picnic>();
}
