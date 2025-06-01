using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchManagement.Domain.Enums;

namespace ResearchManagement.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FirstNameEn { get; set; }
        public string? LastNameEn { get; set; }
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public string? OrcidId { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? FullNameEn => !string.IsNullOrEmpty(FirstNameEn) && !string.IsNullOrEmpty(LastNameEn)
            ? $"{FirstNameEn} {LastNameEn}" : null;
    }

    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FirstNameEn { get; set; }
        public string? LastNameEn { get; set; }
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public string? OrcidId { get; set; }
        public UserRole Role { get; set; } = UserRole.Researcher;
    }

    public class UpdateUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FirstNameEn { get; set; }
        public string? LastNameEn { get; set; }
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public string? OrcidId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
