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
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Здесь должна быть логика проверки учетных данных
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Login == model.Username && u.Password == model.Password);
            if (user == null)
                return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
                    // Добавьте другие утверждения, если необходимо
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            Console.WriteLine($"Generated token for user {user.Login}: {tokenString}");

            return Ok(new { Token = tokenString });
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
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Authorization header is missing or empty");
                return Unauthorized();
            }
            Console.WriteLine("token:\t" + token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            Console.WriteLine("key:\t" + key);

            try
            {
                Console.WriteLine("try");
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer, // Убедитесь, что это значение соответствует Issuer в токене
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience, // Убедитесь, что это значение соответствует Audience в токене
                    ClockSkew = TimeSpan.Zero
                };

                Console.WriteLine("validAud:\t" + validationParameters.ValidAudience);
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                Console.WriteLine("Token validation successful");

                var username = claimsPrincipal.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var user = _context.Employees.FirstOrDefault(u => u.Login == username);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    EmployeeId = user.EmployeeId,
                    Login = user.Login,
                    Firstname = user.Firstname
                    // Добавьте другие поля, которые хотите вернуть
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return Unauthorized();
            }
        }
    }

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

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents.ToListAsync();
                var documentsDto = documents.Select(doc => new DocumentsListDto
                {
                    DocumentId = doc.DocumentId,
                    FilePath = doc.FilePath,
                    DownloadDate = doc.DownloadDate,
                    StudentId = doc.StudentId,
                    Score = doc.Score,
                    Criteria = doc.Criteria,
                    DocumentType = doc.DocumentType,
                    Employee = doc.Employee,
                    Status = doc.Status

                });
                return Ok(documentsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            int studentId = 1;
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
            // Поиск документа в базе данных
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            // Проверка наличия пути к файлу
            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
            {
                return NotFound("File not found.");
            }

            // Чтение файла из файловой системы
            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0; // Установка позиции в начало потока

            // Возвращение файла с указанием MIME-типа и имени файла
            return File(memory, "application/pdf", Path.GetFileName(document.FilePath));
        }
    }

    public class UserSearchRequest
    {
        public string SearchItem { get; set; }
        public int? MinId { get; set; }
    }
}