using Microsoft.AspNetCore.Mvc;

namespace DigitalSeal.Web.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Viewer()
        {
            return View();
        }
    }
}
