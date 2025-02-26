using Microsoft.AspNetCore.Mvc;

namespace DiplomBackend.Controllers
{
    public abstract class BackendBaseController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
