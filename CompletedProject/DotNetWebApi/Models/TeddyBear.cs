using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApi.Models;

/// <summary>
/// Teddy Bears are soft and cuddly.  Each has a name and a unique personality
/// </summary>
[Table("TeddyBear")]
[Index("Name", Name = "IX_TeddyBearName", IsUnique = true)]
public partial class TeddyBear
{
    /// <summary>
    /// The Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The Teddy Bear&apos;s name.  Each is unique
    /// </summary>
    [StringLength(50)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// All teddy bears have a primary color.  The color is a string (but should be picked from a list)
    /// </summary>
    [StringLength(20)]
    public string PrimaryColor { get; set; } = null!;

    /// <summary>
    /// Teddy Bears may have a secondary color.  The color is a string (but should be picked from a list)
    /// </summary>
    [StringLength(20)]
    public string? AccentColor { get; set; }

    /// <summary>
    /// Is the Teddy Bear dressed (true or false)
    /// </summary>
    [Required]
    public bool? IsDressed { get; set; }

    /// <summary>
    /// Who is the teddy bear&apos;s owner
    /// </summary>
    [StringLength(50)]
    public string OwnerName { get; set; } = null!;

    /// <summary>
    /// Teddy bears may have a defining characteristic - fluffy, polite, whatever
    /// </summary>
    [StringLength(50)]
    public string? Characteristic { get; set; }

    [ForeignKey("TeddyBearId")]
    [InverseProperty("TeddyBears")]
    public virtual ICollection<Picnic> Picnics { get; } = new List<Picnic>();
}
