using MediatR;
using ResearchManagement.Application.DTOs;

namespace ResearchManagement.Application.Queries.Research
{
    public class GetResearchByIdQuery : IRequest<ResearchDto?>
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        public GetResearchByIdQuery(int id, string? userId = null)
        {
            Id = id;
            UserId = userId;
        }
    }
}