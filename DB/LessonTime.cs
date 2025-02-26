using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class LessonTime
{
    public int LessonTimeId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
