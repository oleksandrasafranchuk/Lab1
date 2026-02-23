using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int WorkspaceId { get; set; }

    public int StatusId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string? UserComment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual BookingStatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Workspace Workspace { get; set; } = null!;
}
