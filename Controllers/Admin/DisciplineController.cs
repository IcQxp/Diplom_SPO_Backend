using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class DisciplineController : AdminControllerBase
    {
        public DisciplineController(DiplomContext context) : base(context)
        {
        }


        // Добавление дисциплины
        [HttpPost("add-discipline")]
        public async Task<IActionResult> AddDiscipline([FromBody] Discipline discipline)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(discipline.Name))
                    validationErrors.Add("Name is required.");

                if (await _context.Disciplines.AnyAsync(d => d.Name == discipline.Name))
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

                // Добавление дисциплины в базу данных
                _context.Disciplines.Add(discipline);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Discipline added successfully.",
                    disciplineId = discipline.DisciplineId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the discipline.",
                    error = ex.Message
                });
            }
        }

        // Изменение дисциплины
        [HttpPut("update-discipline/{id}")]
        public async Task<IActionResult> UpdateDiscipline(int id, [FromBody] Discipline updatedDiscipline)
        {
            try
            {
                if (id != updatedDiscipline.DisciplineId)
                    return BadRequest("ID mismatch.");

                var existingDiscipline = await _context.Disciplines.FindAsync(id);

                if (existingDiscipline == null)
                    return NotFound($"Discipline with ID {id} not found.");

                // Обновляем свойства существующей дисциплины
                existingDiscipline.Name = updatedDiscipline.Name;

                _context.Disciplines.Update(existingDiscipline);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Discipline updated successfully.",
                    discipline = existingDiscipline
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the discipline.",
                    error = ex.Message
                });
            }
        }

        // Удаление дисциплины
        [HttpDelete("delete-discipline/{id}")]
        public async Task<IActionResult> DeleteDiscipline(int id)
        {
            try
            {
                var discipline = await _context.Disciplines.FindAsync(id);

                if (discipline == null)
                    return NotFound($"Discipline with ID {id} not found.");

                // Проверка на наличие связанных уроков
                if (discipline.Lessons.Any())
                {
                    return BadRequest("Cannot delete a discipline that has associated lessons.");
                }

                _context.Disciplines.Remove(discipline);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the discipline.",
                    error = ex.Message
                });
            }
        }

        // Получение дисциплины по ID
        [HttpGet("get-discipline/{id}")]
        public async Task<IActionResult> GetDisciplineById(int id)
        {
            var discipline = await _context.Disciplines
                .Include(d => d.Lessons)
                .FirstOrDefaultAsync(d => d.DisciplineId == id);

            if (discipline == null)
                return NotFound($"Discipline with ID {id} not found.");

            return Ok(discipline);
        }

        // Получение всех дисциплин
        [HttpGet("get-all-disciplines")]
        public async Task<IActionResult> GetAllDisciplines()
        {
            var disciplines = await _context.Disciplines
                .Include(d => d.Lessons)
                .ToListAsync();

            return Ok(disciplines);
        }

    }
}
