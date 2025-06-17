using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiplomBackend.DB;

public partial class Discipline
{
    public int DisciplineId { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
