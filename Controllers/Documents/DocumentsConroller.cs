using DiplomBackend.DB;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomBackend.Controllers.Documents
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly DiplomContext _context;
        private readonly string _storagePath;

        public DocumentsController(DiplomContext context)
        {
            _context = context;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents.ToListAsync();
                var documentsDto = documents.Select(doc => new DocumentsListDto
                {
                    DocumentId = doc.DocumentId,
                    FilePath = doc.FilePath,
                    DownloadDate = doc.DownloadDate,
                    StudentId = doc.StudentId,
                    Score = doc.Score,
                    Criteria = doc.Criteria,
                    DocumentType = doc.DocumentType,
                    Employee = doc.Employee,
                    Status = doc.Status

                });
                return Ok(documentsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAllUserDocuments(int id)
        {
            try
            {
                var statuses = await _context.Statuses.ToListAsync();
                var docsTypes = await _context.DocumentTypes.ToListAsync();
                var criteries = await _context.Criteria.ToListAsync();
                var employees = await _context.Employees.ToListAsync();

                var documents = await _context.Documents
                    .Where(doc => doc.StudentId == id)
                    .Where(doc => doc.StatusId != 2) // Status DELETED
                    .ToListAsync();

                var updatedDocuments = documents
            .GroupJoin(
                statuses,
                doc => doc.StatusId,
                status => status.StatusId,
                (doc, statusGroup) => new { doc, statusGroup })
            .SelectMany(
                x => x.statusGroup.DefaultIfEmpty(), // Левое соединение
                (x, status) =>
                {
                    x.doc.Status = status; // Обновляем свойство Status
                    return x.doc;
                })
            .GroupJoin(
                docsTypes,
                doc => doc.DocumentTypeId,
                docType => docType.DocumentTypeId,
                (doc, docTypeGroup) => new { doc, docTypeGroup })
            .SelectMany(
                x => x.docTypeGroup.DefaultIfEmpty(), // Левое соединение
                (x, docType) =>
                {
                    x.doc.DocumentType = docType; // Обновляем свойство DocumentType
                    return x.doc;
                })
            .GroupJoin(
                criteries,
                doc => doc.CriteriaId,
                criteria => criteria.CriteriaId,
                (doc, criteriaGroup) => new { doc, criteriaGroup })
            .SelectMany(
                x => x.criteriaGroup.DefaultIfEmpty(), // Левое соединение
                (x, criteria) =>
                {
                    x.doc.Criteria = criteria; // Обновляем свойство Criteria
                    return x.doc;
                })
            .GroupJoin(
                employees,
                doc => doc.CriteriaId,
                emp => emp.EmployeeId,
                (doc, criteriaGroup) => new { doc, criteriaGroup })
            .SelectMany(
                x => x.criteriaGroup.DefaultIfEmpty(), // Левое соединение
                (x, emp) =>
                {
                    x.doc.Employee = emp; // Обновляем свойство Criteria
                    return x.doc;
                })
            .ToList();

                return Ok(updatedDocuments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            int studentId = 1;
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                return BadRequest("Only PDF files are allowed.");
            }

            // Определяем путь для сохранения файла
            var studentFolder = Path.Combine("UploadedFiles", studentId.ToString());
            Directory.CreateDirectory(studentFolder); // Создаем папку, если она не существует

            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string baseFilePath = Path.Combine(studentFolder, fileName + extension);
            string filePath = baseFilePath;

            // Проверяем, существует ли файл, и изменяем имя, если необходимо
            int count = 1;
            while (System.IO.File.Exists(filePath))
            {
                filePath = Path.Combine(studentFolder, $"{fileName} {count}{extension}");
                count++;
            }

            // Сохраняем файл на диск
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                StatusId = 1,
                StudentId = studentId,
                FilePath = filePath,
                DownloadDate = DateTime.UtcNow,
                // Заполните остальные поля по необходимости
            };

            try
            {
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return Ok(new { document.DocumentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound("Document not found.");

            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
                return NotFound("File not found.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/pdf", Path.GetFileName(document.FilePath));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDocument(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                return BadRequest("Only PDF files are allowed.");
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            // Удаляем старый файл
            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            // Сохраняем новый файл
            var studentFolder = Path.Combine("UploadedFiles", document.StudentId.ToString());
            Directory.CreateDirectory(studentFolder);

            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string baseFilePath = Path.Combine(studentFolder, fileName + extension);
            string filePath = baseFilePath;

            int count = 1;
            while (System.IO.File.Exists(filePath))
            {
                filePath = Path.Combine(studentFolder, $"{fileName} ({count}){extension}");
                count++;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Обновляем путь к файлу в базе данных
            document.FilePath = filePath;
            document.DownloadDate = DateTime.UtcNow;
            // Обновляем другие поля по необходимости

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { document.FilePath, FileName = Path.GetFileName(filePath) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            // Устанавливаем статус на 2 (предполагается, что это статус "Удалён" или "Архивирован")
            document.StatusId = 2;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Document marked as deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

}
