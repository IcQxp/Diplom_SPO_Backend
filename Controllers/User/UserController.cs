using DiplomBackend.DB;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomBackend.Controllers.User
{
    [ApiController]
    [Route("api/students")]
    public class UserController:ControllerBase
    {
        private readonly DiplomContext _context;
        private readonly string _storagePath;

        public UserController(DiplomContext context)
        {
            _context = context;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserID(int id)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == id);
                if (student != null)
                {
                    Console.Write(student.Firstname);
                    var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == student.GroupId);
                    if (group != null)
                    {
                        Console.Write(group.GroupNumber);
                        student.Group = group;
                        return Ok(student);
                    }
                    else
                    {
                        return NotFound("Ошибка, группа студента не найдена");
                    }
                }
                else
                {
                    return NotFound("Ошибка, студент не найден");
                }
            }
            catch(Exception e)
            {
                return StatusCode(500, "Ошибка "+e.Message);
            }
        }

    }

}
