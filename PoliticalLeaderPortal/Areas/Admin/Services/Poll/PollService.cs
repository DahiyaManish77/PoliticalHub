using PoliticalLeaderPortal.Areas.Admin.ViewModels.Poll;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels.Poll;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services.Poll
{
    public class PollService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public PollService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
            EnsureSchema();
        }

        public List<PollCategoryVM> GetCategories()
        {
            return _db.Database.SqlQuery<PollCategoryVM>(
                @"SELECT PollCategoryId, CategoryName, CategoryDescription, DisplayOrder, IsActive
                  FROM dbo.PollCategory
                  WHERE IsDeleted = 0
                  ORDER BY DisplayOrder, CategoryName").ToList();
        }

        public List<SelectListItem> GetCategoryDropdown()
        {
            return GetCategories()
                .Where(x => x.IsActive)
                .Select(x => new SelectListItem
                {
                    Value = x.PollCategoryId.ToString(),
                    Text = x.CategoryName
                }).ToList();
        }

        public PollCategoryVM GetCategoryById(int id)
        {
            return _db.Database.SqlQuery<PollCategoryVM>(
                @"SELECT PollCategoryId, CategoryName, CategoryDescription, DisplayOrder, IsActive
                  FROM dbo.PollCategory
                  WHERE PollCategoryId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id)).FirstOrDefault();
        }

        public bool IsDuplicateCategory(string name, int id = 0)
        {
            return _db.Database.SqlQuery<int>(
                @"SELECT COUNT(1)
                  FROM dbo.PollCategory
                  WHERE IsDeleted = 0
                    AND LOWER(LTRIM(RTRIM(CategoryName))) = LOWER(LTRIM(RTRIM(@Name)))
                    AND PollCategoryId <> @Id",
                new SqlParameter("@Name", name ?? String.Empty),
                new SqlParameter("@Id", id)).FirstOrDefault() > 0;
        }

        public void SaveCategory(PollCategoryVM model, int? userId)
        {
            if (model.PollCategoryId > 0)
            {
                _db.Database.ExecuteSqlCommand(
                    @"UPDATE dbo.PollCategory
                      SET CategoryName = @Name,
                          CategoryDescription = @Description,
                          DisplayOrder = @DisplayOrder,
                          IsActive = @IsActive,
                          ModifiedBy = @ModifiedBy,
                          ModifiedDate = GETDATE()
                      WHERE PollCategoryId = @Id",
                    P("@Name", model.CategoryName),
                    P("@Description", model.CategoryDescription),
                    new SqlParameter("@DisplayOrder", model.DisplayOrder),
                    new SqlParameter("@IsActive", model.IsActive),
                    P("@ModifiedBy", userId),
                    new SqlParameter("@Id", model.PollCategoryId));
                return;
            }

            _db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.PollCategory
                  (CategoryName, CategoryDescription, DisplayOrder, IsActive, CreatedBy, CreatedDate, IsDeleted)
                  VALUES
                  (@Name, @Description, @DisplayOrder, @IsActive, @CreatedBy, GETDATE(), 0)",
                P("@Name", model.CategoryName),
                P("@Description", model.CategoryDescription),
                new SqlParameter("@DisplayOrder", model.DisplayOrder),
                new SqlParameter("@IsActive", model.IsActive),
                P("@CreatedBy", userId));
        }

        public bool DeleteCategory(int id, int? userId)
        {
            bool hasPolls = _db.Database.SqlQuery<int>(
                "SELECT COUNT(1) FROM dbo.Poll WHERE PollCategoryId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id)).FirstOrDefault() > 0;

            if (hasPolls)
            {
                return false;
            }

            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.PollCategory
                  SET IsDeleted = 1, IsActive = 0, DeletedBy = @DeletedBy, DeletedDate = GETDATE()
                  WHERE PollCategoryId = @Id",
                P("@DeletedBy", userId),
                new SqlParameter("@Id", id));

            return true;
        }

        public List<PollListVM> GetPolls()
        {
            return _db.Database.SqlQuery<PollListVM>(
                @"SELECT p.PollId, p.Title, p.Question, c.CategoryName, p.Status, p.PublicSlug,
                         p.StartDate, p.EndDate, p.IsActive, p.CreatedDate,
                         ISNULL(v.TotalVotes, 0) AS TotalVotes
                  FROM dbo.Poll p
                  INNER JOIN dbo.PollCategory c ON c.PollCategoryId = p.PollCategoryId
                  OUTER APPLY
                  (
                      SELECT COUNT(1) AS TotalVotes
                      FROM dbo.PollVote pv
                      WHERE pv.PollId = p.PollId AND pv.IsValid = 1
                  ) v
                  WHERE p.IsDeleted = 0
                  ORDER BY p.CreatedDate DESC").ToList();
        }

        public PollEditVM NewPoll()
        {
            return new PollEditVM
            {
                IsActive = true,
                Status = "Draft",
                PollType = "Public Feedback",
                QuestionType = "SingleChoice",
                DisplayMode = "HomePartial",
                StartDate = DateTime.Today,
                Setting = DefaultSetting(),
                Categories = GetCategoryDropdown(),
                Options = new List<PollOptionVM>
                {
                    new PollOptionVM { DisplayOrder = 1, IsActive = true },
                    new PollOptionVM { DisplayOrder = 2, IsActive = true }
                }
            };
        }

        public PollEditVM GetPollById(int id)
        {
            PollEditVM model = _db.Database.SqlQuery<PollEditVM>(
                @"SELECT PollId, PollCategoryId, Title, Question, Description, PollType, QuestionType,
                         TargetArea, DisplayMode, PublicSlug, StartDate, EndDate, Status, IsActive, DisplayOrder
                  FROM dbo.Poll
                  WHERE PollId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id)).FirstOrDefault();

            if (model == null)
            {
                return null;
            }

            model.Categories = GetCategoryDropdown();
            model.Setting = GetSetting(id);
            model.Options = GetOptions(id);

            return model;
        }

        public int SavePoll(PollEditVM model, IEnumerable<string> optionTexts, int? userId)
        {
            ValidatePoll(model, optionTexts);

            if (model.PollId > 0)
            {
                _db.Database.ExecuteSqlCommand(
                    @"UPDATE dbo.Poll
                      SET PollCategoryId = @PollCategoryId,
                          Title = @Title,
                          Question = @Question,
                          Description = @Description,
                          PollType = @PollType,
                          QuestionType = @QuestionType,
                          TargetArea = @TargetArea,
                          DisplayMode = @DisplayMode,
                          StartDate = @StartDate,
                          EndDate = @EndDate,
                          IsActive = @IsActive,
                          DisplayOrder = @DisplayOrder,
                          ModifiedBy = @ModifiedBy,
                          ModifiedDate = GETDATE()
                      WHERE PollId = @PollId",
                    new SqlParameter("@PollCategoryId", model.PollCategoryId),
                    P("@Title", model.Title),
                    P("@Question", model.Question),
                    P("@Description", model.Description),
                    P("@PollType", model.PollType),
                    P("@QuestionType", model.QuestionType),
                    P("@TargetArea", model.TargetArea),
                    P("@DisplayMode", model.DisplayMode ?? "PageOnly"),
                    P("@StartDate", model.StartDate),
                    P("@EndDate", model.EndDate),
                    new SqlParameter("@IsActive", model.IsActive),
                    new SqlParameter("@DisplayOrder", model.DisplayOrder),
                    P("@ModifiedBy", userId),
                    new SqlParameter("@PollId", model.PollId));

                SaveSetting(model.PollId, model.Setting, userId);
                ReplaceOptions(model.PollId, optionTexts);
                AddAudit(model.PollId, "Update", userId, "Poll updated.");
                return model.PollId;
            }

            string slug = CreateSlug(model.Title);
            int pollId = _db.Database.SqlQuery<int>(
                @"INSERT INTO dbo.Poll
                  (PollCategoryId, Title, Question, Description, PollType, QuestionType, PublicSlug,
                   TargetArea, DisplayMode, StartDate, EndDate, Status, IsActive, DisplayOrder, CreatedBy, CreatedDate, IsDeleted)
                  OUTPUT INSERTED.PollId
                  VALUES
                  (@PollCategoryId, @Title, @Question, @Description, @PollType, @QuestionType, @PublicSlug,
                   @TargetArea, @DisplayMode, @StartDate, @EndDate, 'Draft', @IsActive, @DisplayOrder, @CreatedBy, GETDATE(), 0)",
                new SqlParameter("@PollCategoryId", model.PollCategoryId),
                P("@Title", model.Title),
                P("@Question", model.Question),
                P("@Description", model.Description),
                P("@PollType", model.PollType),
                P("@QuestionType", model.QuestionType),
                P("@PublicSlug", slug),
                P("@TargetArea", model.TargetArea),
                P("@DisplayMode", model.DisplayMode ?? "PageOnly"),
                P("@StartDate", model.StartDate),
                P("@EndDate", model.EndDate),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@DisplayOrder", model.DisplayOrder),
                P("@CreatedBy", userId)).First();

            SaveSetting(pollId, model.Setting, userId);
            ReplaceOptions(pollId, optionTexts);
            AddStatus(pollId, "Draft", userId, "Poll created.");
            AddAudit(pollId, "Create", userId, "Poll created.");
            return pollId;
        }

        public bool PublishPoll(int id, int? userId)
        {
            bool canPublish = GetOptions(id).Count(x => x.IsActive) >= 2;
            if (!canPublish)
            {
                return false;
            }

            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.Poll
                  SET Status = 'Published', PublishedBy = @UserId, PublishedDate = GETDATE(), ModifiedDate = GETDATE()
                  WHERE PollId = @Id AND IsDeleted = 0",
                P("@UserId", userId),
                new SqlParameter("@Id", id));

            AddStatus(id, "Published", userId, "Poll published.");
            AddAudit(id, "Publish", userId, "Poll published.");
            return true;
        }

        public void ChangeStatus(int id, string status, int? userId)
        {
            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.Poll
                  SET Status = @Status, ModifiedBy = @UserId, ModifiedDate = GETDATE()
                  WHERE PollId = @Id AND IsDeleted = 0",
                P("@Status", status),
                P("@UserId", userId),
                new SqlParameter("@Id", id));

            AddStatus(id, status, userId, "Poll status changed.");
            AddAudit(id, status, userId, "Poll status changed.");
        }

        public bool DeletePoll(int id, int? userId)
        {
            bool hasVotes = _db.Database.SqlQuery<int>(
                "SELECT COUNT(1) FROM dbo.PollVote WHERE PollId = @Id",
                new SqlParameter("@Id", id)).FirstOrDefault() > 0;

            if (hasVotes)
            {
                ChangeStatus(id, "Archived", userId);
                return true;
            }

            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.Poll
                  SET IsDeleted = 1, IsActive = 0, DeletedBy = @UserId, DeletedDate = GETDATE()
                  WHERE PollId = @Id",
                P("@UserId", userId),
                new SqlParameter("@Id", id));

            AddAudit(id, "Delete", userId, "Poll soft deleted.");
            return true;
        }

        public PublicPollVM GetPublicPoll(string slug)
        {
            PublicPollVM model = _db.Database.SqlQuery<PublicPollVM>(
                @"SELECT p.PollId, p.Title, p.Question, p.Description, p.PublicSlug, p.Status, p.DisplayMode,
                         p.StartDate, p.EndDate,
                         s.ShowPublicResults, s.RequireConsent, s.AllowMultipleVotes, s.RequireMobile, s.RequireName,
                         s.ResultVisibility, s.ThankYouMessage, s.ClosedMessage,
                         ISNULL(v.TotalVotes, 0) AS TotalVotes
                  FROM dbo.Poll p
                  INNER JOIN dbo.PollSetting s ON s.PollId = p.PollId
                  OUTER APPLY
                  (
                      SELECT COUNT(1) AS TotalVotes
                      FROM dbo.PollVote pv
                      WHERE pv.PollId = p.PollId AND pv.IsValid = 1
                  ) v
                  WHERE p.PublicSlug = @Slug
                    AND p.IsDeleted = 0
                    AND p.IsActive = 1",
                P("@Slug", slug)).FirstOrDefault();

            if (model == null)
            {
                return null;
            }

            model.Options = GetPublicOptions(model.PollId);
            return model;
        }

        public PublicPollVM GetFeaturedPoll()
        {
            var slug = _db.Database.SqlQuery<string>(
                @"SELECT TOP 1 PublicSlug
                  FROM dbo.Poll
                  WHERE IsDeleted=0 AND IsActive=1 AND Status='Published'
                    AND DisplayMode IN ('HomePartial','HomeModal')
                    AND (StartDate IS NULL OR StartDate <= GETDATE())
                    AND (EndDate IS NULL OR EndDate >= GETDATE())
                  ORDER BY DisplayOrder, PublishedDate DESC, PollId DESC").FirstOrDefault();
            return String.IsNullOrWhiteSpace(slug) ? null : GetPublicPoll(slug);
        }

        public bool IsPollOpen(PublicPollVM poll)
        {
            if (poll == null || poll.Status != "Published")
            {
                return false;
            }

            DateTime now = DateTime.Now;
            return (!poll.StartDate.HasValue || poll.StartDate.Value <= now) &&
                   (!poll.EndDate.HasValue || poll.EndDate.Value >= now);
        }

        public void SaveVote(PublicPollVoteVM model, string ipAddress, string userAgent)
        {
            PublicPollVM poll = GetPublicPoll(model.PublicSlug);
            if (!IsPollOpen(poll))
            {
                throw new InvalidOperationException("This poll is not open for voting.");
            }

            if (poll.RequireConsent && !model.ConsentGiven)
            {
                throw new InvalidOperationException("Consent is required.");
            }

            if (poll.RequireName && String.IsNullOrWhiteSpace(model.RespondentName))
            {
                throw new InvalidOperationException("Name is required.");
            }

            if (poll.RequireMobile && String.IsNullOrWhiteSpace(model.MobileNo))
            {
                throw new InvalidOperationException("Mobile number is required.");
            }

            bool optionBelongs = poll.Options.Any(x => x.PollOptionId == model.PollOptionId);
            if (!optionBelongs)
            {
                throw new InvalidOperationException("Invalid poll option.");
            }

            if (!poll.AllowMultipleVotes)
            {
                bool duplicateVote = _db.Database.SqlQuery<int>(
                    @"SELECT COUNT(1)
                      FROM dbo.PollVote
                      WHERE PollId = @PollId
                        AND IsValid = 1
                        AND (
                            IpAddress = @IpAddress
                            OR (NULLIF(LTRIM(RTRIM(@MobileNo)), '') IS NOT NULL AND MobileNo = @MobileNo)
                        )",
                    new SqlParameter("@PollId", poll.PollId),
                    P("@IpAddress", ipAddress),
                    P("@MobileNo", model.MobileNo)).FirstOrDefault() > 0;

                if (duplicateVote)
                {
                    throw new InvalidOperationException("Your vote has already been recorded for this poll.");
                }
            }

            int respondentId = _db.Database.SqlQuery<int>(
                @"INSERT INTO dbo.PollRespondent
                  (RespondentName, MobileNo, AreaName, IpAddress, UserAgent, CreatedDate)
                  OUTPUT INSERTED.PollRespondentId
                  VALUES
                  (@Name, @Mobile, @Area, @IpAddress, @UserAgent, GETDATE())",
                P("@Name", model.RespondentName),
                P("@Mobile", model.MobileNo),
                P("@Area", model.AreaName),
                P("@IpAddress", ipAddress),
                P("@UserAgent", userAgent)).First();

            int voteId = _db.Database.SqlQuery<int>(
                @"INSERT INTO dbo.PollVote
                  (PollId, PollRespondentId, VoteText, Source, IpAddress, UserAgent, ConsentGiven,
                   SubmittedDate, IsValid, ValidationStatus)
                  OUTPUT INSERTED.PollVoteId
                  VALUES
                  (@PollId, @RespondentId, @Remarks, @Source, @IpAddress, @UserAgent, @ConsentGiven,
                   GETDATE(), 1, 'Valid')",
                new SqlParameter("@PollId", poll.PollId),
                new SqlParameter("@RespondentId", respondentId),
                P("@Remarks", model.Remarks),
                P("@Source", model.Source ?? "web"),
                P("@IpAddress", ipAddress),
                P("@UserAgent", userAgent),
                new SqlParameter("@ConsentGiven", model.ConsentGiven)).First();

            _db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.PollVoteOption (PollVoteId, PollOptionId, CreatedDate)
                  VALUES (@PollVoteId, @PollOptionId, GETDATE())",
                new SqlParameter("@PollVoteId", voteId),
                new SqlParameter("@PollOptionId", model.PollOptionId));
        }

        public PollResultVM GetResults(int id, string baseUrl)
        {
            PollResultVM model = _db.Database.SqlQuery<PollResultVM>(
                @"SELECT PollId, Title, Question, Status, 0 AS TotalVotes
                  FROM dbo.Poll WHERE PollId = @Id",
                new SqlParameter("@Id", id)).FirstOrDefault();

            if (model == null)
            {
                return null;
            }

            model.Options = GetOptionsWithResults(id);
            model.TotalVotes = model.Options.Sum(x => x.VoteCount);
            model.Votes = GetVotes(id);

            string slug = _db.Database.SqlQuery<string>(
                "SELECT PublicSlug FROM dbo.Poll WHERE PollId = @Id",
                new SqlParameter("@Id", id)).FirstOrDefault();

            model.PublicUrl = (baseUrl ?? String.Empty).TrimEnd('/') + "/poll/" + Uri.EscapeDataString(slug ?? String.Empty);
            return model;
        }

        public List<PollVoteVM> GetVotesForExport(int pollId)
        {
            return _db.Database.SqlQuery<PollVoteVM>(
                @"SELECT v.PollVoteId, o.OptionText, r.RespondentName, r.MobileNo, r.AreaName,
                         v.Source, v.ConsentGiven, v.IsValid, v.SubmittedDate
                  FROM dbo.PollVote v
                  LEFT JOIN dbo.PollRespondent r ON r.PollRespondentId = v.PollRespondentId
                  LEFT JOIN dbo.PollVoteOption vo ON vo.PollVoteId = v.PollVoteId
                  LEFT JOIN dbo.PollOption o ON o.PollOptionId = vo.PollOptionId
                  WHERE v.PollId = @PollId
                  ORDER BY v.SubmittedDate DESC",
                new SqlParameter("@PollId", pollId)).ToList();
        }

        private PollSettingVM GetSetting(int pollId)
        {
            PollSettingVM setting = _db.Database.SqlQuery<PollSettingVM>(
                @"SELECT ShowPublicResults, RequireConsent, AllowMultipleVotes, RequireMobile, RequireName,
                         ResultVisibility, DuplicatePolicy, MaxVotesPerRespondent, IsAnonymous,
                         ThankYouMessage, ClosedMessage
                  FROM dbo.PollSetting WHERE PollId = @PollId",
                new SqlParameter("@PollId", pollId)).FirstOrDefault();

            return setting ?? DefaultSetting();
        }

        private PollSettingVM DefaultSetting()
        {
            return new PollSettingVM
            {
                RequireConsent = true,
                ShowPublicResults = false,
                AllowMultipleVotes = false,
                ResultVisibility = "AfterVote",
                DuplicatePolicy = "Soft",
                MaxVotesPerRespondent = 1,
                ThankYouMessage = "Thank you for your feedback.",
                ClosedMessage = "This poll is currently closed."
            };
        }

        private void SaveSetting(int pollId, PollSettingVM setting, int? userId)
        {
            setting = setting ?? DefaultSetting();

            _db.Database.ExecuteSqlCommand(
                @"IF EXISTS (SELECT 1 FROM dbo.PollSetting WHERE PollId = @PollId)
                  BEGIN
                      UPDATE dbo.PollSetting
                      SET ShowPublicResults = @ShowPublicResults,
                          RequireConsent = @RequireConsent,
                          AllowMultipleVotes = @AllowMultipleVotes,
                          RequireMobile = @RequireMobile,
                          RequireName = @RequireName,
                          ResultVisibility = @ResultVisibility,
                          DuplicatePolicy = @DuplicatePolicy,
                          MaxVotesPerRespondent = @MaxVotesPerRespondent,
                          IsAnonymous = @IsAnonymous,
                          ThankYouMessage = @ThankYouMessage,
                          ClosedMessage = @ClosedMessage,
                          ModifiedBy = @UserId,
                          ModifiedDate = GETDATE()
                      WHERE PollId = @PollId
                  END
                  ELSE
                  BEGIN
                      INSERT INTO dbo.PollSetting
                      (PollId, ShowPublicResults, RequireConsent, AllowMultipleVotes, RequireMobile, RequireName,
                       ResultVisibility, DuplicatePolicy, MaxVotesPerRespondent, IsAnonymous, ThankYouMessage,
                       ClosedMessage, CreatedBy, CreatedDate)
                      VALUES
                      (@PollId, @ShowPublicResults, @RequireConsent, @AllowMultipleVotes, @RequireMobile, @RequireName,
                       @ResultVisibility, @DuplicatePolicy, @MaxVotesPerRespondent, @IsAnonymous, @ThankYouMessage,
                       @ClosedMessage, @UserId, GETDATE())
                  END",
                new SqlParameter("@PollId", pollId),
                new SqlParameter("@ShowPublicResults", setting.ShowPublicResults),
                new SqlParameter("@RequireConsent", setting.RequireConsent),
                new SqlParameter("@AllowMultipleVotes", setting.AllowMultipleVotes),
                new SqlParameter("@RequireMobile", setting.RequireMobile),
                new SqlParameter("@RequireName", setting.RequireName),
                P("@ResultVisibility", setting.ResultVisibility ?? "AfterVote"),
                P("@DuplicatePolicy", setting.DuplicatePolicy ?? "Soft"),
                new SqlParameter("@MaxVotesPerRespondent", setting.MaxVotesPerRespondent <= 0 ? 1 : setting.MaxVotesPerRespondent),
                new SqlParameter("@IsAnonymous", setting.IsAnonymous),
                P("@ThankYouMessage", setting.ThankYouMessage),
                P("@ClosedMessage", setting.ClosedMessage),
                P("@UserId", userId));
        }

        private List<PollOptionVM> GetOptions(int pollId)
        {
            return _db.Database.SqlQuery<PollOptionVM>(
                @"SELECT PollOptionId, PollId, OptionText, OptionDescription, DisplayOrder, IsActive,
                         0 AS VoteCount, CAST(0 AS decimal(18,2)) AS VotePercent
                  FROM dbo.PollOption
                  WHERE PollId = @PollId AND IsDeleted = 0
                  ORDER BY DisplayOrder, PollOptionId",
                new SqlParameter("@PollId", pollId)).ToList();
        }

        private List<PublicPollOptionVM> GetPublicOptions(int pollId)
        {
            var options = _db.Database.SqlQuery<PublicPollOptionVM>(
                @"SELECT o.PollOptionId, o.OptionText, COUNT(v.PollVoteId) AS VoteCount, CAST(0 AS decimal(18,2)) AS VotePercent
                  FROM dbo.PollOption o
                  LEFT JOIN dbo.PollVoteOption vo ON vo.PollOptionId = o.PollOptionId
                  LEFT JOIN dbo.PollVote v ON v.PollVoteId = vo.PollVoteId AND v.IsValid = 1
                  WHERE o.PollId = @PollId AND o.IsDeleted = 0 AND o.IsActive = 1
                  GROUP BY o.PollOptionId, o.OptionText, o.DisplayOrder
                  ORDER BY o.DisplayOrder, o.PollOptionId",
                new SqlParameter("@PollId", pollId)).ToList();

            int total = options.Sum(x => x.VoteCount);
            foreach (var option in options)
            {
                option.VotePercent = total == 0 ? 0 : Math.Round((decimal)option.VoteCount * 100 / total, 2);
            }

            return options;
        }

        private List<PollOptionVM> GetOptionsWithResults(int pollId)
        {
            var options = _db.Database.SqlQuery<PollOptionVM>(
                @"SELECT o.PollOptionId, o.PollId, o.OptionText, o.OptionDescription, o.DisplayOrder, o.IsActive,
                         COUNT(v.PollVoteId) AS VoteCount, CAST(0 AS decimal(18,2)) AS VotePercent
                  FROM dbo.PollOption o
                  LEFT JOIN dbo.PollVoteOption vo ON vo.PollOptionId = o.PollOptionId
                  LEFT JOIN dbo.PollVote v ON v.PollVoteId = vo.PollVoteId AND v.IsValid = 1
                  WHERE o.PollId = @PollId AND o.IsDeleted = 0
                  GROUP BY o.PollOptionId, o.PollId, o.OptionText, o.OptionDescription, o.DisplayOrder, o.IsActive
                  ORDER BY o.DisplayOrder, o.PollOptionId",
                new SqlParameter("@PollId", pollId)).ToList();

            int total = options.Sum(x => x.VoteCount);
            foreach (var option in options)
            {
                option.VotePercent = total == 0 ? 0 : Math.Round((decimal)option.VoteCount * 100 / total, 2);
            }

            return options;
        }

        private List<PollVoteVM> GetVotes(int pollId)
        {
            return _db.Database.SqlQuery<PollVoteVM>(
                @"SELECT TOP 200 v.PollVoteId, o.OptionText, r.RespondentName, r.MobileNo, r.AreaName,
                         v.Source, v.ConsentGiven, v.IsValid, v.SubmittedDate
                  FROM dbo.PollVote v
                  LEFT JOIN dbo.PollRespondent r ON r.PollRespondentId = v.PollRespondentId
                  LEFT JOIN dbo.PollVoteOption vo ON vo.PollVoteId = v.PollVoteId
                  LEFT JOIN dbo.PollOption o ON o.PollOptionId = vo.PollOptionId
                  WHERE v.PollId = @PollId
                  ORDER BY v.SubmittedDate DESC",
                new SqlParameter("@PollId", pollId)).ToList();
        }

        private void ReplaceOptions(int pollId, IEnumerable<string> optionTexts)
        {
            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.PollOption SET IsDeleted = 1, IsActive = 0 WHERE PollId = @PollId",
                new SqlParameter("@PollId", pollId));

            int order = 1;
            foreach (string text in (optionTexts ?? new string[0]).Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                _db.Database.ExecuteSqlCommand(
                    @"INSERT INTO dbo.PollOption
                      (PollId, OptionText, DisplayOrder, IsActive, CreatedDate, IsDeleted)
                      VALUES (@PollId, @OptionText, @DisplayOrder, 1, GETDATE(), 0)",
                    new SqlParameter("@PollId", pollId),
                    P("@OptionText", text),
                    new SqlParameter("@DisplayOrder", order++));
            }
        }

        private void ValidatePoll(PollEditVM model, IEnumerable<string> optionTexts)
        {
            if (model.EndDate.HasValue && model.StartDate.HasValue && model.EndDate.Value <= model.StartDate.Value)
            {
                throw new InvalidOperationException("End date cannot be before start date.");
            }

            int optionCount = (optionTexts ?? new string[0]).Count(x => !String.IsNullOrWhiteSpace(x));
            if ((model.QuestionType ?? "SingleChoice") != "Text" && optionCount < 2)
            {
                throw new InvalidOperationException("Please enter at least two poll options.");
            }
        }

        private string CreateSlug(string title)
        {
            string clean = new string((title ?? "poll").ToLowerInvariant().Select(ch => Char.IsLetterOrDigit(ch) ? ch : '-').ToArray()).Trim('-');
            if (String.IsNullOrWhiteSpace(clean))
            {
                clean = "poll";
            }

            string candidate = clean;
            int counter = 1;
            while (_db.Database.SqlQuery<int>("SELECT COUNT(1) FROM dbo.Poll WHERE PublicSlug = @Slug", P("@Slug", candidate)).FirstOrDefault() > 0)
            {
                candidate = clean + "-" + counter++;
            }

            return candidate;
        }

        private void AddStatus(int pollId, string status, int? userId, string remarks)
        {
            _db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.PollStatusHistory (PollId, Status, ChangedBy, ChangedDate, Remarks)
                  VALUES (@PollId, @Status, @ChangedBy, GETDATE(), @Remarks)",
                new SqlParameter("@PollId", pollId),
                P("@Status", status),
                P("@ChangedBy", userId),
                P("@Remarks", remarks));
        }

        private void AddAudit(int pollId, string action, int? userId, string remarks)
        {
            _db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.PollAuditLog (PollId, ActionName, PerformedBy, Remarks, CreatedDate)
                  VALUES (@PollId, @ActionName, @PerformedBy, @Remarks, GETDATE())",
                new SqlParameter("@PollId", pollId),
                P("@ActionName", action),
                P("@PerformedBy", userId),
                P("@Remarks", remarks));
        }

        private SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private void EnsureSchema()
        {
            _db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID('dbo.PollCategory', 'U') IS NULL
CREATE TABLE dbo.PollCategory
(
    PollCategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollCategory PRIMARY KEY,
    CategoryName NVARCHAR(150) NOT NULL,
    CategoryDescription NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL CONSTRAINT DF_PollCategory_DisplayOrder DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_PollCategory_IsActive DEFAULT(1),
    CreatedBy INT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollCategory_CreatedDate DEFAULT(GETDATE()),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_PollCategory_IsDeleted DEFAULT(0),
    DeletedBy INT NULL,
    DeletedDate DATETIME NULL
);

IF OBJECT_ID('dbo.Poll', 'U') IS NULL
CREATE TABLE dbo.Poll
(
    PollId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Poll PRIMARY KEY,
    PollCategoryId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Question NVARCHAR(500) NOT NULL,
    Description NVARCHAR(1000) NULL,
    PollType NVARCHAR(50) NOT NULL,
    QuestionType NVARCHAR(50) NOT NULL,
    PublicSlug NVARCHAR(180) NOT NULL,
    TargetArea NVARCHAR(200) NULL,
    StartDate DATETIME NULL,
    EndDate DATETIME NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Poll_Status DEFAULT('Draft'),
    IsActive BIT NOT NULL CONSTRAINT DF_Poll_IsActive DEFAULT(1),
    DisplayOrder INT NOT NULL CONSTRAINT DF_Poll_DisplayOrder DEFAULT(0),
    CreatedBy INT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_Poll_CreatedDate DEFAULT(GETDATE()),
    PublishedBy INT NULL,
    PublishedDate DATETIME NULL,
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Poll_IsDeleted DEFAULT(0),
    DeletedBy INT NULL,
    DeletedDate DATETIME NULL
);

IF OBJECT_ID('dbo.PollSetting', 'U') IS NULL
CREATE TABLE dbo.PollSetting
(
    PollSettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollSetting PRIMARY KEY,
    PollId INT NOT NULL,
    ShowPublicResults BIT NOT NULL CONSTRAINT DF_PollSetting_ShowPublicResults DEFAULT(0),
    RequireConsent BIT NOT NULL CONSTRAINT DF_PollSetting_RequireConsent DEFAULT(1),
    AllowMultipleVotes BIT NOT NULL CONSTRAINT DF_PollSetting_AllowMultipleVotes DEFAULT(0),
    RequireMobile BIT NOT NULL CONSTRAINT DF_PollSetting_RequireMobile DEFAULT(0),
    RequireName BIT NOT NULL CONSTRAINT DF_PollSetting_RequireName DEFAULT(0),
    ResultVisibility NVARCHAR(30) NOT NULL CONSTRAINT DF_PollSetting_ResultVisibility DEFAULT('AfterVote'),
    DuplicatePolicy NVARCHAR(50) NOT NULL CONSTRAINT DF_PollSetting_DuplicatePolicy DEFAULT('Soft'),
    MaxVotesPerRespondent INT NOT NULL CONSTRAINT DF_PollSetting_MaxVotes DEFAULT(1),
    IsAnonymous BIT NOT NULL CONSTRAINT DF_PollSetting_IsAnonymous DEFAULT(0),
    ThankYouMessage NVARCHAR(500) NULL,
    ClosedMessage NVARCHAR(500) NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollSetting_CreatedDate DEFAULT(GETDATE()),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL
);

IF OBJECT_ID('dbo.PollOption', 'U') IS NULL
CREATE TABLE dbo.PollOption
(
    PollOptionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollOption PRIMARY KEY,
    PollId INT NOT NULL,
    OptionText NVARCHAR(250) NOT NULL,
    OptionDescription NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL CONSTRAINT DF_PollOption_DisplayOrder DEFAULT(0),
    IsActive BIT NOT NULL CONSTRAINT DF_PollOption_IsActive DEFAULT(1),
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollOption_CreatedDate DEFAULT(GETDATE()),
    IsDeleted BIT NOT NULL CONSTRAINT DF_PollOption_IsDeleted DEFAULT(0)
);

IF OBJECT_ID('dbo.PollRespondent', 'U') IS NULL
CREATE TABLE dbo.PollRespondent
(
    PollRespondentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollRespondent PRIMARY KEY,
    RespondentName NVARCHAR(150) NULL,
    MobileNo NVARCHAR(30) NULL,
    EmailAddress NVARCHAR(150) NULL,
    AreaName NVARCHAR(150) NULL,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollRespondent_CreatedDate DEFAULT(GETDATE())
);

IF OBJECT_ID('dbo.PollVote', 'U') IS NULL
CREATE TABLE dbo.PollVote
(
    PollVoteId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollVote PRIMARY KEY,
    PollId INT NOT NULL,
    PollRespondentId INT NULL,
    VoteText NVARCHAR(1000) NULL,
    Source NVARCHAR(50) NULL,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    ConsentGiven BIT NOT NULL CONSTRAINT DF_PollVote_Consent DEFAULT(0),
    SubmittedDate DATETIME NOT NULL CONSTRAINT DF_PollVote_SubmittedDate DEFAULT(GETDATE()),
    IsValid BIT NOT NULL CONSTRAINT DF_PollVote_IsValid DEFAULT(1),
    ValidationStatus NVARCHAR(50) NULL,
    InvalidReason NVARCHAR(300) NULL
);

IF OBJECT_ID('dbo.PollVoteOption', 'U') IS NULL
CREATE TABLE dbo.PollVoteOption
(
    PollVoteOptionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollVoteOption PRIMARY KEY,
    PollVoteId INT NOT NULL,
    PollOptionId INT NOT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollVoteOption_CreatedDate DEFAULT(GETDATE())
);

IF OBJECT_ID('dbo.PollStatusHistory', 'U') IS NULL
CREATE TABLE dbo.PollStatusHistory
(
    PollStatusHistoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollStatusHistory PRIMARY KEY,
    PollId INT NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    ChangedBy INT NULL,
    ChangedDate DATETIME NOT NULL CONSTRAINT DF_PollStatusHistory_ChangedDate DEFAULT(GETDATE()),
    Remarks NVARCHAR(500) NULL
);

IF OBJECT_ID('dbo.PollAuditLog', 'U') IS NULL
CREATE TABLE dbo.PollAuditLog
(
    PollAuditLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PollAuditLog PRIMARY KEY,
    PollId INT NULL,
    ActionName NVARCHAR(80) NOT NULL,
    PerformedBy INT NULL,
    IpAddress NVARCHAR(64) NULL,
    Remarks NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_PollAuditLog_CreatedDate DEFAULT(GETDATE())
);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Poll_PublicSlug' AND object_id = OBJECT_ID('dbo.Poll'))
CREATE UNIQUE INDEX UX_Poll_PublicSlug ON dbo.Poll(PublicSlug);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Poll_Status_Date' AND object_id = OBJECT_ID('dbo.Poll'))
CREATE INDEX IX_Poll_Status_Date ON dbo.Poll(Status, IsActive, StartDate, EndDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PollOption_Poll' AND object_id = OBJECT_ID('dbo.PollOption'))
CREATE INDEX IX_PollOption_Poll ON dbo.PollOption(PollId, DisplayOrder);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PollVote_Poll' AND object_id = OBJECT_ID('dbo.PollVote'))
CREATE INDEX IX_PollVote_Poll ON dbo.PollVote(PollId, SubmittedDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PollVoteOption_Option' AND object_id = OBJECT_ID('dbo.PollVoteOption'))
CREATE INDEX IX_PollVoteOption_Option ON dbo.PollVoteOption(PollOptionId);");

            _db.Database.ExecuteSqlCommand(@"
IF COL_LENGTH('dbo.Poll','DisplayMode') IS NULL
    ALTER TABLE dbo.Poll ADD DisplayMode NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Poll_DisplayMode DEFAULT('PageOnly');");
        }
    }
}
