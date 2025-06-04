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
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        protected string GetCurrentUserId()
        {
            try
            {
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userId ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        protected async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void AddSuccessMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = message;
            }
        }

        protected void AddErrorMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;
            }
        }

        protected void AddWarningMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["WarningMessage"] = message;
            }
        }

        protected void AddInfoMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["InfoMessage"] = message;
            }
        }
    }
}