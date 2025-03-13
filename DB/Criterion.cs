using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiplomBackend.DB;

public partial class Criterion
{
    public int CriteriaId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int MaxScore { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
