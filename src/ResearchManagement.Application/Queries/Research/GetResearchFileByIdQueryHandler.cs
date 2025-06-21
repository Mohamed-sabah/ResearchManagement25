using AutoMapper;
using MediatR;
using ResearchManagement.Application.DTOs;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.Research
{
	public class GetResearchFileByIdQueryHandler
		   : IRequestHandler<GetResearchFileByIdQuery, ResearchFileDto?>
	{
		private readonly IGenericRepository<ResearchFile> _fileRepo;

		private readonly IMapper _mapper;

		public GetResearchFileByIdQueryHandler(
				 IGenericRepository<ResearchFile> fileRepo,
				 IMapper mapper)
		{
			_fileRepo = fileRepo;
			_mapper = mapper;
		}

		public async Task<ResearchFileDto?> Handle(
			GetResearchFileByIdQuery request,
			CancellationToken cancellationToken)
		{
			var file = await _fileRepo.GetByIdAsync(request.FileId);

			if (file == null) return null;

			// optional: check request.UserId for authorization here

			return _mapper.Map<ResearchFileDto>(file);
		}
	}
}
