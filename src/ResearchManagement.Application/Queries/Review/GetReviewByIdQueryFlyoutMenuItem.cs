using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchManagement.Application.Queries.Review
{
    public class GetReviewByIdQueryFlyoutMenuItem
    {
        public GetReviewByIdQueryFlyoutMenuItem()
        {
            TargetType = typeof(GetReviewByIdQueryFlyoutMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}