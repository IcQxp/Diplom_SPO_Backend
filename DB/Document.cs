using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class Document
{
    public int DocumentId { get; set; }

    public int StudentId { get; set; }

    public int StatusId { get; set; }

    public int? EmployeeId { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime DownloadDate { get; set; }

    public int? DocumentTypeId { get; set; }

    public int? CriteriaId { get; set; }

    public int? Score { get; set; }

    public virtual Criterion? Criteria { get; set; }

    public virtual DocumentType? DocumentType { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Status Status { get; set; } = null!;
}
