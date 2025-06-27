


//using System.ComponentModel.DataAnnotations;
//using ResearchManagement.Domain.Entities;
//using ResearchManagement.Domain.Enums;
//using ResearchManagement.Web.Controllers;

//namespace ResearchManagement.Web.Models.ViewModels.TrackManagement
//{
//    // عرض البحوث في المسار
//    public class TrackResearchesViewModel
//    {
//        public List<Domain.Entities.Research> Researches { get; set; } = new();
//        public string TrackName { get; set; } = string.Empty;
//        public int TrackId { get; set; }

//        // Pagination
//        public int CurrentPage { get; set; }
//        public int PageSize { get; set; }
//        public int TotalCount { get; set; }
//        public int TotalPages { get; set; }

//        // Filters
//        public ResearchStatus? StatusFilter { get; set; }
//        public string SearchQuery { get; set; } = string.Empty;
//        public ResearchTrack? TrackFilter { get; set; }
//        public ResearchType? TypeFilter { get; set; }
//        public DateTime? SubmissionDateFrom { get; set; }
//        public DateTime? SubmissionDateTo { get; set; }

//        // Statistics
//        public int TotalResearches { get; set; }
//        public int PendingReviews { get; set; }
//        public int UnderReview { get; set; }
//        public int AcceptedResearches { get; set; }
//        public int RejectedResearches { get; set; }
//        public int SubmittedResearches { get; set; }
//        public int WithdrawnResearches { get; set; }

//        // Advanced Statistics
//        public double AcceptanceRate => TotalResearches > 0 ? (double)AcceptedResearches / TotalResearches * 100 : 0;
//        public double RejectionRate => TotalResearches > 0 ? (double)RejectedResearches / TotalResearches * 100 : 0;
//        public double PendingRate => TotalResearches > 0 ? (double)PendingReviews / TotalResearches * 100 : 0;

//        // Helper Properties
//        public bool HasPreviousPage => CurrentPage > 1;
//        public bool HasNextPage => CurrentPage < TotalPages;
//        public int StartRecord => TotalCount > 0 ? (CurrentPage - 1) * PageSize + 1 : 0;
//        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalCount);

//        // Filter Helper Properties
//        public bool HasActiveFilters => StatusFilter.HasValue ||
//                                       !string.IsNullOrEmpty(SearchQuery) ||
//                                       TrackFilter.HasValue ||
//                                       TypeFilter.HasValue ||
//                                       SubmissionDateFrom.HasValue ||
//                                       SubmissionDateTo.HasValue;

//        // Research Analysis
//        public List<ResearchSummaryItem> ResearchSummaries => Researches.Select(r => new ResearchSummaryItem
//        {
//            Id = r.Id,
//            Title = r.Title,
//            TitleEn = r.TitleEn,
//            Status = r.Status,
//            Track = r.Track,
//            ResearchType = r.ResearchType,
//            SubmissionDate = r.SubmissionDate,
//            SubmittedByName = $"{r.SubmittedBy?.FirstName} {r.SubmittedBy?.LastName}",
//            AuthorsCount = r.Authors?.Count(a => !a.IsDeleted) ?? 0,
//            ReviewsCount = r.Reviews?.Count(rev => !rev.IsDeleted) ?? 0,
//            CompletedReviewsCount = r.Reviews?.Count(rev => !rev.IsDeleted && rev.IsCompleted) ?? 0,
//            FilesCount = r.Files?.Count(f => !f.IsDeleted && f.IsActive) ?? 0,
//            AverageScore = r.Reviews?.Where(rev => !rev.IsDeleted && rev.IsCompleted)
//                                   .Select(rev => rev.OverallScore)
//                                   .DefaultIfEmpty(0)
//                                   .Average() ?? 0,
//            HasOverdueReviews = r.Reviews?.Any(rev => !rev.IsDeleted && !rev.IsCompleted && rev.Deadline < DateTime.UtcNow) ?? false,
//            DaysFromSubmission = (DateTime.UtcNow - r.SubmissionDate).Days,
//            RequiresAttention = GetRequiresAttentionStatus(r)
//        }).ToList();

//        // Helper method to determine if research requires attention
//        private bool GetRequiresAttentionStatus(Domain.Entities.Research research)
//        {
//            if (research.Status == ResearchStatus.Submitted &&
//                (DateTime.UtcNow - research.SubmissionDate).Days > 7)
//                return true;

//            if (research.Reviews?.Any(r => !r.IsDeleted && !r.IsCompleted && r.Deadline < DateTime.UtcNow) == true)
//                return true;

//            if (research.Status == ResearchStatus.RequiresMinorRevisions ||
//                research.Status == ResearchStatus.RequiresMajorRevisions)
//                return true;

//            return false;
//        }
//    }

//    // نموذج ملخص البحث
//    public class ResearchSummaryItem
//    {
//        public int Id { get; set; }
//        public string Title { get; set; } = string.Empty;
//        public string? TitleEn { get; set; }
//        public ResearchStatus Status { get; set; }
//        public ResearchTrack Track { get; set; }
//        public ResearchType ResearchType { get; set; }
//        public DateTime SubmissionDate { get; set; }
//        public string SubmittedByName { get; set; } = string.Empty;
//        public int AuthorsCount { get; set; }
//        public int ReviewsCount { get; set; }
//        public int CompletedReviewsCount { get; set; }
//        public int FilesCount { get; set; }
//        public double AverageScore { get; set; }
//        public bool HasOverdueReviews { get; set; }
//        public int DaysFromSubmission { get; set; }
//        public bool RequiresAttention { get; set; }

//        // Display Properties
//        public string StatusDisplayName => Status switch
//        {
//            ResearchStatus.Submitted => "مقدم",
//            ResearchStatus.UnderInitialReview => "قيد المراجعة الأولية",
//            ResearchStatus.AssignedForReview => "موزع للتقييم",
//            ResearchStatus.UnderReview => "قيد التقييم",
//            ResearchStatus.UnderEvaluation => "تحت المراجعة",
//            ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
//            ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات جوهرية",
//            ResearchStatus.RevisionsSubmitted => "تعديلات مقدمة",
//            ResearchStatus.RevisionsUnderReview => "مراجعة التعديلات",
//            ResearchStatus.Accepted => "مقبول",
//            ResearchStatus.Rejected => "مرفوض",
//            ResearchStatus.Withdrawn => "منسحب",
//            ResearchStatus.AwaitingFourthReviewer => "بانتظار المقيم الرابع",
//            _ => status.ToString()
//        };

//        public string StatusBadgeClass => Status switch
//        {
//            ResearchStatus.Submitted => "bg-primary",
//            ResearchStatus.UnderInitialReview => "bg-info",
//            ResearchStatus.AssignedForReview => "bg-info",
//            ResearchStatus.UnderReview => "bg-warning",
//            ResearchStatus.UnderEvaluation => "bg-warning",
//            ResearchStatus.RequiresMinorRevisions => "bg-secondary",
//            ResearchStatus.RequiresMajorRevisions => "bg-warning",
//            ResearchStatus.RevisionsSubmitted => "bg-info",
//            ResearchStatus.RevisionsUnderReview => "bg-warning",
//            ResearchStatus.Accepted => "bg-success",
//            ResearchStatus.Rejected => "bg-danger",
//            ResearchStatus.Withdrawn => "bg-dark",
//            ResearchStatus.AwaitingFourthReviewer => "bg-warning",
//            _ => "bg-secondary"
//        };

//        public string ResearchTypeDisplayName => ResearchType switch
//        {
//            ResearchType.OriginalResearch => "بحث أصلي",
//            ResearchType.SystematicReview => "مراجعة منهجية",
//            ResearchType.CaseStudy => "دراسة حالة",
//            ResearchType.ExperimentalStudy => "دراسة تجريبية",
//            ResearchType.TheoreticalStudy => "دراسة نظرية",
//            ResearchType.AppliedResearch => "بحث تطبيقي",
//            //ResearchType.ExperimentalResearch => "بحث تجريبي",
//            //ResearchType.TheoreticalResearch => "بحث نظري",
//            //ResearchType.SurveyResearch => "بحث استقصائي",
//            //ResearchType.ReviewPaper => "ورقة مراجعة",
//            _ => ResearchType.ToString()
//        };

//        public string TrackDisplayName => Track switch
//        {
//            ResearchTrack.InformationTechnology => "تقنية المعلومات",
//            ResearchTrack.InformationSecurity => "أمن المعلومات",
//            ResearchTrack.ArtificialIntelligence => "الذكاء الاصطناعي",
//            ResearchTrack.DataScience => "علوم البيانات",
//            ResearchTrack.SoftwareEngineering => "هندسة البرمجيات",
//            ResearchTrack.NetworkingAndCommunications => "الشبكات والاتصالات",
//            ResearchTrack.CloudComputing => "الحوسبة السحابية",
//            ResearchTrack.InternetOfThings => "إنترنت الأشياء",
//            ResearchTrack.ARAndVR => "الواقع المعزز والافتراضي",
//            ResearchTrack.Blockchain => "البلوك تشين",
//            ResearchTrack.MachineLearning => "التعلم الآلي",
//            ResearchTrack.NaturalLanguageProcessing => "معالجة اللغات الطبيعية",
//            ResearchTrack.HighPerformanceComputing => "الحوسبة عالية الأداء",
//            ResearchTrack.MobileAppDevelopment => "تطوير التطبيقات المحمولة",
//            ResearchTrack.DatabaseSystems => "قواعد البيانات",
//            _ => Track.ToString()
//        };

//        public string ProgressStatus
//        {
//            get
//            {
//                if (ReviewsCount == 0) return "لم تبدأ المراجعة";
//                if (CompletedReviewsCount == 0) return "في انتظار المراجعة";
//                if (CompletedReviewsCount < ReviewsCount) return $"{CompletedReviewsCount}/{ReviewsCount} مراجعة";
//                return "المراجعة مكتملة";
//            }
//        }

//        public int ProgressPercentage
//        {
//            get
//            {
//                if (ReviewsCount == 0) return 0;
//                return (int)((double)CompletedReviewsCount / ReviewsCount * 100);
//            }
//        }

//        public string UrgencyLevel
//        {
//            get
//            {
//                if (RequiresAttention) return "عاجل";
//                if (HasOverdueReviews) return "متأخر";
//                if (DaysFromSubmission > 30) return "قديم";
//                if (DaysFromSubmission > 14) return "متوسط";
//                return "حديث";
//            }
//        }

//        public string UrgencyBadgeClass => UrgencyLevel switch
//        {
//            "عاجل" => "bg-danger",
//            "متأخر" => "bg-warning",
//            "قديم" => "bg-secondary",
//            "متوسط" => "bg-info",
//            "حديث" => "bg-success",
//            _ => "bg-light"
//        };

//        public bool CanAssignReviewers => Status == ResearchStatus.Submitted ||
//                                        Status == ResearchStatus.UnderInitialReview ||
//                                        Status == ResearchStatus.AssignedForReview;

//        public bool CanMakeFinalDecision => ReviewsCount > 0 &&
//                                          CompletedReviewsCount == ReviewsCount &&
//                                          (Status == ResearchStatus.UnderReview ||
//                                           Status == ResearchStatus.UnderEvaluation);

//        public bool NeedsMoreReviewers => ReviewsCount < 2 &&
//                                        (Status == ResearchStatus.Submitted ||
//                                         Status == ResearchStatus.AssignedForReview);
//    }

//    // تفاصيل البحث من منظور مدير المسار
//    public class ResearchDetailsForTrackManagerViewModel
//    {
//        public Domain.Entities.Research Research { get; set; } = null!;
//        public TrackManager TrackManager { get; set; } = null!;
//        public List<User> AvailableReviewers { get; set; } = new();

//        public bool CanAssignReviewers { get; set; }
//        public bool CanMakeFinalDecision { get; set; }
//        public int RequiredReviewersCount { get; set; }
//        public int AssignedReviewersCount { get; set; }

//        // Research Analysis
//        public ResearchAnalysisViewModel Analysis { get; set; } = new();

//        // Review Statistics
//        public List<ReviewSummaryDto> ReviewSummaries { get; set; } = new();
//        public ReviewStatistics ReviewStats { get; set; } = new();

//        // File Management
//        public List<ResearchFileDto> ActiveFiles { get; set; } = new();
//        public List<ResearchFileDto> AllFiles { get; set; } = new();

//        // Author Information
//        public List<ResearchAuthorDto> Authors { get; set; } = new();
//        public ResearchAuthorDto? CorrespondingAuthor => Authors?.FirstOrDefault(a => a.IsCorresponding);

//        // Status History
//        public List<ResearchStatusHistoryItem> StatusHistory { get; set; } = new();

//        // Helper Properties
//        public bool IsFullyReviewed => AssignedReviewersCount >= RequiredReviewersCount;
//        public bool AllReviewsCompleted => Research.Reviews?.All(r => r.IsCompleted || r.IsDeleted) ?? false;
//        public double AverageScore => Research.Reviews?.Where(r => r.IsCompleted && !r.IsDeleted)
//                                                     .Select(r => r.OverallScore)
//                                                     .DefaultIfEmpty(0)
//                                                     .Average() ?? 0;

//        public string ResearchAge
//        {
//            get
//            {
//                var days = (DateTime.UtcNow - Research.SubmissionDate).Days;
//                if (days < 1) return "اليوم";
//                if (days == 1) return "أمس";
//                if (days < 7) return $"منذ {days} أيام";
//                if (days < 30) return $"منذ {days / 7} أسابيع";
//                if (days < 365) return $"منذ {days / 30} أشهر";
//                return $"منذ {days / 365} سنوات";
//            }
//        }

//        public List<RequiredAction> RequiredActions { get; set; } = new();

//        public bool HasUrgentActions => RequiredActions.Any(a => a.IsUrgent);
//        public int UrgentActionsCount => RequiredActions.Count(a => a.IsUrgent);

//        // Recommendation based on reviews
//        public FinalDecisionRecommendation GetRecommendation()
//        {
//            if (!AllReviewsCompleted || !Research.Reviews.Any())
//                return new FinalDecisionRecommendation { Decision = ResearchStatus.UnderReview, Confidence = 0 };

//            var reviews = Research.Reviews.Where(r => !r.IsDeleted && r.IsCompleted).ToList();
//            var avgScore = reviews.Average(r => r.OverallScore);
//            var decisions = reviews.Select(r => r.Decision).ToList();

//            var acceptCount = decisions.Count(d => d == ReviewDecision.AcceptAsIs || d == ReviewDecision.AcceptWithMinorRevisions);
//            var rejectCount = decisions.Count(d => d == ReviewDecision.Reject || d == ReviewDecision.NotSuitableForConference);
//            var revisionCount = decisions.Count(d => d == ReviewDecision.MajorRevisionsRequired);

//            ResearchStatus recommendedDecision;
//            int confidence;

//            if (acceptCount > rejectCount && acceptCount > revisionCount)
//            {
//                recommendedDecision = decisions.Any(d => d == ReviewDecision.AcceptWithMinorRevisions)
//                    ? ResearchStatus.RequiresMinorRevisions
//                    : ResearchStatus.Accepted;
//                confidence = (int)((double)acceptCount / reviews.Count * 100);
//            }
//            else if (rejectCount > acceptCount && rejectCount > revisionCount)
//            {
//                recommendedDecision = ResearchStatus.Rejected;
//                confidence = (int)((double)rejectCount / reviews.Count * 100);
//            }
//            else if (revisionCount > 0)
//            {
//                recommendedDecision = ResearchStatus.RequiresMajorRevisions;
//                confidence = (int)((double)revisionCount / reviews.Count * 100);
//            }
//            else
//            {
//                recommendedDecision = avgScore >= 7 ? ResearchStatus.Accepted : ResearchStatus.Rejected;
//                confidence = 50; // متوسط عند التضارب
//            }

//            return new FinalDecisionRecommendation
//            {
//                Decision = recommendedDecision,
//                Confidence = confidence,
//                AverageScore = avgScore,
//                ReviewCount = reviews.Count,
//                AcceptanceCount = acceptCount,
//                RejectionCount = rejectCount,
//                RevisionCount = revisionCount,
//                Notes = GenerateRecommendationNotes(avgScore, decisions)
//            };
//        }

//        private string GenerateRecommendationNotes(double avgScore, List<ReviewDecision> decisions)
//        {
//            var notes = new List<string>();

//            if (avgScore >= 8.5)
//                notes.Add("متوسط تقييم ممتاز");
//            else if (avgScore >= 7)
//                notes.Add("متوسط تقييم جيد");
//            else if (avgScore >= 5)
//                notes.Add("متوسط تقييم مقبول");
//            else
//                notes.Add("متوسط تقييم ضعيف");

//            var uniqueDecisions = decisions.Distinct().Count();
//            if (uniqueDecisions == 1)
//                notes.Add("إجماع المراجعين");
//            else if (uniqueDecisions == decisions.Count)
//                notes.Add("تضارب في آراء المراجعين");
//            else
//                notes.Add("اتفاق جزئي بين المراجعين");

//            return string.Join("، ", notes);
//        }
//    }

//    // نموذج تحليل البحث
//    public class ResearchAnalysisViewModel
//    {
//        public int WordCount { get; set; }
//        public int PagesEstimate { get; set; }
//        public int KeywordsCount { get; set; }
//        public int AuthorsCount { get; set; }
//        public int FilesCount { get; set; }
//        public bool HasEnglishContent { get; set; }
//        public bool IsMultilingual { get; set; }
//        public ResearchComplexity Complexity { get; set; }
//        public List<string> DetectedKeywords { get; set; } = new();
//        public double ReadabilityScore { get; set; }
//    }

//    public enum ResearchComplexity
//    {
//        Simple,
//        Moderate,
//        Complex,
//        VeryComplex
//    }

//    // إحصائيات المراجعة
//    public class ReviewStatistics
//    {
//        public int TotalReviews { get; set; }
//        public int CompletedReviews { get; set; }
//        public int PendingReviews { get; set; }
//        public int OverdueReviews { get; set; }
//        public double AverageScore { get; set; }
//        public double AverageCompletionTime { get; set; } // بالأيام
//        public ReviewDecision? MajorityDecision { get; set; }
//        public double ConsistencyScore { get; set; } // مدى اتساق المراجعات
//        public List<int> ScoreDistribution { get; set; } = new();
//    }

//    // عنصر تاريخ الحالة
//    public class ResearchStatusHistoryItem
//    {
//        public int Id { get; set; }
//        public ResearchStatus FromStatus { get; set; }
//        public ResearchStatus ToStatus { get; set; }
//        public DateTime ChangedAt { get; set; }
//        public string ChangedByName { get; set; } = string.Empty;
//        public string? Notes { get; set; }
//        public string Icon { get; set; } = string.Empty;
//        public string Color { get; set; } = string.Empty;

//        public string FromStatusDisplayName => GetStatusDisplayName(FromStatus);
//        public string ToStatusDisplayName => GetStatusDisplayName(ToStatus);

//        private string GetStatusDisplayName(ResearchStatus status)
//        {
//            return status switch
//            {
//                ResearchStatus.Submitted => "مقدم",
//                ResearchStatus.UnderInitialReview => "قيد المراجعة الأولية",
//                ResearchStatus.AssignedForReview => "موزع للتقييم",
//                ResearchStatus.UnderReview => "قيد التقييم",
//                ResearchStatus.UnderEvaluation => "تحت المراجعة",
//                ResearchStatus.RequiresMinorRevisions => "يتطلب تعديلات طفيفة",
//                ResearchStatus.RequiresMajorRevisions => "يتطلب تعديلات جوهرية",
//                ResearchStatus.RevisionsSubmitted => "تعديلات مقدمة",
//                ResearchStatus.RevisionsUnderReview => "مراجعة التعديلات",
//                ResearchStatus.Accepted => "مقبول",
//                ResearchStatus.Rejected => "مرفوض",
//                ResearchStatus.Withdrawn => "منسحب",
//                ResearchStatus.AwaitingFourthReviewer => "بانتظار المقيم الرابع",
//                _ => status.ToString()
//            };
//        }
//    }

//    // الإجراء المطلوب
//    public class RequiredAction
//    {
//        public string Title { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//        public ActionType Type { get; set; }
//        public bool IsUrgent { get; set; }
//        public DateTime? DueDate { get; set; }
//        public string ActionUrl { get; set; } = string.Empty;
//        public string Icon { get; set; } = string.Empty;
//        public string Color { get; set; } = string.Empty;

//        public int DaysRemaining
//        {
//            get
//            {
//                if (!DueDate.HasValue) return int.MaxValue;
//                return Math.Max(0, (DueDate.Value - DateTime.UtcNow).Days);
//            }
//        }

//        public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value;
//    }

//    public enum ActionType
//    {
//        AssignReviewer,
//        FollowUpReview,
//        MakeFinalDecision,
//        RequestRevisions,
//        ContactAuthor,
//        ExtendDeadline,
//        UploadDocument,
//        Other
//    }

//    // توصية القرار النهائي
//    public class FinalDecisionRecommendation
//    {
//        public ResearchStatus Decision { get; set; }
//        public int Confidence { get; set; } // نسبة الثقة من 0 إلى 100
//        public double AverageScore { get; set; }
//        public int ReviewCount { get; set; }
//        public int AcceptanceCount { get; set; }
//        public int RejectionCount { get; set; }
//        public int RevisionCount { get; set; }
//        public string Notes { get; set; } = string.Empty;

//        public string ConfidenceLevel => Confidence switch
//        {
//            >= 90 => "عالية جداً",
//            >= 75 => "عالية",
//            >= 60 => "متوسطة",
//            >= 40 => "منخفضة",
//            _ => "منخفضة جداً"
//        };

//        public string ConfidenceColor => Confidence switch
//        {
//            >= 90 => "success",
//            >= 75 => "info",
//            >= 60 => "warning",
//            >= 40 => "warning",
//            _ => "danger"
//        };

//        public string DecisionDisplayName => Decision switch
//        {
//            ResearchStatus.Accepted => "قبول البحث",
//            ResearchStatus.Rejected => "رفض البحث",
//            ResearchStatus.RequiresMinorRevisions => "تعديلات طفيفة مطلوبة",
//            ResearchStatus.RequiresMajorRevisions => "تعديلات جوهرية مطلوبة",
//            _ => "قرار غير محدد"
//        };
//    }

//    // تعيين مراجع
//    public class AssignReviewerViewModel
//    {
//        [Required(ErrorMessage = "يجب تحديد البحث")]
//        public int ResearchId { get; set; }

//        [Required(ErrorMessage = "يجب اختيار مراجع")]
//        public string ReviewerId { get; set; } = string.Empty;

//        [DataType(DataType.Date)]
//        [Display(Name = "الموعد النهائي")]
//        public DateTime? Deadline { get; set; }

//        [Display(Name = "تعليمات خاصة")]
//        [StringLength(1000, ErrorMessage = "التعليمات الخاصة لا يجب أن تتجاوز 1000 حرف")]
//        public string? SpecialInstructions { get; set; }
//    }

//    // اتخاذ القرار النهائي
//    public class FinalDecisionViewModel
//    {
//        [Required(ErrorMessage = "يجب تحديد البحث")]
//        public int ResearchId { get; set; }

//        [Required(ErrorMessage = "يجب اختيار القرار النهائي")]
//        [Display(Name = "القرار النهائي")]
//        public ResearchStatus FinalDecision { get; set; }

//        [Required(ErrorMessage = "يجب إضافة تعليقات")]
//        [Display(Name = "تعليقات مدير المسار")]
//        [StringLength(2000, ErrorMessage = "التعليقات لا يجب أن تتجاوز 2000 حرف")]
//        public string Comments { get; set; } = string.Empty;

//        [Display(Name = "سبب الرفض")]
//        [StringLength(1000, ErrorMessage = "سبب الرفض لا يجب أن يتجاوز 1000 حرف")]
//        public string? RejectionReason { get; set; }

//        // Helper Properties
//        public bool IsRejection => FinalDecision == ResearchStatus.Rejected;
//    }

//    // إدارة المراجعين
//    public class ManageReviewersViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public List<TrackReviewer> TrackReviewers { get; set; } = new();
//        public List<User> AvailableReviewers { get; set; } = new();
//        public string TrackName { get; set; } = string.Empty;

//        // Statistics
//        public int TotalReviewers => TrackReviewers.Count;
//        public int ActiveReviewers => TrackReviewers.Count(tr => tr.IsActive);
//    }

//    // جدولة المراجعات
//    public class ReviewScheduleViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<Review> PendingReviews { get; set; } = new();
//        public List<Review> CompletedReviews { get; set; } = new();
//        public List<Review> OverdueReviews { get; set; } = new();

//        // Statistics
//        public int TotalPendingReviews => PendingReviews.Count;
//        public int TotalCompletedReviews => CompletedReviews.Count;
//        public int TotalOverdueReviews => OverdueReviews.Count;

//        // Upcoming Deadlines
//        public List<Review> UpcomingDeadlines => PendingReviews
//            .Where(r => r.Deadline <= DateTime.UtcNow.AddDays(7))
//            .OrderBy(r => r.Deadline)
//            .ToList();
//    }

//    // تقارير المسار
//    public class TrackReportsViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;

//        // Research Statistics
//        public int TotalResearches { get; set; }
//        public int AcceptedResearches { get; set; }
//        public int RejectedResearches { get; set; }
//        public int PendingResearches { get; set; }

//        // Review Statistics
//        public int TotalReviews { get; set; }
//        public int CompletedReviews { get; set; }
//        public int PendingReviews { get; set; }
//        public int OverdueReviews { get; set; }
//        public double AverageReviewTime { get; set; }

//        // Charts Data
//        public List<MonthlySubmissionData> MonthlySubmissions { get; set; } = new();
//        public List<ReviewerPerformanceData> ReviewerPerformance { get; set; } = new();

//        // Calculated Properties
//        public double AcceptanceRate => TotalResearches > 0 ?
//            (double)AcceptedResearches / TotalResearches * 100 : 0;
//        public double RejectionRate => TotalResearches > 0 ?
//            (double)RejectedResearches / TotalResearches * 100 : 0;
//        public double CompletionRate => TotalReviews > 0 ?
//            (double)CompletedReviews / TotalReviews * 100 : 0;
//    }

//    // التعيين الجماعي للمراجعين
//    public class BulkAssignReviewersViewModel
//    {
//        public List<BulkAssignmentItem> Assignments { get; set; } = new();
//    }

//    public class BulkAssignmentItem
//    {
//        public int ResearchId { get; set; }
//        public List<string> ReviewerIds { get; set; } = new();
//    }

//    // عرض المراجعات المتأخرة
//    public class OverdueReviewsViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<Review> OverdueReviews { get; set; } = new();

//        // Grouping
//        public Dictionary<string, List<Review>> ReviewsByReviewer =>
//            OverdueReviews.GroupBy(r => $"{r.Reviewer.FirstName} {r.Reviewer.LastName}")
//                         .ToDictionary(g => g.Key, g => g.ToList());

//        public Dictionary<TimeSpan, List<Review>> ReviewsByOverdueDuration =>
//            OverdueReviews.GroupBy(r => GetOverdueDuration(r.Deadline))
//                         .ToDictionary(g => g.Key, g => g.ToList());

//        private TimeSpan GetOverdueDuration(DateTime deadline)
//        {
//            var overdueDays = (DateTime.UtcNow - deadline).Days;
//            if (overdueDays <= 7) return TimeSpan.FromDays(7);
//            if (overdueDays <= 14) return TimeSpan.FromDays(14);
//            if (overdueDays <= 30) return TimeSpan.FromDays(30);
//            return TimeSpan.FromDays(365); // More than 30 days
//        }
//    }

//    // إحصائيات مراجع معين
//    public class ReviewerStatsViewModel
//    {
//        public User Reviewer { get; set; } = null!;
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<Review> Reviews { get; set; } = new();

//        // Statistics
//        public int TotalAssigned => Reviews.Count;
//        public int TotalCompleted => Reviews.Count(r => r.IsCompleted);
//        public int TotalPending => Reviews.Count(r => !r.IsCompleted);
//        public int TotalOverdue => Reviews.Count(r => !r.IsCompleted && r.Deadline < DateTime.UtcNow);

//        public double CompletionRate => TotalAssigned > 0 ?
//            (double)TotalCompleted / TotalAssigned * 100 : 0;

//        public double AverageCompletionTime => Reviews.Where(r => r.IsCompleted && r.CompletedDate.HasValue)
//                                                     .Select(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
//                                                     .DefaultIfEmpty(0)
//                                                     .Average();

//        public double AverageScore => Reviews.Where(r => r.IsCompleted)
//                                           .Select(r => r.OverallScore)
//                                           .DefaultIfEmpty(0)
//                                           .Average();

//        // Performance Rating
//        public string PerformanceRating
//        {
//            get
//            {
//                if (CompletionRate >= 90 && AverageCompletionTime <= 14) return "ممتاز";
//                if (CompletionRate >= 80 && AverageCompletionTime <= 18) return "جيد جداً";
//                if (CompletionRate >= 70 && AverageCompletionTime <= 21) return "جيد";
//                if (CompletionRate >= 60) return "مقبول";
//                return "ضعيف";
//            }
//        }

//        public string PerformanceColor => PerformanceRating switch
//        {
//            "ممتاز" => "success",
//            "جيد جداً" => "primary",
//            "جيد" => "info",
//            "مقبول" => "warning",
//            _ => "danger"
//        };
//    }

//    // نموذج البحث والفلترة
//    public class TrackSearchFiltersViewModel
//    {
//        [Display(Name = "البحث في العنوان أو الملخص")]
//        public string? SearchQuery { get; set; }

//        [Display(Name = "حالة البحث")]
//        public ResearchStatus? Status { get; set; }

//        [Display(Name = "تاريخ التقديم من")]
//        [DataType(DataType.Date)]
//        public DateTime? SubmissionDateFrom { get; set; }

//        [Display(Name = "تاريخ التقديم إلى")]
//        [DataType(DataType.Date)]
//        public DateTime? SubmissionDateTo { get; set; }

//        [Display(Name = "نوع البحث")]
//        public ResearchType? ResearchType { get; set; }

//        [Display(Name = "لغة البحث")]
//        public ResearchLanguage? Language { get; set; }

//        [Display(Name = "المراجع")]
//        public string? ReviewerId { get; set; }

//        [Display(Name = "حالة المراجعة")]
//        public ReviewStatus? ReviewStatus { get; set; }

//        // Helper Properties
//        public bool HasFilters => !string.IsNullOrEmpty(SearchQuery) ||
//                                Status.HasValue ||
//                                SubmissionDateFrom.HasValue ||
//                                SubmissionDateTo.HasValue ||
//                                ResearchType.HasValue ||
//                                Language.HasValue ||
//                                !string.IsNullOrEmpty(ReviewerId) ||
//                                ReviewStatus.HasValue;
//    }

//    // تصدير البيانات
//    public class ExportDataViewModel
//    {
//        [Display(Name = "نوع التصدير")]
//        public ExportType ExportType { get; set; }

//        [Display(Name = "تنسيق الملف")]
//        public ExportFormat ExportFormat { get; set; }

//        [Display(Name = "فترة البيانات")]
//        public DateRange DateRange { get; set; }

//        [Display(Name = "تاريخ البداية")]
//        [DataType(DataType.Date)]
//        public DateTime? StartDate { get; set; }

//        [Display(Name = "تاريخ النهاية")]
//        [DataType(DataType.Date)]
//        public DateTime? EndDate { get; set; }

//        [Display(Name = "تضمين التفاصيل")]
//        public bool IncludeDetails { get; set; }

//        [Display(Name = "تضمين المراجعات")]
//        public bool IncludeReviews { get; set; }
//    }

//    public enum ExportType
//    {
//        [Display(Name = "البحوث")]
//        Researches = 1,
//        [Display(Name = "المراجعات")]
//        Reviews = 2,
//        [Display(Name = "الإحصائيات")]
//        Statistics = 3,
//        [Display(Name = "تقرير شامل")]
//        FullReport = 4
//    }

//    public enum ExportFormat
//    {
//        [Display(Name = "Excel")]
//        Excel = 1,
//        [Display(Name = "PDF")]
//        PDF = 2,
//        [Display(Name = "CSV")]
//        CSV = 3
//    }

//    public enum DateRange
//    {
//        [Display(Name = "الشهر الحالي")]
//        CurrentMonth = 1,
//        [Display(Name = "الشهر الماضي")]
//        LastMonth = 2,
//        [Display(Name = "الربع الحالي")]
//        CurrentQuarter = 3,
//        [Display(Name = "السنة الحالية")]
//        CurrentYear = 4,
//        [Display(Name = "فترة مخصصة")]
//        Custom = 5
//    }

//    public enum ReviewStatus
//    {
//        [Display(Name = "جميع المراجعات")]
//        All = 0,
//        [Display(Name = "معلقة")]
//        Pending = 1,
//        [Display(Name = "مكتملة")]
//        Completed = 2,
//        [Display(Name = "متأخرة")]
//        Overdue = 3
//    }

//    // نموذج إرسال التذكيرات
//    public class SendReminderViewModel
//    {
//        [Required(ErrorMessage = "يجب تحديد المراجعة")]
//        public int ReviewId { get; set; }

//        [Required(ErrorMessage = "يجب كتابة رسالة التذكير")]
//        [Display(Name = "رسالة التذكير")]
//        [StringLength(500, ErrorMessage = "رسالة التذكير لا يجب أن تتجاوز 500 حرف")]
//        public string Message { get; set; } = string.Empty;

//        [Display(Name = "نوع التذكير")]
//        public ReminderType ReminderType { get; set; }

//        public Review Review { get; set; } = null!;
//    }

//    public enum ReminderType
//    {
//        [Display(Name = "تذكير عادي")]
//        Normal = 1,
//        [Display(Name = "تذكير عاجل")]
//        Urgent = 2,
//        [Display(Name = "إنذار نهائي")]
//        Final = 3
//    }

//    // نموذج تقييم أداء المراجعين
//    public class ReviewerEvaluationViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<ReviewerPerformanceData> ReviewerPerformances { get; set; } = new();
//        public DateTime EvaluationPeriodStart { get; set; }
//        public DateTime EvaluationPeriodEnd { get; set; }

//        // Filtering Options
//        public PerformanceFilter PerformanceFilter { get; set; }
//        public int MinimumReviews { get; set; } = 5;

//        // Summary Statistics
//        public double AverageCompletionRate => ReviewerPerformances.Any() ?
//            ReviewerPerformances.Average(r => r.CompletionRate) : 0;
//        public double AverageCompletionTime => ReviewerPerformances.Any() ?
//            ReviewerPerformances.Average(r => r.AverageCompletionTime) : 0;

//        // Top Performers
//        public List<ReviewerPerformanceData> TopPerformers => ReviewerPerformances
//            .Where(r => r.TotalAssigned >= MinimumReviews)
//            .OrderByDescending(r => r.CompletionRate)
//            .ThenBy(r => r.AverageCompletionTime)
//            .Take(5)
//            .ToList();

//        // Underperformers
//        public List<ReviewerPerformanceData> Underperformers => ReviewerPerformances
//            .Where(r => r.CompletionRate < 70 || r.TotalOverdue > 0)
//            .OrderBy(r => r.CompletionRate)
//            .ThenByDescending(r => r.TotalOverdue)
//            .ToList();
//    }

//    public enum PerformanceFilter
//    {
//        [Display(Name = "جميع المراجعين")]
//        All = 0,
//        [Display(Name = "أداء ممتاز")]
//        Excellent = 1,
//        [Display(Name = "أداء جيد")]
//        Good = 2,
//        [Display(Name = "يحتاج تحسين")]
//        NeedsImprovement = 3,
//        [Display(Name = "أداء ضعيف")]
//        Poor = 4
//    }

//    // نموذج تحليل جودة المراجعات
//    public class ReviewQualityAnalysisViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<Review> CompletedReviews { get; set; } = new();

//        // Quality Metrics
//        public double AverageReviewLength => CompletedReviews.Any() ?
//            CompletedReviews.Average(r => (r.CommentsToAuthor?.Length ?? 0) + (r.CommentsToTrackManager?.Length ?? 0)) : 0;

//        public double ConsistencyScore => CalculateConsistencyScore();

//        public Dictionary<ReviewDecision, int> DecisionDistribution => CompletedReviews
//            .GroupBy(r => r.Decision)
//            .ToDictionary(g => g.Key, g => g.Count());

//        public Dictionary<string, double> ReviewerConsistency => CompletedReviews
//            .GroupBy(r => r.ReviewerId)
//            .Where(g => g.Count() >= 3)
//            .ToDictionary(
//                g => $"{g.First().Reviewer.FirstName} {g.First().Reviewer.LastName}",
//                g => CalculateReviewerConsistency(g.ToList())
//            );

//        private double CalculateConsistencyScore()
//        {
//            if (!CompletedReviews.Any()) return 0;

//            var researchGroups = CompletedReviews.GroupBy(r => r.ResearchId);
//            var consistencyScores = new List<double>();

//            foreach (var group in researchGroups.Where(g => g.Count() >= 2))
//            {
//                var scores = group.Select(r => r.OverallScore).ToList();
//                var average = scores.Average();
//                var variance = scores.Sum(s => Math.Pow(s - average, 2)) / scores.Count;
//                var consistency = Math.Max(0, 100 - (variance * 10));
//                consistencyScores.Add(consistency);
//            }

//            return consistencyScores.Any() ? consistencyScores.Average() : 0;
//        }

//        private double CalculateReviewerConsistency(List<Review> reviews)
//        {
//            if (reviews.Count < 2) return 0;

//            var scores = reviews.Select(r => r.OverallScore).ToList();
//            var average = scores.Average();
//            var variance = scores.Sum(s => Math.Pow(s - average, 2)) / scores.Count;
//            return Math.Max(0, 100 - (variance * 20));
//        }
//    }

//    // نموذج جدولة التذكيرات التلقائية
//    public class AutoRemindersViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;

//        [Display(Name = "تفعيل التذكيرات التلقائية")]
//        public bool EnableAutoReminders { get; set; }

//        [Display(Name = "أول تذكير (أيام قبل الموعد)")]
//        [Range(1, 30, ErrorMessage = "يجب أن يكون بين 1 و 30 يوم")]
//        public int FirstReminderDays { get; set; } = 7;

//        [Display(Name = "ثاني تذكير (أيام قبل الموعد)")]
//        [Range(1, 15, ErrorMessage = "يجب أن يكون بين 1 و 15 يوم")]
//        public int SecondReminderDays { get; set; } = 3;

//        [Display(Name = "تذكير نهائي (أيام قبل الموعد)")]
//        [Range(0, 5, ErrorMessage = "يجب أن يكون بين 0 و 5 أيام")]
//        public int FinalReminderDays { get; set; } = 1;

//        [Display(Name = "تذكير بعد انتهاء الموعد")]
//        public bool SendOverdueReminders { get; set; }

//        [Display(Name = "فترة التذكير بعد التأخير (أيام)")]
//        [Range(1, 14, ErrorMessage = "يجب أن يكون بين 1 و 14 يوم")]
//        public int OverdueReminderInterval { get; set; } = 3;

//        [Display(Name = "الحد الأقصى للتذكيرات المتأخرة")]
//        [Range(1, 10, ErrorMessage = "يجب أن يكون بين 1 و 10 تذكيرات")]
//        public int MaxOverdueReminders { get; set; } = 3;

//        [Display(Name = "إرسال نسخة لمدير المسار")]
//        public bool CopyTrackManager { get; set; } = true;

//        public List<ScheduledReminder> UpcomingReminders { get; set; } = new();
//    }

//    public class ScheduledReminder
//    {
//        public int ReviewId { get; set; }
//        public string ResearchTitle { get; set; } = string.Empty;
//        public string ReviewerName { get; set; } = string.Empty;
//        public DateTime ScheduledDate { get; set; }
//        public ReminderType Type { get; set; }
//        public bool IsOverdue { get; set; }
//    }

//    // نموذج إعدادات المسار
//    public class TrackSettingsViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;

//        [Display(Name = "وصف المسار")]
//        [StringLength(500, ErrorMessage = "وصف المسار لا يجب أن يتجاوز 500 حرف")]
//        public string? TrackDescription { get; set; }

//        [Display(Name = "عدد المراجعين المطلوب")]
//        [Range(1, 5, ErrorMessage = "عدد المراجعين يجب أن يكون بين 1 و 5")]
//        public int RequiredReviewersCount { get; set; } = 2;

//        [Display(Name = "المدة الافتراضية للمراجعة (أيام)")]
//        [Range(7, 60, ErrorMessage = "المدة يجب أن تكون بين 7 و 60 يوم")]
//        public int DefaultReviewDays { get; set; } = 21;

//        [Display(Name = "السماح بالتمديد التلقائي")]
//        public bool AllowAutoExtension { get; set; }

//        [Display(Name = "مدة التمديد التلقائي (أيام)")]
//        [Range(1, 14, ErrorMessage = "مدة التمديد يجب أن تكون بين 1 و 14 يوم")]
//        public int AutoExtensionDays { get; set; } = 7;

//        [Display(Name = "الحد الأقصى للتمديدات")]
//        [Range(0, 3, ErrorMessage = "الحد الأقصى يجب أن يكون بين 0 و 3")]
//        public int MaxExtensions { get; set; } = 1;

//        [Display(Name = "طلب مراجع رابع عند التضارب")]
//        public bool RequireFourthReviewerOnConflict { get; set; } = true;

//        [Display(Name = "حد نقاط التضارب")]
//        [Range(0.5, 3.0, ErrorMessage = "حد التضارب يجب أن يكون بين 0.5 و 3.0")]
//        public double ConflictThreshold { get; set; } = 1.5;

//        [Display(Name = "إرسال إشعارات بريد إلكتروني")]
//        public bool SendEmailNotifications { get; set; } = true;

//        [Display(Name = "إرسال إشعارات داخلية")]
//        public bool SendInternalNotifications { get; set; } = true;

//        public AutoRemindersViewModel AutoReminders { get; set; } = new();
//    }

//    // إحصائيات مقارنة بين المسارات (للمديرين الإداريين)
//    public class TrackComparisonViewModel
//    {
//        public List<TrackComparisonData> TrackComparisons { get; set; } = new();
//        public DateTime ComparisonPeriodStart { get; set; }
//        public DateTime ComparisonPeriodEnd { get; set; }

//        public TrackComparisonData? BestPerformingTrack => TrackComparisons
//            .OrderByDescending(t => t.OverallScore)
//            .FirstOrDefault();

//        public TrackComparisonData? MostActiveTrack => TrackComparisons
//            .OrderByDescending(t => t.TotalResearches)
//            .FirstOrDefault();
//    }

//    public class TrackComparisonData
//    {
//        public ResearchTrack Track { get; set; }
//        public string TrackName { get; set; } = string.Empty;
//        public string TrackManagerName { get; set; } = string.Empty;

//        public int TotalResearches { get; set; }
//        public int AcceptedResearches { get; set; }
//        public int RejectedResearches { get; set; }
//        public double AcceptanceRate { get; set; }

//        public int TotalReviews { get; set; }
//        public int CompletedReviews { get; set; }
//        public double CompletionRate { get; set; }
//        public double AverageReviewTime { get; set; }

//        public int ActiveReviewers { get; set; }
//        public double ReviewerEfficiency { get; set; }

//        public double OverallScore { get; set; }
//        public string PerformanceGrade { get; set; } = string.Empty;
//        public string PerformanceColor { get; set; } = string.Empty;
//    }



//    // إدارة المراجعين
//    public class ManageReviewersViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public List<TrackReviewer> TrackReviewers { get; set; } = new();
//        public List<User> AvailableReviewers { get; set; } = new();
//        public string TrackName { get; set; } = string.Empty;

//        // Reviewer Performance Data
//        public List<ReviewerPerformanceInfo> ReviewerPerformances { get; set; } = new();

//        // Statistics
//        public int TotalReviewers => TrackReviewers.Count;
//        public int ActiveReviewers => TrackReviewers.Count(tr => tr.IsActive && !tr.IsDeleted);
//        public int InactiveReviewers => TrackReviewers.Count(tr => !tr.IsActive || tr.IsDeleted);

//        // Advanced Statistics
//        public double AverageReviewerLoad => ReviewerPerformances.Any()
//            ? ReviewerPerformances.Average(r => r.CurrentLoad)
//            : 0;

//        public double AverageCompletionRate => ReviewerPerformances.Any()
//            ? ReviewerPerformances.Average(r => r.CompletionRate)
//            : 0;

//        // Helper Methods
//        public List<User> GetTopPerformers(int count = 5)
//        {
//            return ReviewerPerformances
//                .Where(r => r.CompletionRate >= 80 && r.AverageCompletionTime <= 14)
//                .OrderByDescending(r => r.CompletionRate)
//                .ThenBy(r => r.AverageCompletionTime)
//                .Take(count)
//                .Select(r => AvailableReviewers.FirstOrDefault(u => u.Id == r.ReviewerId))
//                .Where(u => u != null)
//                .ToList()!;
//        }

//        public List<User> GetUnderperformers(int count = 5)
//        {
//            return ReviewerPerformances
//                .Where(r => r.CompletionRate < 70 || r.AverageCompletionTime > 21 || r.OverdueCount > 0)
//                .OrderBy(r => r.CompletionRate)
//                .ThenByDescending(r => r.OverdueCount)
//                .Take(count)
//                .Select(r => TrackReviewers.FirstOrDefault(tr => tr.ReviewerId == r.ReviewerId)?.Reviewer)
//                .Where(u => u != null)
//                .ToList()!;
//        }
//    }

//    // معلومات أداء المراجع
//    public class ReviewerPerformanceInfo
//    {
//        public string ReviewerId { get; set; } = string.Empty;
//        public string ReviewerName { get; set; } = string.Empty;
//        public string ReviewerEmail { get; set; } = string.Empty;
//        public string? Institution { get; set; }
//        public string? AcademicDegree { get; set; }

//        // Performance Metrics
//        public int TotalAssigned { get; set; }
//        public int TotalCompleted { get; set; }
//        public int CurrentLoad { get; set; } // المراجعات النشطة
//        public int OverdueCount { get; set; }
//        public double CompletionRate { get; set; }
//        public double AverageCompletionTime { get; set; } // بالأيام
//        public double AverageScore { get; set; }
//        public DateTime? LastReviewDate { get; set; }

//        // Availability
//        public bool IsAvailable => CurrentLoad < 3 && CompletionRate >= 70;
//        public int MaxCapacity { get; set; } = 3;
//        public int AvailableSlots => Math.Max(0, MaxCapacity - CurrentLoad);

//        // Performance Rating
//        public string PerformanceRating
//        {
//            get
//            {
//                if (CompletionRate >= 95 && AverageCompletionTime <= 10 && OverdueCount == 0)
//                    return "ممتاز";
//                if (CompletionRate >= 85 && AverageCompletionTime <= 14 && OverdueCount <= 1)
//                    return "جيد جداً";
//                if (CompletionRate >= 75 && AverageCompletionTime <= 18 && OverdueCount <= 2)
//                    return "جيد";
//                if (CompletionRate >= 60 && AverageCompletionTime <= 21)
//                    return "مقبول";
//                return "ضعيف";
//            }
//        }

//        public string PerformanceColor => PerformanceRating switch
//        {
//            "ممتاز" => "success",
//            "جيد جداً" => "primary",
//            "جيد" => "info",
//            "مقبول" => "warning",
//            _ => "danger"
//        };

//        // Load Status
//        public string LoadStatus
//        {
//            get
//            {
//                var percentage = MaxCapacity > 0 ? (double)CurrentLoad / MaxCapacity * 100 : 0;
//                return percentage switch
//                {
//                    <= 30 => "منخفض",
//                    <= 70 => "متوسط",
//                    <= 90 => "عالي",
//                    _ => "مشغول"
//                };
//            }
//        }

//        public string LoadStatusColor => LoadStatus switch
//        {
//            "منخفض" => "success",
//            "متوسط" => "info",
//            "عالي" => "warning",
//            "مشغول" => "danger",
//            _ => "secondary"
//        };
//    }

//    // جدولة المراجعات
//    public class ReviewScheduleViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;
//        public List<Domain.Entities.Review> PendingReviews { get; set; } = new();
//        public List<Domain.Entities.Review> CompletedReviews { get; set; } = new();
//        public List<Domain.Entities.Review> OverdueReviews { get; set; } = new();

//        // Calendar View Data
//        public List<ReviewCalendarItem> CalendarItems { get; set; } = new();
//        public DateTime CurrentMonth { get; set; } = DateTime.UtcNow;

//        // Statistics
//        public int TotalPendingReviews => PendingReviews.Count;
//        public int TotalCompletedReviews => CompletedReviews.Count;
//        public int TotalOverdueReviews => OverdueReviews.Count;

//        // Upcoming Deadlines
//        public List<Domain.Entities.Review> UpcomingDeadlines => PendingReviews
//            .Where(r => r.Deadline <= DateTime.UtcNow.AddDays(7))
//            .OrderBy(r => r.Deadline)
//            .ToList();

//        // Critical Reviews (overdue or due today)
//        public List<Domain.Entities.Review> CriticalReviews => PendingReviews
//            .Where(r => r.Deadline <= DateTime.UtcNow.AddDays(1))
//            .OrderBy(r => r.Deadline)
//            .ToList();

//        // Workload Distribution
//        public Dictionary<string, int> WorkloadByReviewer => PendingReviews
//            .GroupBy(r => $"{r.Reviewer.FirstName} {r.Reviewer.LastName}")
//            .ToDictionary(g => g.Key, g => g.Count());

//        // Monthly Statistics
//        public List<MonthlyReviewData> MonthlyData { get; set; } = new();

//        // Helper Properties
//        public bool HasCriticalReviews => CriticalReviews.Any();
//        public bool HasUpcomingDeadlines => UpcomingDeadlines.Any();
//        public double AverageReviewTime => CompletedReviews.Where(r => r.CompletedDate.HasValue)
//                                                          .Select(r => (r.CompletedDate!.Value - r.AssignedDate).TotalDays)
//                                                          .DefaultIfEmpty(0)
//                                                          .Average();
//    }

//    // عنصر تقويم المراجعة
//    public class ReviewCalendarItem
//    {
//        public int ReviewId { get; set; }
//        public string Title { get; set; } = string.Empty;
//        public string ReviewerName { get; set; } = string.Empty;
//        public DateTime Date { get; set; }
//        public ReviewCalendarItemType Type { get; set; }
//        public string Color { get; set; } = string.Empty;
//        public bool IsOverdue { get; set; }
//        public string Url { get; set; } = string.Empty;

//        public string DisplayTitle => Type switch
//        {
//            ReviewCalendarItemType.Deadline => $"موعد نهائي: {Title}",
//            ReviewCalendarItemType.Assignment => $"تعيين: {Title}",
//            ReviewCalendarItemType.Completion => $"اكتمال: {Title}",
//            ReviewCalendarItemType.Reminder => $"تذكير: {Title}",
//            _ => Title
//        };
//    }

//    public enum ReviewCalendarItemType
//    {
//        Deadline,
//        Assignment,
//        Completion,
//        Reminder
//    }

//    // بيانات شهرية للمراجعات
//    public class MonthlyReviewData
//    {
//        public int Year { get; set; }
//        public int Month { get; set; }
//        public int AssignedCount { get; set; }
//        public int CompletedCount { get; set; }
//        public int OverdueCount { get; set; }
//        public double AverageCompletionTime { get; set; }
//        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("ar-SA"));
//        public double CompletionRate => AssignedCount > 0 ? (double)CompletedCount / AssignedCount * 100 : 0;
//    }

//    // تقارير المسار
//    public class TrackReportsViewModel
//    {
//        public TrackManager TrackManager { get; set; } = null!;
//        public string TrackName { get; set; } = string.Empty;

//        // Research Statistics
//        public int TotalResearches { get; set; }
//        public int AcceptedResearches { get; set; }
//        public int RejectedResearches { get; set; }
//        public int PendingResearches { get; set; }
//        public int WithdrawnResearches { get; set; }

//        // Review Statistics
//        public int TotalReviews { get; set; }
//        public int CompletedReviews { get; set; }
//        public int PendingReviews { get; set; }
//        public int OverdueReviews { get; set; }
//        public double AverageReviewTime { get; set; }

//        // Quality Metrics
//        public double AverageResearchScore { get; set; }
//        public double ReviewConsistency { get; set; } // مدى اتساق المراجعات
//        public int ConflictingReviews { get; set; } // مراجعات متضاربة

//        // Time-based Analysis
//        public List<MonthlySubmissionData> MonthlySubmissions { get; set; } = new();
//        public List<ReviewerPerformanceData> ReviewerPerformance { get; set; } = new();
//        public List<ResearchTypeDistribution> ResearchTypeStats { get; set; } = new();

//        // Trend Analysis
//        public TrendAnalysis SubmissionTrend { get; set; } = new();
//        public TrendAnalysis AcceptanceTrend { get; set; } = new();
//        public TrendAnalysis ReviewTimeTrend { get; set; } = new();

//        // Comparison with Other Tracks
//        public List<TrackComparisonItem> TrackComparisons { get; set; } = new();

//        // Calculated Properties
//        public double AcceptanceRate => TotalResearches > 0 ?
//            (double)AcceptedResearches / TotalResearches * 100 : 0;

//        public double RejectionRate => TotalResearches > 0 ?
//            (double)RejectedResearches / TotalResearches * 100 : 0;

//        public double CompletionRate => TotalReviews > 0 ?
//            (double)CompletedReviews / TotalReviews * 100 : 0;

//        public double OverdueRate => TotalReviews > 0 ?
//            (double)OverdueReviews / TotalReviews * 100 : 0;

//        // Performance Indicators
//        public TrackPerformanceLevel PerformanceLevel
//        {
//            get
//            {
//                var score = CalculatePerformanceScore();
//                return score switch
//                {
//                    >= 90 => TrackPerformanceLevel.Excellent,
//                    >= 80 => TrackPerformanceLevel.VeryGood,
//                    >= 70 => TrackPerformanceLevel.Good,
//                    >= 60 => TrackPerformanceLevel.Fair,
//                    _ => TrackPerformanceLevel.Poor
//                };
//            }
//        }

//        private double CalculatePerformanceScore()
//        {
//            double score = 0;

//            // Acceptance rate (25%)
//            if (AcceptanceRate >= 60) score += 25;
//            else if (AcceptanceRate >= 40) score += 20;
//            else if (AcceptanceRate >= 20) score += 15;
//            else score += 10;

//            // Review completion rate (25%)
//            if (CompletionRate >= 95) score += 25;
//            else if (CompletionRate >= 90) score += 20;
//            else if (CompletionRate >= 80) score += 15;
//            else score += 10;

//            // Average review time (25%)
//            if (AverageReviewTime <= 14) score += 25;
//            else if (AverageReviewTime <= 18) score += 20;
//            else if (AverageReviewTime <= 21) score += 15;
//            else score += 10;

//            // Overdue rate (25%)
//            if (OverdueRate <= 5) score += 25;
//            else if (OverdueRate <= 10) score += 20;
//            else if (OverdueRate <= 15) score += 15;
//            else score += 10;

//            return score;
//        }

//        // Export Options
//        public List<ExportOption> AvailableExports { get; set; } = new()
//        {
//            new ExportOption { Name = "تقرير شامل", Format = "PDF", Icon = "file-pdf" },
//            new ExportOption { Name = "إحصائيات Excel", Format = "XLSX", Icon = "file-excel" },
//            new ExportOption { Name = "بيانات CSV", Format = "CSV", Icon = "file-csv" },
//            new ExportOption { Name = "عرض تقديمي", Format = "PPTX", Icon = "file-powerpoint" }
//        };
//    }

//    // توزيع أنواع البحوث
//    public class ResearchTypeDistribution
//    {
//        public ResearchType Type { get; set; }
//        public string TypeName { get; set; } = string.Empty;
//        public int Count { get; set; }
//        public double Percentage { get; set; }
//        public double AcceptanceRate { get; set; }
//        public double AverageScore { get; set; }
//    }

//    // تحليل الاتجاه
//    public class TrendAnalysis
//    {
//        public double CurrentValue { get; set; }
//        public double PreviousValue { get; set; }
//        public double ChangePercentage { get; set; }
//        public TrendDirection Direction { get; set; }
//        public string TrendDescription { get; set; } = string.Empty;

//        public string TrendIcon => Direction switch
//        {
//            TrendDirection.Up => "arrow-up",
//            TrendDirection.Down => "arrow-down",
//            _ => "arrow-right"
//        };

//        public string TrendColor => Direction switch
//        {
//            TrendDirection.Up => "success",
//            TrendDirection.Down => "danger",
//            _ => "secondary"
//        };
//    }

//    public enum TrendDirection
//    {
//        Up,
//        Down,
//        Stable
//    }

//    public enum TrackPerformanceLevel
//    {
//        Excellent,
//        VeryGood,
//        Good,
//        Fair,
//        Poor
//    }

//    // عنصر مقارنة المسارات
//    public class TrackComparisonItem
//    {
//        public ResearchTrack Track { get; set; }
//        public string TrackName { get; set; } = string.Empty;
//        public double AcceptanceRate { get; set; }
//        public double AverageReviewTime { get; set; }
//        public double CompletionRate { get; set; }
//        public double OverallScore { get; set; }
//        public int Rank { get; set; }
//    }

//    // خيار التصدير
//    public class ExportOption
//    {
//        public string Name { get; set; } = string.Empty;
//        public string Format { get; set; } = string.Empty;
//        public string Icon { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//        public bool IsAvailable { get; set; } = true;
//    }

//}