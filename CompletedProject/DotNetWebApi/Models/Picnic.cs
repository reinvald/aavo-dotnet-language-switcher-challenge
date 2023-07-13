using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApi.Models;

/// <summary>
/// Teddy bears have picnics.  Picnics have locations and participants (teddy bears)
/// </summary>
[Table("Picnic")]
[Index("PicnicName", Name = "IX_PicnicName", IsUnique = true)]
public partial class Picnic
{
    /// <summary>
    /// The primary key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The name of the picnic.  All picnics have unique names
    /// </summary>
    [StringLength(50)]
    public string PicnicName { get; set; } = null!;

    /// <summary>
    /// A reference to where the picnic will be held (a foreign key into the PicnicLocation table)
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Picnics have a start time.  They always end at 6:00pm, when the mommies and daddies take them home to bed because they are tired little teddy bears
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Will the picnic have music (default = true)
    /// </summary>
    [Required]
    public bool? HasMusic { get; set; }

    /// <summary>
    /// Will the picnic have food (default = true)
    /// </summary>
    [Required]
    public bool? HasFood { get; set; }

    [ForeignKey("LocationId")]
    [InverseProperty("Picnics")]
    public virtual PicnicLocation? Location { get; set; }

    [ForeignKey("PicnicId")]
    [InverseProperty("Picnics")]
    public virtual ICollection<TeddyBear> TeddyBears { get; } = new List<TeddyBear>();
}
