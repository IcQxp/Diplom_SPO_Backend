using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class LessonController : AdminControllerBase
    {
        public LessonController(DiplomContext context) : base(context)
        {
        }


        [HttpPost("add-lesson")]
        public async Task<IActionResult> AddLesson([FromBody] Lesson lesson)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (lesson.DisciplineId <= 0 || !await _context.Disciplines.AnyAsync(d => d.DisciplineId == lesson.DisciplineId))
                    validationErrors.Add("DisciplineId must be a valid ID of an existing discipline.");

                if (lesson.GroupId <= 0 || !await _context.Groups.AnyAsync(g => g.GroupId == lesson.GroupId))
                    validationErrors.Add("GroupId must be a valid ID of an existing group.");

                if (lesson.LessonTimeId <= 0 || !await _context.LessonTimes.AnyAsync(lt => lt.LessonTimeId == lesson.LessonTimeId))
                    validationErrors.Add("LessonTimeId must be a valid ID of an existing lesson time.");

                if (lesson.EmployeeId <= 0 || !await _context.Employees.AnyAsync(e => e.EmployeeId == lesson.EmployeeId))
                    validationErrors.Add("EmployeeId must be a valid ID of an existing employee.");

                if (lesson.LessonDate == default || lesson.LessonDate > DateOnly.FromDateTime(DateTime.Now))
                    validationErrors.Add("LessonDate is required and must be a valid date in the past or present.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление урока в базу данных
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Lesson added successfully.",
                    lessonId = lesson.LessonId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the lesson.",
                    error = ex.Message
                });
            }
        }

        // Изменение урока
        [HttpPut("update-lesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] Lesson updatedLesson)
        {
            try
            {
                if (id != updatedLesson.LessonId)
                    return BadRequest("ID mismatch.");

                var existingLesson = await _context.Lessons.FindAsync(id);

                if (existingLesson == null)
                    return NotFound($"Lesson with ID {id} not found.");

                // Обновляем свойства существующего урока
                existingLesson.DisciplineId = updatedLesson.DisciplineId;
                existingLesson.GroupId = updatedLesson.GroupId;
                existingLesson.LessonTimeId = updatedLesson.LessonTimeId;
                existingLesson.LessonDate = updatedLesson.LessonDate;
                existingLesson.EmployeeId = updatedLesson.EmployeeId;

                _context.Lessons.Update(existingLesson);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Lesson updated successfully.",
                    lesson = existingLesson
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the lesson.",
                    error = ex.Message
                });
            }
        }

        // Удаление урока
        [HttpDelete("delete-lesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(id);

                if (lesson == null)
                    return NotFound($"Lesson with ID {id} not found.");

                // Проверка на наличие связанных оценок
                if (lesson.Grades.Any())
                {
                    return BadRequest("Cannot delete a lesson that has associated grades.");
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the lesson.",
                    error = ex.Message
                });
            }
        }

        // Получение урока по ID
        [HttpGet("get-lesson/{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Discipline)
                .Include(l => l.Group)
                .Include(l => l.Employee)
                .Include(l => l.LessonTime)
                .Include(l => l.Grades)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
                return NotFound($"Lesson with ID {id} not found.");

            return Ok(lesson);
        }

        // Получение всех уроков
        [HttpGet("get-all-lessons")]
        public async Task<IActionResult> GetAllLessons()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Discipline)
                .Include(l => l.Group)
                .Include(l => l.Employee)
                .Include(l => l.LessonTime)
                .Include(l => l.Grades)
                .ToListAsync();

            return Ok(lessons);
        }
    }
}