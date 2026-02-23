using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Workspace
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public string Number { get; set; } = null!;

    public decimal PricePerHour { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual WorkspaceType Type { get; set; } = null!;
}
