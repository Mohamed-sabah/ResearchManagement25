using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ResearchManagement.Domain.Entities;
using System.Security.Claims;

namespace ResearchManagement.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly UserManager<User> _userManager;

        protected BaseController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        protected async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        protected void AddSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void AddErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void AddWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        protected void AddInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }
    }
}
