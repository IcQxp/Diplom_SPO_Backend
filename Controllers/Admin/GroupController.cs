using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{
    public class GroupController : AdminControllerBase
    {
        public GroupController(DiplomContext context) : base(context)
        {
        }

        

        // Добавление группы
        [HttpPost("add-group")]
        public async Task<IActionResult> AddGroup([FromBody] Group group)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(group.GroupNumber))
                    validationErrors.Add("GroupNumber is required.");

                if (await _context.Groups.AnyAsync(g => g.GroupNumber == group.GroupNumber))
                    validationErrors.Add("GroupNumber must be unique.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление группы в базу данных
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Group added successfully.",
                    groupId = group.GroupId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the group.",
                    error = ex.Message
                });
            }
        }

        // Изменение группы
        [HttpPut("update-group/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] Group updatedGroup)
        {
            try
            {
                if (id != updatedGroup.GroupId)
                    return BadRequest("ID mismatch.");

                var existingGroup = await _context.Groups.FindAsync(id);

                if (existingGroup == null)
                    return NotFound($"Group with ID {id} not found.");

                // Обновляем свойства существующей группы
                existingGroup.GroupNumber = updatedGroup.GroupNumber;

                _context.Groups.Update(existingGroup);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Group updated successfully.",
                    group = existingGroup
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the group.",
                    error = ex.Message
                });
            }
        }

        // Удаление группы
        [HttpDelete("delete-group/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                var group = await _context.Groups.FindAsync(id);

                if (group == null)
                    return NotFound($"Group with ID {id} not found.");

                // Проверка на наличие связанных студентов и уроков
                if (group.Students.Any() || group.Lessons.Any())
                {
                    return BadRequest("Cannot delete a group that has associated students or lessons.");
                }

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the group.",
                    error = ex.Message
                });
            }
        }

        // Получение группы по ID
        [HttpGet("get-group/{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Students)
                .Include(g => g.Lessons)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
                return NotFound($"Group with ID {id} not found.");

            return Ok(group);
        }

        // Получение всех групп
        [HttpGet("get-all-groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.Students)
                .Include(g => g.Lessons)
                .ToListAsync();

            return Ok(groups);
        }

    }
}
