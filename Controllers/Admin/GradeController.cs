using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class GradeController : AdminControllerBase
    {
        public GradeController(DiplomContext context) : base(context)
        {
        }

        // Добавление оценки
        [HttpPost("add-grade")]
        public async Task<IActionResult> AddGrade([FromBody] Grade grade)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (grade.StudentId <= 0 || !await _context.Students.AnyAsync(s => s.StudentId == grade.StudentId))
                    validationErrors.Add("StudentId must be a valid ID of an existing student.");

                if (grade.LessonId <= 0 || !await _context.Lessons.AnyAsync(l => l.LessonId == grade.LessonId))
                    validationErrors.Add("LessonId must be a valid ID of an existing lesson.");

                if (grade.Value < 0 || grade.Value > 100) // Предположим, что оценка находится в диапазоне от 0 до 100
                    validationErrors.Add("Value must be between 0 and 100.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление оценки в базу данных
                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Grade added successfully.",
                    gradeId = grade.GradeId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the grade.",
                    error = ex.Message
                });
            }
        }

        // Изменение оценки
        [HttpPut("update-grade/{id}")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] Grade updatedGrade)
        {
            try
            {
                if (id != updatedGrade.GradeId)
                    return BadRequest("ID mismatch.");

                var existingGrade = await _context.Grades.FindAsync(id);

                if (existingGrade == null)
                    return NotFound($"Grade with ID {id} not found.");

                // Обновляем свойства существующей оценки
                existingGrade.StudentId = updatedGrade.StudentId;
                existingGrade.LessonId = updatedGrade.LessonId;
                existingGrade.Value = updatedGrade.Value;

                _context.Grades.Update(existingGrade);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Grade updated successfully.",
                    grade = existingGrade
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the grade.",
                    error = ex.Message
                });
            }
        }

        // Удаление оценки
        [HttpDelete("delete-grade/{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try
            {
                var grade = await _context.Grades.FindAsync(id);

                if (grade == null)
                    return NotFound($"Grade with ID {id} not found.");

                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the grade.",
                    error = ex.Message
                });
            }
        }

        // Получение оценки по ID
        [HttpGet("get-grade/{id}")]
        public async Task<IActionResult> GetGradeById(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Lesson)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
                return NotFound($"Grade with ID {id} not found.");

            return Ok(grade);
        }

        // Получение всех оценок
        [HttpGet("get-all-grades")]
        public async Task<IActionResult> GetAllGrades()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Lesson)
                .ToListAsync();

            return Ok(grades);
        }
    }
    
}
