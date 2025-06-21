using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;

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
	public class GetResearchFileByIdQuery : IRequest<ResearchFileDto?>
	{
		public int FileId { get; }
		public string? UserId { get; }

		public GetResearchFileByIdQuery(int fileId, string? userId = null)
		{
			FileId = fileId;
			UserId = userId;
		}
	}


}