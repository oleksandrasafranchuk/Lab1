using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class BookingHistory
{
    public long Id { get; set; }

    public int BookingId { get; set; }

    public int StatusToId { get; set; }

    public int? ChangedByUserId { get; set; }

    public string? ChangeReason { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User? ChangedByUser { get; set; }

    public virtual BookingStatus StatusTo { get; set; } = null!;
}
