using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class CriteriaController : AdminControllerBase
    {
        public CriteriaController(DiplomContext context) : base(context)
        {
        }

        // Добавление критерия
        [HttpPost("add-criterion")]
        public async Task<IActionResult> AddCriterion([FromBody] Criterion criterion)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(criterion.Name))
                    validationErrors.Add("Name is required.");

                if (string.IsNullOrWhiteSpace(criterion.Description))
                    validationErrors.Add("Description is required.");

                if (criterion.MaxScore <= 0)
                    validationErrors.Add("MaxScore must be a positive number.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление критерия в базу данных
                _context.Criteria.Add(criterion);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Criterion added successfully.",
                    criteriaId = criterion.CriteriaId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the criterion.",
                    error = ex.Message
                });
            }
        }

        // Изменение критерия
        [HttpPut("update-criterion/{id}")]
        public async Task<IActionResult> UpdateCriterion(int id, [FromBody] Criterion updatedCriterion)
        {
            try
            {
                if (id != updatedCriterion.CriteriaId)
                    return BadRequest("ID mismatch.");

                var existingCriterion = await _context.Criteria.FindAsync(id);

                if (existingCriterion == null)
                    return NotFound($"Criterion with ID {id} not found.");

                // Обновляем свойства существующего критерия
                existingCriterion.Name = updatedCriterion.Name;
                existingCriterion.Description = updatedCriterion.Description;
                existingCriterion.MaxScore = updatedCriterion.MaxScore;

                _context.Criteria.Update(existingCriterion);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Criterion updated successfully.",
                    criterion = existingCriterion
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the criterion.",
                    error = ex.Message
                });
            }
        }

        // Удаление критерия
        [HttpDelete("delete-criterion/{id}")]
        public async Task<IActionResult> DeleteCriterion(int id)
        {
            try
            {
                var criterion = await _context.Criteria.FindAsync(id);

                if (criterion == null)
                    return NotFound($"Criterion with ID {id} not found.");

                // Проверка на наличие связанных документов
                if (criterion.Documents.Any())
                {
                    return BadRequest("Cannot delete a criterion that has associated documents.");
                }

                _context.Criteria.Remove(criterion);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the criterion.",
                    error = ex.Message
                });
            }
        }

        // Получение критерия по ID
        [HttpGet("get-criterion/{id}")]
        public async Task<IActionResult> GetCriterionById(int id)
        {
            var criterion = await _context.Criteria
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CriteriaId == id);

            if (criterion == null)
                return NotFound($"Criterion with ID {id} not found.");

            return Ok(criterion);
        }

        // Получение всех критериев
        [HttpGet("get-all-criteria")]
        public async Task<IActionResult> GetAllCriteria()
        {
            var criteria = await _context.Criteria
                .Include(c => c.Documents)
                .ToListAsync();

            return Ok(criteria);
        }
    }

}
