using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Group
{
    public int GroupId { get; set; }

    public string GroupNumber { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
