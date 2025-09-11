using System;
using System.Collections.Generic;

namespace Infrastructure.Data;

public partial class Sport
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string Group { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int HasOutrights { get; set; }

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
