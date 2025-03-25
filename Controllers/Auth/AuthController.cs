using DiplomBackend.DB;
using DiplomBackend.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiplomBackend.Controllers.Auth
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly DiplomContext _context;

        public AuthController(IOptions<JwtSettings> jwtSettings, DiplomContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool isEmployee { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginEmployee([FromBody] LoginModel model)
        {
            var student = new Student();
            var employee = new Employee();
            employee = null;
            student = null;
            if (model.isEmployee)
                employee = await _context.Employees.FirstOrDefaultAsync(u => u.Login == model.Username && u.Password == model.Password);
            else
                student = await _context.Students.FirstOrDefaultAsync(u => u.Login == model.Username && u.Password == model.Password);


            if (employee == null && student == null)
                return Unauthorized();

            var role = model.isEmployee ? await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == employee.RoleId) : new Role { RoleId = 0, Name = "Студент" };
            string rolename = role?.Name;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name,  model.isEmployee? employee.Login:student.Login),
                new Claim(ClaimTypes.Role,  model.isEmployee? employee.RoleId.ToString():"0")
                    // Добавьте другие утверждения, если необходимо
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                user = new EmployeeDto
                {
                    lastname = model.isEmployee ? employee.Lastname : student.Lastname,
                    firstname = model.isEmployee ? employee.Firstname : student.Firstname,
                    patronymic = model.isEmployee ? employee.Patronymic : student.Patronymic,
                    login = model.isEmployee ? employee.Login : student.Login,
                    gender = model.isEmployee ? employee.GenderCode : student.GenderCode,
                    id = model.isEmployee ? employee.EmployeeId : student.StudentId,
                    email = model.isEmployee ? employee.Email : "-",
                    telephone = model.isEmployee ? employee.Telephone : "-",
                    rolename = rolename,
                    roleId = model.isEmployee ? employee.RoleId : 0,
                    birthDate = model.isEmployee ? employee.BirthDate : student.BirthDate

                }
            });
        }

        class EmployeeDto
        {
            public string? lastname { get; set; }
            public string? firstname { get; set; }
            public string? patronymic { get; set; }
            public string? login { get; set; }
            public string? gender { get; set; }
            public int id { get; set; }
            public string? email { get; set; }
            public string? telephone { get; set; }
            public string? rolename { get; set; }
            public int roleId { get; set; }
            public DateOnly birthDate { get; set; }
        }

        [Authorize(Policy = "RequireRoleId1")]
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok(new { Status = 1 });
        }




        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var roleId = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(roleId))
                return Unauthorized("Invalid token or user not found.");

            var rolename = roleId == "0" ? "Студент" : _context.Roles.FirstOrDefault(r => r.RoleId.ToString() == roleId)?.Name;

            if (rolename == null)
                return Unauthorized("Invalid role.");

            object userData;

            if (roleId == "0")
            {
                var student = _context.Students.FirstOrDefault(u => u.Login == username);
                if (student == null)
                    return NotFound();

                userData = new EmployeeDto
                {
                    lastname = student.Lastname,
                    firstname = student.Firstname,
                    patronymic = student.Patronymic,
                    login = student.Login,
                    gender = student.GenderCode,
                    id = student.StudentId,
                    email = "-",
                    telephone = "-",
                    rolename = rolename,
                    roleId = 0,
                    birthDate = student.BirthDate
                };
            }
            else
            {
                var employee = _context.Employees.FirstOrDefault(u => u.Login == username);
                if (employee == null)
                    return NotFound();

                userData = new EmployeeDto
                {
                    lastname = employee.Lastname,
                    firstname = employee.Firstname,
                    patronymic = employee.Patronymic,
                    login = employee.Login,
                    gender = employee.GenderCode,
                    id = employee.EmployeeId,
                    email = employee.Email,
                    telephone = employee.Telephone,
                    rolename = rolename,
                    roleId = employee.RoleId,
                    birthDate = employee.BirthDate
                };
            }

            return Ok(new { user = userData });
        }
    }
}
