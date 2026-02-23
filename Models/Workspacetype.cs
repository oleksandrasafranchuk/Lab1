using System;
using System.Collections.Generic;

namespace Lab1_Project.Models;

public partial class WorkspaceType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
