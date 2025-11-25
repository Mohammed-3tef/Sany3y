using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;
using Sany3y.Infrastructure.ViewModels; // لو حطينا الـ HomeIndexViewModel هنا

namespace Sany3y.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;

        public HomeController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            // جلب أول 8 كاتيجوريز من قاعدة البيانات
            var categories = await _categoryRepository.GetAll();
            var model = new HomeIndexViewModel
            {
                Categories = categories.Take(8).ToList()
            };
            return View(model);
        }

        public IActionResult Policy()
        {
            return View();
        }

        public IActionResult OurTeam()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }
    }
}