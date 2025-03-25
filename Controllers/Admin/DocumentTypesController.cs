using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class DocumentTypesController : AdminControllerBase
    {
        public DocumentTypesController(DiplomContext context) : base(context)
        {
        }


        // Добавление дисциплины
        [HttpPost("add-document-type")]
        public async Task<IActionResult> AddDocumentType([FromBody] DocumentType documentType)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(documentType.Name))
                    validationErrors.Add("Name is required.");

                if (string.IsNullOrWhiteSpace(documentType.Description))
                    validationErrors.Add("Description is required.");

                if (await _context.DocumentTypes.AnyAsync(dt => dt.Name == documentType.Name))
                    validationErrors.Add("Name must be unique.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление типа документа в базу данных
                _context.DocumentTypes.Add(documentType);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Document type added successfully.",
                    documentTypeId = documentType.DocumentTypeId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the document type.",
                    error = ex.Message
                });
            }
        }

        // Изменение типа документа
        [HttpPut("update-document-type/{id}")]
        public async Task<IActionResult> UpdateDocumentType(int id, [FromBody] DocumentType updatedDocumentType)
        {
            try
            {
                if (id != updatedDocumentType.DocumentTypeId)
                    return BadRequest("ID mismatch.");

                var existingDocumentType = await _context.DocumentTypes.FindAsync(id);

                if (existingDocumentType == null)
                    return NotFound($"Document type with ID {id} not found.");

                // Обновляем свойства существующего типа документа
                existingDocumentType.Name = updatedDocumentType.Name;
                existingDocumentType.Description = updatedDocumentType.Description;

                _context.DocumentTypes.Update(existingDocumentType);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Document type updated successfully.",
                    documentType = existingDocumentType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the document type.",
                    error = ex.Message
                });
            }
        }

        // Удаление типа документа
        [HttpDelete("delete-document-type/{id}")]
        public async Task<IActionResult> DeleteDocumentType(int id)
        {
            try
            {
                var documentType = await _context.DocumentTypes.FindAsync(id);

                if (documentType == null)
                    return NotFound($"Document type with ID {id} not found.");

                // Проверка на наличие связанных документов
                if (documentType.Documents.Any())
                {
                    return BadRequest("Cannot delete a document type that has associated documents.");
                }

                _context.DocumentTypes.Remove(documentType);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the document type.",
                    error = ex.Message
                });
            }
        }

        // Получение типа документа по ID
        [HttpGet("get-document-type/{id}")]
        public async Task<IActionResult> GetDocumentTypeById(int id)
        {
            var documentType = await _context.DocumentTypes
                .Include(dt => dt.Documents)
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == id);

            if (documentType == null)
                return NotFound($"Document type with ID {id} not found.");

            return Ok(documentType);
        }

        // Получение всех типов документов
        [HttpGet("get-all-document-types")]
        public async Task<IActionResult> GetAllDocumentTypes()
        {
            var documentTypes = await _context.DocumentTypes
                .Include(dt => dt.Documents)
                .ToListAsync();

            return Ok(documentTypes);
        }
    }
}
