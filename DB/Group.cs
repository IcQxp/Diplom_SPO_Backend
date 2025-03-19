using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiplomBackend.DB;

public partial class Group
{
    public int GroupId { get; set; }

    public string GroupNumber { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    
    [JsonIgnore]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
