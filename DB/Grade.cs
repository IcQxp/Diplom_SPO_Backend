using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Grade
{
    public int GradeId { get; set; }

    public int StudentId { get; set; }

    public int LessonId { get; set; }

    public int Value { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
