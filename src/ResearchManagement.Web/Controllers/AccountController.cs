using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Domain.Enums;
using ResearchManagement.Application.Interfaces;
namespace ResearchManagement.Web.Controllers
{
   
        public class AccountController : BaseController
        {
            private readonly SignInManager<User> _signInManager;
            private readonly IEmailService _emailService;

            public AccountController(
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                IEmailService emailService) : base(userManager)
            {
                _signInManager = signInManager;
                _emailService = emailService;
            }

            [HttpGet]
            public IActionResult Register()
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home");

                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Register(CreateUserDto model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم مسبقاً");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FirstNameEn = model.FirstNameEn,
                    LastNameEn = model.LastNameEn,
                    Institution = model.Institution,
                    AcademicDegree = model.AcademicDegree,
                    OrcidId = model.OrcidId,
                    Role = model.Role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                // إضافة المستخدم إلى الدور المناسب
                var roleName = model.Role switch
                {
                    UserRole.Researcher => "Researcher",
                    UserRole.Reviewer => "Reviewer",
                    UserRole.TrackManager => "TrackManager",
                    UserRole.ConferenceManager => "ConferenceManager",
                    UserRole.SystemAdmin => "SystemAdmin",
                    _ => "Researcher"
                };

                await _userManager.AddToRoleAsync(user, roleName);

                // إرسال بريد تأكيد
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token }, Request.Scheme);

                    // TODO: إرسال البريد الإلكتروني

                    AddSuccessMessage("تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني.");
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            [HttpGet]
            public IActionResult Login(string? returnUrl = null)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home");

                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
            {
                ViewData["ReturnUrl"] = returnUrl;

                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !user.IsActive)
                {
                    ModelState.AddModelError("", "بيانات الدخول غير صحيحة");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Dashboard");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "تم قفل الحساب مؤقتاً بسبب محاولات دخول متعددة");
                    return View(model);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                }

                ModelState.AddModelError("", "بيانات الدخول غير صحيحة");
                return View(model);
            }

            [HttpPost]
            [Authorize]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Logout()
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            [HttpGet]
            public async Task<IActionResult> ConfirmEmail(string userId, string token)
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                    return BadRequest();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound();

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    AddSuccessMessage("تم تأكيد البريد الإلكتروني بنجاح");
                    return RedirectToAction("Login");
                }

                AddErrorMessage("حدث خطأ في تأكيد البريد الإلكتروني");
                return View();
            }

            [HttpGet]
            public IActionResult ForgotPassword()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> ForgotPassword(string email)
            {
                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("", "يرجى إدخال البريد الإلكتروني");
                    return View();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    AddSuccessMessage("إذا كان البريد الإلكتروني صحيحاً، فستتلقى رسالة لإعادة تعيين كلمة المرور");
                    return View();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token, email = user.Email }, Request.Scheme);

                // TODO: إرسال البريد الإلكتروني

                AddSuccessMessage("تم إرسال رابط إعادة تعيين كلمة المرور إلى بريدكم الإلكتروني");
                return View();
            }

            [HttpGet]
            public IActionResult ResetPassword(string? token = null, string? email = null)
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                    return BadRequest();

                var model = new ResetPasswordDto { Token = token, Email = email };
                return View(model);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    AddSuccessMessage("تم إعادة تعيين كلمة المرور بنجاح");
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    AddSuccessMessage("تم إعادة تعيين كلمة المرور بنجاح");
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }
    }

