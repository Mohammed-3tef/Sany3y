using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;
using Sany3y.Infrastructure.Repositories;

namespace Sany3y.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserRepository _userRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<ProfilePicture> _profilePictureRepository;
        private readonly IRepository<Category> _categoryRepository;

        public AdminController(
            UserManager<User> userManager,
            UserRepository userRepository,
            SignInManager<User> signInManager,
            IRepository<Address> addressRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProfilePicture> profilePictureRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _signInManager = signInManager;
            _addressRepository = addressRepository;
            _categoryRepository = categoryRepository;
            _profilePictureRepository = profilePictureRepository;
        }

        [Authorize]
        public IActionResult Index()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Users()
        {
            if (!User.IsInRole("Admin")) 
                return Forbid();

            var users = await _userRepository.GetAll();
            var userRoles = new List<(User User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add((user, roles));
            }

            return View(userRoles);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ViewUser(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Address = await _addressRepository.GetById(user.AddressId);
            var viewModel = (User: user, Roles: roles);
            return PartialView("_ViewUserModal", viewModel);
        }

        [Authorize]
        public IActionResult Categories()
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var categories = _categoryRepository.GetAll().Result;
            return View(categories);
        }
    }
}
