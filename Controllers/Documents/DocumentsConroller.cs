using DiplomBackend.DB;
using DiplomBackend.Models;
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
            // Поиск документа в базе данных
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            // Проверка наличия пути к файлу
            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
            {
                return NotFound("File not found.");
            }

            // Чтение файла из файловой системы
            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0; // Установка позиции в начало потока

            // Возвращение файла с указанием MIME-типа и имени файла
            return File(memory, "application/pdf", Path.GetFileName(document.FilePath));
        }
    }

}
