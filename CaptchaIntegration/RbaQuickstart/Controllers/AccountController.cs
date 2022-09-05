using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RbaQuickstart.Models;
using Rsk.RiskBasedAuthentication.Enums;
using Rsk.RiskBasedAuthentication.Services.Interfaces;

namespace RbaQuickstart.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IRbaAlertService alertService;

        public AccountController(SignInManager<IdentityUser> signInManager, IRbaAlertService alertService)
        {
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var model = new LoginViewModel();

            var alertLevel = await alertService.GetAlertLevel();

            model.ShowCaptcha = alertLevel >= AlertLevel.High;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var alertLevel = await alertService.GetAlertLevel();

            if (alertLevel >= AlertLevel.High)
            {
                //Verify Captcha
                if (!Request.Form.ContainsKey("g-recaptcha-response"))
                {
                    ModelState.AddModelError("captcha", "Captcha Missing");
                    return View(model);

                }
                var captcha = Request.Form["g-recaptcha-response"].ToString();

                if (!await IsCaptchaValid(captcha))
                {
                    ModelState.AddModelError("captcha", "Captcha Failed");
                    return View(model);
                }
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

        private async Task<bool> IsCaptchaValid(string captcha)
        {
            try
            {
                var captchaClient = new HttpClient();
                captchaClient.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
                var postTask = await captchaClient
                    .PostAsync($"?secret=API_KEY={captcha}", new StringContent(""));

                var result = await postTask.Content.ReadAsStringAsync();

                var resultObject = JObject.Parse(result);
                dynamic success = resultObject["success"];
                return (bool)success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
