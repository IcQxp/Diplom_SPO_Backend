using DiplomBackend.DB;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        //Новый
        //[HttpGet("stream/{id}")]
        //public async Task<IActionResult> StreamDocument(int id)
        //{
        //    var document = await _context.Documents.FindAsync(id);
        //    if (document == null || !System.IO.File.Exists(document.FilePath))
        //        return NotFound("Файл не найден.");

        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(document.FilePath, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;

        //    return File(memory, "application/pdf", Path.GetFileName(document.FilePath));
        //}
        //[HttpGet("stream/{id}")]
        //public async Task<IActionResult> StreamDocument(int id)
        //{
        //    var document = await _context.Documents.FindAsync(id);

        //    if (document == null)
        //    {
        //        Console.WriteLine($"Документ с ID {id} не найден.");
        //        return NotFound("Документ не найден.");
        //    }

        //    Console.WriteLine($"Попытка загрузить файл: {document.FilePath}");

        //    if (!System.IO.File.Exists(document.FilePath))
        //    {
        //        Console.WriteLine($"Файл по указанному пути не существует: {document.FilePath}");
        //        return NotFound("Файл не найден.");
        //    }

        //    try
        //    {
        //        var memory = new MemoryStream();
        //        using (var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
        //        {
        //            await stream.CopyToAsync(memory);
        //        }
        //        memory.Position = 0;

        //        Console.WriteLine($"Файл успешно загружен: {document.FilePath}");

        //        return File(memory, "application/pdf", Path.GetFileName(document.FilePath));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine($"Ошибка при чтении файла: {ex.Message}");
        //        return StatusCode(500, "Ошибка при чтении файла.");
        //    }
        //}
        //[HttpGet("stream/{id}")]
        //public async Task<IActionResult> StreamDocument(int id)
        //{
        //    var document = await _context.Documents.FindAsync(id);
        //    if (document == null)
        //    {
        //        return NotFound("Документ не найден.");
        //    }

        //    if (!System.IO.File.Exists(document.FilePath))
        //    {
        //        return NotFound("Файл не найден.");
        //    }

        //    try
        //    {
        //        // Возвращаем файл как поток с правильными заголовками
        //        var fileStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
        //        return File(fileStream, "application/pdf", enableRangeProcessing: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Ошибка при чтении файла.");
        //    }
        //}
        //[HttpGet("stream/{id}")]
        //public async Task<IActionResult> StreamDocument(int id)
        //{
        //    var document = await _context.Documents.FindAsync(id);
        //    if (document == null)
        //    {
        //        return NotFound(new
        //        {
        //            Message = "Документ не найден в базе данных",
        //            DocumentId = id,
        //            Found = false
        //        });
        //    }

        //    bool fileExists = System.IO.File.Exists(document.FilePath);

        //    // Отладочная информация
        //    var debugInfo = new
        //    {
        //        DocumentId = id,
        //        FilePath = document.FilePath,
        //        FileExists = fileExists,
        //        FileSize = fileExists ? new FileInfo(document.FilePath).Length : 0,
        //        FileLastModified = fileExists ? File.GetLastWriteTime(document.FilePath) : (DateTime?)null,
        //        IsPdf = false,
        //        Message = fileExists ? "Файл найден" : "Файл не существует по указанному пути"
        //    };

        //    if (!fileExists)
        //    {
        //        return NotFound(debugInfo);
        //    }

        //    // Проверяем, является ли файл PDF
        //    try
        //    {
        //        byte[] header = new byte[4];
        //        using (var fs = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
        //        {
        //            await fs.ReadAsync(header, 0, 4);
        //        }

        //        debugInfo["IsPdf"] = header[0] == 0x25 && // %
        //                         header[1] == 0x50 && // P
        //                         header[2] == 0x44 && // D
        //                         header[3] == 0x46;   // F

        //        if (!debugInfo.IsPdf)
        //        {
        //            return BadRequest(new
        //            {
        //                debugInfo,
        //                Message = "Файл не является PDF (не начинается с %PDF)"
        //            });
        //        }

        //        // Если все проверки пройдены, возвращаем файл И отладочную информацию
        //        var fileStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
        //        Response.Headers.Append("X-Debug-Info", JsonSerializer.Serialize(debugInfo));

        //        return File(fileStream, "application/pdf", enableRangeProcessing: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            debugInfo,
        //            Error = ex.Message,
        //            StackTrace = ex.StackTrace
        //        });
        //    }
        //}


        [HttpGet("pdf/{id}")]
        public async Task<IActionResult> GetDocumentPdf(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            if (!System.IO.File.Exists(document.FilePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            var base64String = Convert.ToBase64String(fileBytes);

            return Ok(new
            {
                fileData = base64String,
                mimeType = "application/pdf",
                document = document // если нужно передать метаданные
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents
                    .Include(i => i.Criteria)
                    .Include(i => i.DocumentType)
                    .Include(i => i.Status)
                    .ToListAsync();
                //var documentsDto = documents.Select(doc => new DocumentsListDto
                //{
                //    DocumentId = doc.DocumentId,
                //    FilePath = doc.FilePath,
                //    DownloadDate = doc.DownloadDate,
                //    StudentId = doc.StudentId,
                //    Score = doc.Score,
                //    Criteria = doc.Criteria,
                //    DocumentType = doc.DocumentType,
                //    Employee = doc.Employee,
                //    Status = doc.Status

                //});
                return Ok(documents);
                //return Ok(documentsDto);
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

            string studentIdString = Request.Form["studentId"];
            if (!int.TryParse(studentIdString, out int studentId))
            {
                return BadRequest("Invalid or missing studentId.");
            }

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

        [HttpGet("download2/{id}")]
        public async Task<IActionResult> DownloadDocument2(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                Console.WriteLine($"Document with ID {id} not found.");
                return NotFound("Document not found.");
            }

            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
            {
                Console.WriteLine($"File for document {id} not found or path is invalid.");
                return NotFound("File not found.");
            }

            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var fileBytes = memory.ToArray();
                var fileBase64 = Convert.ToBase64String(fileBytes);

                Console.WriteLine($"Successfully returned PDF for document {id}.");
                return Ok(new
                {
                    fileName = Path.GetFileName(document.FilePath),
                    fileType = "application/pdf",
                    fileData = fileBase64
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while downloading document: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
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
            document.StatusId = 1;
            document.CriteriaId = null;
            document.EmployeeId = null;
            document.DocumentTypeId = null;
            document.Score = 0;
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

        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateDocumentStatus(int id, [FromBody] DocumentStatusUpdateModel model)
        {
            // Проверяем входные данные
            if (model == null)
            {
                return BadRequest("Request body is required.");
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }
            // Status_ID Name
            //1   Новый
            //2   Удален
            //3   Проверен
            //4   Требует изменения
            //5   На проверке

            switch (model.StatusId) {
                case 4:
                case 5:
                    {
                        document.StatusId = model.StatusId;
                        document.EmployeeId = model.EmployeeId;
                        break;
                    }
                 default:
                    {
                        document.StatusId = model.StatusId;
                        document.CriteriaId = model.CriteriaId;
                        document.EmployeeId = model.EmployeeId;
                        document.DocumentTypeId = model.DocumentTypeId;
                        document.Score = model.Score;
                        break;
                    }
            }


            // Обновляем только статусы
            

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Document status updated successfully.",
                    documentId = document.DocumentId,
                    updatedStatus = new
                    {
                        StatusId = document.StatusId,
                        CriteriaId = document.CriteriaId,
                        EmployeeId = document.EmployeeId,
                        DocumentTypeId = document.DocumentTypeId,
                        Score = document.Score
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class DocumentStatusUpdateModel
        {
            public int StatusId { get; set; }
            public int? CriteriaId { get; set; }
            public int? EmployeeId { get; set; }
            public int? DocumentTypeId { get; set; }
            public int? Score { get; set; }
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


        [HttpGet("statuses")]
        public async Task<IActionResult> GetAllStatuses()
        {
            return Ok(await _context.Statuses.ToListAsync());
        }

        [HttpGet("doc/{id}")]
        
        public async Task<IActionResult> GetAllStatuses(int id)
        {
            try
            {
                var doc = await _context.Documents
                    .Include(i=>i.Status)
                    .Include(i => i.Criteria)
                    .Include(i => i.DocumentType)
                    .Include(i => i.Employee)
                    .FirstAsync(elem=>elem.DocumentId==id);
                if (doc==null)
                    return NotFound("Такого документа не существует");

                return Ok(doc);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
    }

}
