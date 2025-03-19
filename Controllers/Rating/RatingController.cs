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

        //[Route("{id}")]
        //[HttpGet]
        //public async Task<IActionResult> GetUserRating(int id)
        //{
        //    try
        //    {
        //        // Получаем все документы для указанного студента
        //        var documents = await _context.Documents
        //            .Where(d => d.StudentId == id)
        //            .Include(d => d.Criteria) // Подгружаем связанные данные Criteria
        //            .ToListAsync();

        //        // Получаем все критерии из базы данных
        //        var allCriteria = await _context.Criteria.ToListAsync();

        //        // Группируем документы по CriteriaId и суммируем Score
        //        var groupedScores = documents
        //            .Where(d => d.Criteria != null && d.Score.HasValue) // Исключаем документы без критерия или баллов
        //            .GroupBy(d => d.Criteria.Name) // Группируем по названию критерия
        //            .ToDictionary(
        //                group => group.Key, // Ключ: название критерия
        //                group => group.Sum(d => d.Score.Value) // Значение: сумма баллов
        //            );

        //        // Формируем результат, включая все критерии
        //        var result = allCriteria
        //            .ToDictionary(
        //                criterion => criterion.Name, // Ключ: название критерия
        //                criterion => groupedScores.ContainsKey(criterion.Name)
        //                    ? groupedScores[criterion.Name] // Если есть баллы, берем их
        //                    : 0 // Если нет, ставим 0
        //            );

        //        // Возвращаем результат
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Логирование ошибки (можно добавить логирование в реальном приложении)
        //        return StatusCode(500, "An error occurred while processing your request.");
        //    }
        //}

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
                // Проверяем, что передано не более 10 ID
                if (userIds == null || userIds.Length == 0 || userIds.Length > 10)
                {
                    return BadRequest("You can request ratings for up to 10 users.");
                }

                // Получаем всех пользователей по переданным ID
                var users = await _context.Students
                    .Where(u => userIds.Contains(u.StudentId))
                    .ToListAsync();

                if (users.Count == 0)
                {
                    return NotFound("No users found with the provided IDs.");
                }

                // Получаем все документы для указанных студентов
                var documents = await _context.Documents
                    .Where(d => userIds.Contains(d.StudentId))
                    .Include(d => d.Criteria) // Подгружаем связанные данные Criteria
                    .ToListAsync();

                // Получаем все критерии из базы данных
                var allCriteria = await _context.Criteria.ToListAsync();

                // Группируем документы по CriteriaId и суммируем Score для каждого пользователя
                var groupedScores = documents
                    .Where(d => d.Criteria != null && d.Score.HasValue) // Исключаем документы без критерия или баллов
                    .GroupBy(d => new { d.StudentId, d.Criteria.Name }) // Группируем по ID студента и названию критерия
                    .ToDictionary(
                        group => (group.Key.StudentId, group.Key.Name), // Ключ: (ID студента, название критерия)
                        group => group.Sum(d => d.Score.Value) // Значение: сумма баллов
                    );

                // Формируем результат
                var userKeys = users.Select(user => $"{user.Lastname} {user.Firstname}, {user.Group}").ToList();

                var result = new
                {
                    Users = userKeys,
                    Ratings = allCriteria.Select(criterion =>
                    {
                        // Создаем начальный словарь с названием критерия
                        var initialDict = new Dictionary<string, object>
        {
            { "criteria", criterion.Name } // Название критерия
        };

                        // Преобразуем словарь в IEnumerable<KeyValuePair<string, object>>
                        var initialEnumerable = initialDict.AsEnumerable();

                        // Добавляем данные о баллах для каждого пользователя
                        var userScores = users.ToDictionary(
                            user => $"{user.Lastname} {user.Firstname}, {user.Group}", // Ключ: "Фамилия Имя, Группа"
                            user => groupedScores.ContainsKey((user.StudentId, criterion.Name))
                                ? groupedScores[(user.StudentId, criterion.Name)] // Если есть баллы, берем их
                                : 0 // Если нет, ставим 0
                        );

                        // Приводим userScores к типу IEnumerable<KeyValuePair<string, object>>
                        var userScoresEnumerable = userScores.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value));

                        // Объединяем начальный словарь и данные о баллах
                        return initialEnumerable.Concat(userScoresEnumerable).ToDictionary(pair => pair.Key, pair => pair.Value);
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
        }


        public class StudentScoreResult
        {
            public string FullName { get; set; }
            public int TotalScore { get; set; }
        }

        //[HttpGet("top-students")]
        //public IActionResult GetTopStudents(int? criteriaId, int count)
        //{
        //    if (count <= 0)
        //    {
        //        return BadRequest("Count must be greater than zero.");
        //    }

        //    // Запрос для получения данных
        //    var query = _context.Documents
        //        .Where(d => d.StudentId != null) // Убедимся, что документ связан со студентом
        //        .GroupBy(d => d.StudentId) // Группируем по студенту
        //        .Select(g => new
        //        {
        //            StudentId = g.Key,
        //            TotalScore = criteriaId.HasValue
        //                ? g.Where(d => d.CriteriaId == criteriaId).Sum(d => d.Score.GetValueOrDefault(0)) // Если указан критерий
        //                : g.Sum(d => d.Score.GetValueOrDefault(0)) // Общие баллы
        //        })
        //        .OrderByDescending(x => x.TotalScore) // Сортируем по убыванию баллов
        //        .Take(count); // Ограничиваем количество записей

        //    // Получаем данные студентов
        //    var result = query
        //        .Join(
        //            _context.Students,
        //            doc => doc.StudentId,
        //            student => student.StudentId,
        //            (doc, student) => new StudentScoreResult
        //            {
        //                FullName = $"{student.Lastname} {student.Firstname} {student.Patronymic}",
        //                TotalScore = doc.TotalScore
        //            })
        //        .ToList();

        //    return Ok(result);
        //}

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
        public IActionResult GetTopStudents()
        {



            // 1. Получаем все критерии
            var criteria = _context.Criteria.ToList();

            // 2. Получаем всех студентов
            var students = _context.Students.ToList();

            // 3. Создаем список названий критериев (ключи)
            var keys = criteria.Select(c => c.Name).ToList();

            // 4. Формируем данные для каждого студента
            var data = students.Select(student => new Dictionary<string, object>
            {
                ["country"] = $"{student.Lastname} {student.Firstname}" // Полное имя студента
            }.Concat(criteria.SelectMany(criterion =>
            {
                // Баллы студента по этому критерию
                var score = _context.Documents
                    .Where(d => d.StudentId == student.StudentId && d.CriteriaId == criterion.CriteriaId && d.Score.HasValue)
                    .Sum(d => d.Score.GetValueOrDefault(0));

                // Добавляем два поля: баллы и цвет
                return new Dictionary<string, object>
        {
            { criterion.Name, score }, // Значение баллов
            { $"{criterion.Name}Color", GenerateRandomHslColor() } // Цвет
        };
            })).ToDictionary(pair => pair.Key, pair => pair.Value)).ToList();

            // 5. Формируем итоговый ответ
            var response = new ChartResponse
            {
                Keys = keys,
                Data = data
            };


            

            return Ok(response);
        }

        // Метод для генерации случайного HSL-цвета
        private static string GenerateRandomHslColor()
        {
            var random = new Random();
            var hue = random.Next(0, 360); // Оттенок (0-360)
            var saturation = 70; // Насыщенность (фиксированная)
            var lightness = 50; // Яркость (фиксированная)
            return $"hsl({hue}, {saturation}%, {lightness}%)";
        }
    }
}
