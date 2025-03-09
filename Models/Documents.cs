using DiplomBackend.DB;

namespace DiplomBackend.Models
{
    public class Documents
    {
    }

    public class DocumentsListDto
    {
        /*
         criteria: null
documentId: 2
documentType: null
downloadDate: "2025-02-12T22:07:17.82"
employee: null
filePath: "UploadedFiles\\1\\Rosatomtalents (1).pdf"
score: null
        status: null
        studentId: 1
         */

        public int DocumentId { get; set; }

        public int StudentId { get; set; }

        public string FilePath { get; set; } = null!;

        public DateTime DownloadDate { get; set; }

        public int? Score { get; set; }

        public virtual Criterion? Criteria { get; set; }

        public virtual DocumentType? DocumentType { get; set; }

        public virtual Employee? Employee { get; set; }

        public virtual Status Status { get; set; } = null!;
    }
}
