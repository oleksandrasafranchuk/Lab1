using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab1_Project.Models;

public partial class Workspace
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    [Required]
    public string Number { get; set; } = null!;

    [Range(0.01, double.MaxValue, ErrorMessage = "Ціна повинна бути більшою за 0")]
    public decimal PricePerHour { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual WorkspaceType Type { get; set; } = null!;
}
