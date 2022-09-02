using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RbaQuickstart.Models;

namespace RbaQuickstart.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var model = new LoginViewModel()
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await signInManager.UserManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                ModelState.AddModelError("login", "Invalid Credentials");
                return View(model);
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, new AuthenticationProperties());
                return Redirect(model.ReturnUrl ?? "/");
            }

            ModelState.AddModelError("login", "Invalid Credentials");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            var model = new LoginViewModel()
            {
                ReturnUrl = returnUrl
            };

            return View("Register", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginViewModel model)
        {
            var user = await signInManager.UserManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                ModelState.AddModelError("login", "Registration Failed");
                return View("Register", model);
            }

            var newUser = new IdentityUser(model.Username);

            var result = await signInManager.UserManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("login", "Registration Failed");
                return View("Register", model);
            }

            await signInManager.UserManager.AddPasswordAsync(newUser, model.Password);

            return Redirect(model.ReturnUrl ?? "/");
        }
    }
}
