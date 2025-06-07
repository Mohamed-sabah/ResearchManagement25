using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.DTOs
{
    public class ResearchAuthorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FirstNameEn { get; set; }
        public string? LastNameEn { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public string? OrcidId { get; set; }
        public int Order { get; set; }
        public bool IsCorresponding { get; set; }
        public string? UserId { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? FullNameEn => !string.IsNullOrEmpty(FirstNameEn) && !string.IsNullOrEmpty(LastNameEn)
            ? $"{FirstNameEn} {LastNameEn}" : null;
    }

    public class CreateResearchAuthorDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FirstNameEn { get; set; }
        public string? LastNameEn { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Institution { get; set; }
        public string? AcademicDegree { get; set; }
        public string? OrcidId { get; set; }
        public int Order { get; set; } = 1;
        public bool IsCorresponding { get; set; } = false;
    }

    public class UpdateResearchAuthorDto : CreateResearchAuthorDto
    {
        public int Id { get; set; }
    }
}
