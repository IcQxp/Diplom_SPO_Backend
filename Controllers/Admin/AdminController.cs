using DiplomBackend.DB;
using Microsoft.AspNetCore.Mvc;

namespace DiplomBackend.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public abstract class AdminControllerBase : ControllerBase
    {
        protected readonly DiplomContext _context;

        protected AdminControllerBase(DiplomContext context)
        {
            _context = context;
        }

        protected IActionResult HandleException(Exception ex)
        {
            // Логирование ошибки (можно добавить реальное логирование)
            Console.Error.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
