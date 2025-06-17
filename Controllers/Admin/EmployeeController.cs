using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DiplomBackend.Controllers.Admin
{

    public class EmployeeController : AdminControllerBase
    {
        public EmployeeController(DiplomContext context) : base(context)
        {
        }
              

        // Добавление сотрудника
        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            try
            {
                // Проверка входных данных
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(employee.Lastname))
                    validationErrors.Add("Lastname is required.");

                if (string.IsNullOrWhiteSpace(employee.Firstname))
                    validationErrors.Add("Firstname is required.");

                if (string.IsNullOrWhiteSpace(employee.Patronymic))
                    validationErrors.Add("Patronymic is required.");

                if (string.IsNullOrWhiteSpace(employee.GenderCode) || !IsValidGenderCode(employee.GenderCode))
                    validationErrors.Add("GenderCode is required and must be valid (e.g., 'M' or 'F').");

                if (employee.RoleId <= 0)
                    validationErrors.Add("RoleId must be a valid positive number.");

                if (string.IsNullOrWhiteSpace(employee.Login))
                    validationErrors.Add("Login is required.");
                else if (await _context.Employees.AnyAsync(e => e.Login == employee.Login))
                    validationErrors.Add("Login must be unique.");

                if (string.IsNullOrWhiteSpace(employee.Password))
                    validationErrors.Add("Password is required.");

                if (string.IsNullOrWhiteSpace(employee.Email) || !IsValidEmail(employee.Email))
                    validationErrors.Add("Email is required and must be valid.");

                if (string.IsNullOrWhiteSpace(employee.Telephone) || !IsValidTelephone(employee.Telephone))
                    validationErrors.Add("Telephone is required and must be valid.");

                if (employee.BirthDate == default || employee.BirthDate > DateOnly.FromDateTime(DateTime.Now))
                    validationErrors.Add("BirthDate is required and must be a valid date in the past.");

                // Если есть ошибки валидации, возвращаем их
                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed.",
                        errors = validationErrors
                    });
                }

                // Добавление сотрудника в базу данных
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Employee added successfully.",
                    employeeId = employee.EmployeeId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the employee.",
                    error = ex.Message
                });
            }
        }

        // Изменение сотрудника
        [HttpPut("update-employee/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            try
            {
                if (id != updatedEmployee.EmployeeId)
                    return BadRequest("ID mismatch.");

                var existingEmployee = await _context.Employees.FindAsync(id);

                if (existingEmployee == null)
                    return NotFound($"Employee with ID {id} not found.");

                // Обновляем свойства существующего сотрудника
                existingEmployee.Lastname = updatedEmployee.Lastname;
                existingEmployee.Firstname = updatedEmployee.Firstname;
                existingEmployee.Patronymic = updatedEmployee.Patronymic;
                existingEmployee.GenderCode = updatedEmployee.GenderCode;
                existingEmployee.BirthDate = updatedEmployee.BirthDate;
                existingEmployee.Login = updatedEmployee.Login;
                existingEmployee.Password = updatedEmployee.Password;
                existingEmployee.Email = updatedEmployee.Email;
                existingEmployee.Telephone = updatedEmployee.Telephone;
                existingEmployee.RoleId = updatedEmployee.RoleId;

                _context.Employees.Update(existingEmployee);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Employee updated successfully.",
                    employee = existingEmployee
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the employee.",
                    error = ex.Message
                });
            }
        }

        // Удаление сотрудника
        [HttpDelete("delete-employee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                    return NotFound($"Employee with ID {id} not found.");

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the employee.",
                    error = ex.Message
                });
            }
        }

        // Получение сотрудника по ID
        [HttpGet("get-employee/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .Include(e => e.GenderCodeNavigation)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound($"Employee with ID {id} not found.");

            return Ok(employee);
        }

        // Вспомогательные методы для валидации
        private bool IsValidGenderCode(string genderCode)
        {
            genderCode = genderCode.ToLower();
            return genderCode == "м" || genderCode == "ж";
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidTelephone(string telephone)
        {
            // Простая проверка формата телефона (например, +79991234567)
            return !string.IsNullOrWhiteSpace(telephone) && telephone.All(char.IsDigit) && telephone.Length >= 10;
        }

        [HttpGet("get-all-employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employee = await _context.Employees.ToListAsync();

            return Ok(employee);
        }
    }
}
