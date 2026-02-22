using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Workspace
{
    public int Id { get; set; }

    public int Typeid { get; set; }

    public string Number { get; set; } = null!;

    public decimal Priceperhour { get; set; }

    public bool Isactive { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Workspacetype Type { get; set; } = null!;
}
