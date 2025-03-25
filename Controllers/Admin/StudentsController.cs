using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomBackend.Controllers.Admin
{
    public class StudentsController : AdminControllerBase
    {
        public StudentsController(DiplomContext context) : base(context)
        {
        }

        [HttpPost("add-student")]
        public async Task<IActionResult> AddStudent([FromBody] Student student)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(student.Lastname))
                {
                    validationErrors.Add("Lastname is required.");
                }

                if (string.IsNullOrWhiteSpace(student.Firstname))
                {
                    validationErrors.Add("Firstname is required.");
                }

                if (string.IsNullOrWhiteSpace(student.Patronymic))
                {
                    validationErrors.Add("Patronymic is required.");
                }

                if (string.IsNullOrWhiteSpace(student.GenderCode) || !IsValidGenderCode(student.GenderCode))
                {
                    validationErrors.Add("GenderCode is required and must be valid (e.g., 'M' or 'F').");
                }

                if (student.GroupId <= 0)
                {
                    validationErrors.Add("GroupId must be a valid positive number.");
                }

                if (string.IsNullOrWhiteSpace(student.Login))
                {
                    validationErrors.Add("Login is required.");
                }
                else if (await _context.Students.AnyAsync(s => s.Login == student.Login))
                {
                    validationErrors.Add("Login must be unique.");
                }

                if (string.IsNullOrWhiteSpace(student.Password))
                {
                    validationErrors.Add("Password is required.");
                }

                if (student.BirthDate == default || student.BirthDate > DateOnly.FromDateTime(DateTime.Now))
                {
                    validationErrors.Add("BirthDate is required and must be a valid date in the past.");
                }

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление студента в базу данных
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Возвращаем успешный ответ с ID добавленного студента
                return Ok(new
                {
                    message = "Student added successfully.",
                    studentId = student.StudentId
                });
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем серверную ошибку
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the student.",
                    error = ex.Message
                });
            }
        }

        // Метод для проверки допустимых значений GenderCode
        private bool IsValidGenderCode(string genderCode)
        {
            genderCode = genderCode.ToLower();
            return genderCode == "м" || genderCode == "ж";
        }


        [HttpDelete("delete-student/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound($"Студент с ID {id} не найден.");
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при удалении студента: " + ex.Message);
            }
        }

        [HttpPut("update-student/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            if (!ModelState.IsValid || id != updatedStudent.StudentId)
            {
                return BadRequest("Неверные данные или ID не совпадает.");
            }

            var existingStudent = await _context.Students.FindAsync(id);

            if (existingStudent == null)
            {
                return NotFound($"Студент с ID {id} не найден.");
            }

            try
            {
                existingStudent.Lastname = updatedStudent.Lastname;
                existingStudent.Firstname = updatedStudent.Firstname;
                existingStudent.Patronymic = updatedStudent.Patronymic;
                existingStudent.GenderCode = updatedStudent.GenderCode;
                existingStudent.GroupId = updatedStudent.GroupId;
                existingStudent.Login = updatedStudent.Login;
                existingStudent.Password = updatedStudent.Password;
                existingStudent.BirthDate = updatedStudent.BirthDate;

                _context.Students.Update(existingStudent);
                await _context.SaveChangesAsync();

                return Ok(existingStudent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при обновлении студента: " + ex.Message);
            }
        }
    }
}
