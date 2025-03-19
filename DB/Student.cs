using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiplomBackend.DB;

public partial class Student
{
    public int StudentId { get; set; }

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string GenderCode { get; set; } = null!;
    
    [JsonIgnore]
    public int GroupId { get; set; }

    public string Login { get; set; } = null!;
    
    [JsonIgnore]
    public string Password { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    [JsonIgnore]
    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Group Group { get; set; } = null!;
}
