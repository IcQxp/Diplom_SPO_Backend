using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Gender
{
    public string GenderCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
