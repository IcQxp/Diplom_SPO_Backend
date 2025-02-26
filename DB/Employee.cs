using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string GenderCode { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telephone { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Role Role { get; set; } = null!;
}
