using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int DisciplineId { get; set; }

    public int GroupId { get; set; }

    public int LessonTimeId { get; set; }

    public DateOnly LessonDate { get; set; }

    public int EmployeeId { get; set; }

    public virtual Discipline Discipline { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Group Group { get; set; } = null!;

    public virtual LessonTime LessonTime { get; set; } = null!;
}
