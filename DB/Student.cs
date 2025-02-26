using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Student
{
    public int StudentId { get; set; }

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string GenderCode { get; set; } = null!;

    public int GroupId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Group Group { get; set; } = null!;
}
