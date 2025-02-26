using System;
using System.Collections.Generic;

namespace DiplomBackend.DB;

public partial class DocumentType
{
    public int DocumentTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
