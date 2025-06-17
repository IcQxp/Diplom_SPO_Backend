using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiplomBackend.DB;

public partial class LessonTime
{
    public int LessonTimeId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [JsonIgnore]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
