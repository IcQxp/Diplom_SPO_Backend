using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomBackend.DB;
using DiplomBackend.JWT;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using DiplomBackend.Models;

namespace DiplomBackend.Controllers
{
    

    [Route("api/gender")]
    [ApiController]
    [Authorize]
    public class GenderController : ControllerBase
    {
        private readonly DiplomContext _context;

        public GenderController(DiplomContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            var users = await _context.Genders.ToListAsync();
            return Ok(users);
        }
    }
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DiplomContext _context;


        public UsersController(DiplomContext context)
        {
            _context = context;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            var users = await _context.Students.ToListAsync();
            return Ok(users);
        }



        [HttpGet("get-user-all-marks")]
        public async Task<ActionResult<IEnumerable<Student>>> GetUserAllMarks()
        {

            int userTestID = 1;
            int TestSubjectId = 1;

            var marks = await _context.Grades.Where(x => x.StudentId == userTestID).ToListAsync();
            return Ok(marks.Select(x => x.GradeId));
        }


        [HttpPost("add-user")]
        public async Task<ActionResult<Student>> AddUser(Student user)
        {
            _context.Students.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllStudents), new { id = user.StudentId }, user);
        }

        [HttpGet("find/{searchItem}")]
        public async Task<ActionResult<IEnumerable<Student>>> FindUsers(string searchItem, [FromQuery] string searchItem2)
        {
            if (string.IsNullOrEmpty(searchItem))
            {
                return BadRequest("Search term cannot be empty.");
            }

            var users = await _context.Students
                .Where(u => u.Login != null && u.Login.ToLower().Contains(searchItem2.ToLower()))
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound("No users found matching the criteria.");
            }

            return Ok(users);
        }

        [HttpPost("find")]
        public async Task<ActionResult<IEnumerable<Student>>> FindUsersWithCriteria([FromBody] UserSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.SearchItem) || request.SearchItem.Length > 100)
            {
                return BadRequest("Search term cannot be empty and must not exceed 100 characters.");
            }

            var query = _context.Students.AsQueryable();
            query = query.Where(u => u.Login != null && u.Login.ToLower().Contains(request.SearchItem.ToLower()));

            if (request.MinId.HasValue)
            {
                query = query.Where(u => u.StudentId >= request.MinId.Value);
            }

            var users = await query.ToListAsync();

            if (!users.Any())
            {
                return NotFound("No users found matching the criteria.");
            }

            return Ok(users);
        }
    }


    public class UserSearchRequest
    {
        public string SearchItem { get; set; }
        public int? MinId { get; set; }
    }
}