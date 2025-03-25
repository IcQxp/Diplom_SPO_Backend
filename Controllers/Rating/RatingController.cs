using DiplomBackend.DB;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiplomBackend.Controllers.Rating
{
    [Route("api/rating")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly DiplomContext _context;
        private readonly string _storagePath;

        public RatingController(DiplomContext context)
        {
            _context = context;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [Route("categories")]
        [HttpGet]
        public async Task<IActionResult> GetRatingCategories()
        {
            try
            {
                var categories = await _context.Criteria.ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                // Логирование ошибки (можно добавить логирование в реальном приложении)
                return StatusCode(500, "An error occurred while processing your request.");
            }


        }


        [Route("{stringId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserRating(string stringId)
        {
            int id;
            bool success = int.TryParse(stringId, out id);
            if (success)
                try
                {
                    // Получаем пользователя по ID
                    var user = await _context.Students.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound("User not found.");
                    }

                    // Получаем все документы для указанного студента
                    var documents = await _context.Documents
                        .Where(d => d.StudentId == id)
                        .Include(d => d.Criteria) // Подгружаем связанные данные Criteria
                        .ToListAsync();

                    // Получаем все критерии из базы данных
                    var allCriteria = await _context.Criteria.ToListAsync();

                    // Группируем документы по CriteriaId и суммируем Score
                    var groupedScores = documents
                        .Where(d => d.Criteria != null && d.Score.HasValue) // Исключаем документы без критерия или баллов
                        .GroupBy(d => d.Criteria.Name) // Группируем по названию критерия
                        .ToDictionary(
                            group => group.Key, // Ключ: название критерия
                            group => group.Sum(d => d.Score.Value) // Значение: сумма баллов
                        );

                    // Формируем массив объектов в требуемом формате
                    var result = new
                    {
                        UserName = user.Lastname, // Фамилия пользователя
                        Ratings = allCriteria.Select(criterion =>
                        {
                            var rating = new Dictionary<string, object>
                    {
                    { "criteria", criterion.Name }, // Название критерия
                    { user.Lastname, groupedScores.ContainsKey(criterion.Name)
                        ? groupedScores[criterion.Name] // Если есть баллы, берем их
                        : 0 } // Если нет, ставим 0
                    };
                            return rating;
                        }).ToList()
                    };

                    // Возвращаем результат
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    // Логирование ошибки (можно добавить логирование в реальном приложении)
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            else
            {
                return NotFound("Такого пользователя нет. Неправильный ID");
            }
        }

        [Route("GetUsersRatings")]
        [HttpPost]
        public async Task<IActionResult> GetUsersRatings([FromBody] int[] userIds)
        {
            try
            {
                // 1. Проверяем входные данные
                if (userIds == null || userIds.Length == 0 || userIds.Length > 10)
                {
                    return BadRequest("You can request ratings for up to 10 users.");
                }

                // 2. Получаем всех студентов по переданным ID
                var users = await _context.Students
                    .Where(u => userIds.Contains(u.StudentId))
                    .ToListAsync();

                if (users.Count == 0)
                {
                    return NotFound("No users found with the provided IDs.");
                }

                // 3. Получаем все документы для указанных студентов
                var documents = await _context.Documents
                    .Where(d => userIds.Contains(d.StudentId) && d.Score.HasValue)
                    .Include(d => d.Criteria) // Подгружаем связанные данные Criteria
                    .ToListAsync();

                // 4. Получаем все критерии из базы данных
                var allCriteria = await _context.Criteria.ToListAsync();

                // 5. Группируем документы по студентам и критериям
                var groupedScores = documents
                    .GroupBy(d => new { d.StudentId, d.Criteria.Name }) // Группируем по ID студента и названию критерия
                    .ToDictionary(
                        group => (group.Key.StudentId, group.Key.Name), // Ключ: (ID студента, название критерия)
                        group => group.Sum(d => d.Score.Value) // Значение: сумма баллов
                    );

                // 6. Формируем список пользователей (ключи)
                var userKeys = users.Select(user =>
                    $"{user.Lastname} {user.Firstname} {user.Group?.GroupNumber ?? "No Group"}"
                ).ToList();

                // 7. Формируем результат (рейтинги)
                var ratings = allCriteria.Select(criterion =>
                {
                    var ratingData = new Dictionary<string, object>
                    {
                        ["criteria"] = criterion.Name // Название критерия
                    };

                    // Добавляем баллы для каждого пользователя
                    foreach (var user in users)
                    {
                        var key = (user.StudentId, criterion.Name);
                        var score = groupedScores.ContainsKey(key)
                            ? groupedScores[key] // Если есть баллы, берем их
                            : 0; // Если нет, ставим 0

                        // Формируем ключ для словаря: ФИО + группа
                        var userKey = $"{user.Lastname} {user.Firstname} {user.Group?.GroupNumber ?? "No Group"}";
                        ratingData[userKey] = score;
                    }

                    return ratingData;
                }).ToList();

                // 8. Формируем итоговый ответ
                var result = new ChartResponse
                {
                    Keys = userKeys,
                    Data = ratings
                };

                // 9. Возвращаем результат
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Логирование ошибки (можно добавить логирование в реальном приложении)
                Console.Error.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public class StudentScoreResult
        {
            public string FullName { get; set; }
            public int TotalScore { get; set; }
        }

        public class ChartResponse
        {
            public List<string> Keys { get; set; } = new List<string>(); // Названия критериев
            public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
        }

        public class ChartData
        {
            public string Country { get; set; } // Название критерия
            public Dictionary<string, int> Scores { get; set; } = new Dictionary<string, int>();
        }

        [HttpGet("top-students")]
        public IActionResult GetTopStudents(int count, int? criteriaId = null)
        {
            // 1. Получаем все критерии
            var criteria = _context.Criteria.ToList();

            // 2. Получаем всех студентов
            var students = _context.Students.ToList();

            // 3. Формируем данные для каждого студента
            var studentScores = students.Select(student => new
            {
                FullName = $"{student.Lastname} {student.Firstname}", // Полное имя студента
                studentid = student.StudentId,
                Scores = criteria.ToDictionary(
                    criterion => criterion.Name,
                    criterion =>
                    {
                        // Баллы студента по этому критерию
                        var score = _context.Documents
                            .Where(d => d.StudentId == student.StudentId && d.CriteriaId == criterion.CriteriaId && d.Score.HasValue)
                            .Sum(d => d.Score.GetValueOrDefault(0));
                        return score;
                    }),
                TotalScore = criteria.Sum(criterion =>
                {
                    // Сумма баллов по всем критериям
                    var score = _context.Documents
                        .Where(d => d.StudentId == student.StudentId && d.CriteriaId == criterion.CriteriaId && d.Score.HasValue)
                        .Sum(d => d.Score.GetValueOrDefault(0));
                    return score;
                })
            }).ToList();

            // 4. Фильтруем и сортируем студентов
            IEnumerable<dynamic> topStudents;
            if (criteriaId.HasValue)
            {
                // Если указан criteriaId, фильтруем по этому критерию
                var criterion = criteria.FirstOrDefault(c => c.CriteriaId == criteriaId.Value);
                if (criterion == null)
                {
                    return NotFound($"Критерий с ID {criteriaId} не найден.");
                }

                topStudents = studentScores
                    .OrderByDescending(s => s.Scores[criterion.Name]) // Сортировка по баллам конкретного критерия
                    .Take(count); // Топ count студентов
            }
            else
            {
                // Если criteriaId не указан, сортируем по сумме баллов
                topStudents = studentScores
                    .OrderByDescending(s => s.TotalScore) // Сортировка по общей сумме баллов
                    .Take(count); // Топ count студентов
            }

            // 5. Форматируем данные для ответа
            var keys = criteria.Select(c => c.Name).ToList();

            var data = topStudents.Select(student => new Dictionary<string, object>
            {
                ["country"] = student.FullName, // Полное имя студента
                ["studentid"] = student.studentid,
            }.Concat(keys.SelectMany(key =>
            {
                // Добавляем баллы и цвета для каждого критерия
                var score = student.Scores[key];
                return new Dictionary<string, object>
        {
            { key, score }, // Значение баллов
            //{ $"{key}Color", GenerateRandomHslColor() } // Цвет
        };
            })).ToDictionary(pair => pair.Key, pair => pair.Value)).ToList();

            // 6. Формируем итоговый ответ
            var response = new ChartResponse
            {
                Keys = keys,
                Data = data
            };

            return Ok(response);
        }

        private static string GenerateRandomHslColor()
        {
            var random = new Random();
            var hue = random.Next(0, 360); // Оттенок (0-360)
            var saturation = 70; // Насыщенность (фиксированная)
            var lightness = 50; // Яркость (фиксированная)
            return $"hsl({hue}, {saturation}%, {lightness}%)";
        }

        public class TopStudentsRequest
        {
            public int Count { get; set; }
            public int[] CriteriaIDs { get; set; }
        }

        [HttpPost("top-students-array")]
        public IActionResult GetTopStudents([FromBody] TopStudentsRequest request)
        {
            int count = request.Count;
            int[] criteriaIDs = request.CriteriaIDs;

            // 1. Получаем все критерии
            var allCriteria = _context.Criteria.ToList();

            // 2. Фильтруем критерии, если указан массив criteriaIDs
            var filteredCriteria = criteriaIDs != null && criteriaIDs.Length > 0
                ? allCriteria.Where(c => criteriaIDs.Contains(c.CriteriaId)).ToList()
                : allCriteria;

            // 3. Получаем всех студентов
            var students = _context.Students.ToList();

            // 4. Получаем топ студентов
            var studentScores = students.Select(student => new
            {
                FullName = $"{student.Lastname} {student.Firstname}", // Полное имя студента
                StudentId = student.StudentId, // ID студента
                Scores = filteredCriteria.ToDictionary(
                    criterion => criterion.Name,
                    criterion =>
                    {
                        // Баллы студента по этому критерию
                        var score = _context.Documents
                            .Where(d => d.StudentId == student.StudentId && d.CriteriaId == criterion.CriteriaId && d.Score.HasValue)
                            .Sum(d => d.Score.GetValueOrDefault(0));
                        return score;
                    }),
                TotalScore = filteredCriteria.Sum(criterion =>
                {
                    // Сумма баллов по выбранным критериям
                    var score = _context.Documents
                        .Where(d => d.StudentId == student.StudentId && d.CriteriaId == criterion.CriteriaId && d.Score.HasValue)
                        .Sum(d => d.Score.GetValueOrDefault(0));
                    return score;
                })
            }).ToList();

            // 5. Сортируем студентов по общей сумме баллов по выбранным критериям
            var topStudents = studentScores
                .OrderByDescending(s => s.TotalScore) // Сортировка по общей сумме баллов
                .Take(count) // Топ count студентов
                .ToList();

            // 6. Формируем keys (студенты)
            var keys = topStudents.Select(student => new Dictionary<string, string>
    {
        { student.StudentId.ToString(), student.FullName }
    }).ToList();

            // 7. Формируем data (критерии и баллы студентов)
            var data = filteredCriteria.Select(criterion => new Dictionary<string, object>
            {
                ["criteria"] = criterion.Name
            }.Concat(topStudents.Select(student => new KeyValuePair<string, object>(
                student.FullName, // Имя студента
                student.Scores[criterion.Name] // Баллы студента по этому критерию
            ))).ToDictionary(pair => pair.Key, pair => pair.Value)).ToList();

            // 8. Формируем итоговый ответ
            var response = new
            {
                Keys = keys,
                Data = data
            };

            return Ok(response);
        }

        [HttpGet("grade/{id}")]
        public IActionResult GetUserGrades(int id)
        {

            var grades = _context.Grades.Where(g => g.StudentId == id);

            return Ok(grades);
        }

    }
}
