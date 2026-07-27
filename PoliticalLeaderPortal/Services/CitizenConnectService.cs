using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Services
{
    public class CitizenConnectService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;
        private readonly string connectionString;
        private readonly GeographyLookupService geography;

        public CitizenConnectService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
            connectionString = db.Database.Connection.ConnectionString;
            geography = new GeographyLookupService();
        }

        public List<CitizenConnectVM> GetAll(string requestType, string status, string keyword)
        {
            EnsureSchema();

            IQueryable<CitizenConnectRequest> query = db.CitizenConnectRequests
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            requestType = Clean(requestType);
            status = Clean(status);
            keyword = Clean(keyword);

            if (requestType != null)
                query = query.Where(x => x.RequestType == requestType);

            if (status != null)
                query = query.Where(x => x.Status == status);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.FullName.Contains(keyword) ||
                    x.MobileNumber.Contains(keyword) ||
                    (x.Email != null && x.Email.Contains(keyword)) ||
                    x.Subject.Contains(keyword) ||
                    (x.District != null && x.District.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new CitizenConnectVM
                {
                    CitizenConnectId = x.CitizenConnectId,
                    RequestType = x.RequestType,
                    FullName = x.FullName,
                    MobileNumber = x.MobileNumber,
                    Email = x.Email,
                    District = x.District,
                    Assembly = x.Assembly,
                    StateId = x.StateId,
                    DistrictId = x.DistrictId,
                    AssemblyConstituencyId = x.AssemblyConstituencyId,
                    BlockId = x.BlockId,
                    GramPanchayatId = x.GramPanchayatId,
                    VillageId = x.VillageId,
                    Subject = x.Subject,
                    Message = x.Message,
                    PreferredRole = x.PreferredRole,
                    Skills = x.Skills,
                    AvailableDays = x.AvailableDays,
                    AvailableTime = x.AvailableTime,
                    WhatsAppConsent = x.WhatsAppConsent,
                    SmsConsent = x.SmsConsent,
                    EmailConsent = x.EmailConsent,
                    VoiceConsent = x.VoiceConsent,
                    PrivacyConsent = x.PrivacyConsent,
                    Status = x.Status,
                    AdminRemarks = x.AdminRemarks,
                    CreatedDate = x.CreatedDate,
                    ConvertedPersonId = x.ConvertedPersonId,
                    ConvertedDate = x.ConvertedDate
                }).ToList();
        }

        public CitizenConnectVM GetById(int id)
        {
            EnsureSchema();
            CitizenConnectRequest x = db.CitizenConnectRequests
                .AsNoTracking()
                .FirstOrDefault(a => a.CitizenConnectId == id && !a.IsDeleted);

            return x == null ? null : Map(x);
        }

        public bool Save(CitizenConnectVM model)
        {
            if (model == null) throw new ArgumentNullException("model");
            EnsureSchema();
            Normalize(model);
            if (String.Equals(model.RequestType, "Volunteer", StringComparison.OrdinalIgnoreCase))
                ValidateVolunteerGeography(model);

            string mobile = NormalizeMobile(model.MobileNumber);
            DateTime duplicateSince = DateTime.Now.AddDays(-1);

            bool duplicate = db.CitizenConnectRequests.Any(x =>
                !x.IsDeleted &&
                x.RequestType == model.RequestType &&
                x.MobileNumber == mobile &&
                x.CreatedDate >= duplicateSince);

            if (duplicate) return false;

            var entity = new CitizenConnectRequest
            {
                RequestType = model.RequestType,
                FullName = model.FullName,
                MobileNumber = mobile,
                Email = model.Email,
                District = model.District,
                Assembly = model.Assembly,
                StateId = model.StateId,
                DistrictId = model.DistrictId,
                AssemblyConstituencyId = model.AssemblyConstituencyId,
                BlockId = model.BlockId,
                GramPanchayatId = model.GramPanchayatId,
                VillageId = model.VillageId,
                Subject = model.Subject,
                Message = model.Message,
                PreferredRole = model.PreferredRole,
                Skills = model.Skills,
                AvailableDays = model.AvailableDays,
                AvailableTime = model.AvailableTime,
                WhatsAppConsent = model.WhatsAppConsent,
                SmsConsent = model.SmsConsent,
                EmailConsent = model.EmailConsent,
                VoiceConsent = model.VoiceConsent,
                PrivacyConsent = model.PrivacyConsent,
                Status = "New",
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            db.CitizenConnectRequests.Add(entity);
            db.SaveChanges();
            return true;
        }

        private void ValidateVolunteerGeography(CitizenConnectVM model)
        {
            if (!geography.IsValidSelection("District", model.DistrictId, model.StateId, "State"))
                throw new InvalidOperationException("The selected District does not belong to the selected State.");
            if (!geography.IsValidSelection("ParliamentaryConstituency", model.ParliamentaryConstituencyId, model.StateId, "State"))
                throw new InvalidOperationException("The selected Parliamentary Constituency does not belong to the selected State.");
            if (!geography.IsValidSelection("AssemblyConstituency", model.AssemblyConstituencyId,
                model.ParliamentaryConstituencyId ?? model.StateId,
                model.ParliamentaryConstituencyId.HasValue ? "ParliamentaryConstituency" : "State"))
                throw new InvalidOperationException("The selected Assembly Constituency is not valid for the selected hierarchy.");
            if (!geography.IsValidSelection("Block", model.BlockId, model.DistrictId, "District"))
                throw new InvalidOperationException("The selected Block does not belong to the selected District.");
            if (!geography.IsValidSelection("GramPanchayat", model.GramPanchayatId, model.BlockId, "Block"))
                throw new InvalidOperationException("The selected Gram Panchayat does not belong to the selected Block.");
            if (!geography.IsValidSelection("Village", model.VillageId,
                model.GramPanchayatId ?? model.BlockId,
                model.GramPanchayatId.HasValue ? "GramPanchayat" : "Block"))
                throw new InvalidOperationException("The selected Village is not valid for the selected hierarchy.");
        }

        public bool UpdateStatus(int id, string status, string remarks)
        {
            EnsureSchema();
            CitizenConnectRequest entity = db.CitizenConnectRequests
                .FirstOrDefault(x => x.CitizenConnectId == id && !x.IsDeleted);

            if (entity == null) return false;

            entity.Status = NormalizeStatus(status);
            entity.AdminRemarks = Clean(remarks);
            entity.UpdatedDate = DateTime.Now;
            db.SaveChanges();
            return true;
        }

        public int ConvertVolunteerToPerson(int requestId, int userId)
        {
            EnsureSchema();
            EnsurePeopleSchema();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    CitizenConnectRequest request = db.CitizenConnectRequests
                        .FirstOrDefault(x => x.CitizenConnectId == requestId && !x.IsDeleted);

                    if (request == null)
                        throw new InvalidOperationException("Volunteer application not found.");

                    if (!String.Equals(request.RequestType, "Volunteer", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Only volunteer applications can be converted.");

                    if (!request.PrivacyConsent)
                        throw new InvalidOperationException("Volunteer privacy consent is required before conversion.");

                    if (request.ConvertedPersonId.HasValue)
                        return request.ConvertedPersonId.Value;

                    string mobile = NormalizeMobile(request.MobileNumber);
                    string actor = userId > 0 ? userId.ToString() : "System";
                    DateTime now = DateTime.Now;
                    int? parliamentaryConstituencyId =
                        geography.GetParliamentaryConstituencyId(request.AssemblyConstituencyId);

                    PersonMaster person = db.PersonMasters
                        .FirstOrDefault(x => !x.IsDeleted &&
                            (x.NormalizedMobile == mobile || x.MobileNumber == mobile));

                    if (person == null)
                    {
                        person = new PersonMaster
                        {
                            FullName = request.FullName,
                            MobileNumber = mobile,
                            NormalizedMobile = mobile,
                            Email = request.Email,
                            StateId = request.StateId,
                            DistrictId = request.DistrictId,
                            ParliamentaryConstituencyId = parliamentaryConstituencyId,
                            AssemblyConstituencyId = request.AssemblyConstituencyId,
                            BlockId = request.BlockId,
                            GramPanchayatId = request.GramPanchayatId,
                            VillageId = request.VillageId,
                            WhatsAppConsent = request.WhatsAppConsent,
                            SmsConsent = request.SmsConsent,
                            EmailConsent = request.EmailConsent,
                            VoiceCallConsent = request.VoiceConsent,
                            ConsentDate = now,
                            ConsentSource = "Citizen Connect Volunteer Form",
                            IsOptedOut = false,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = actor,
                            CreatedDate = now
                        };
                        db.PersonMasters.Add(person);
                        db.SaveChanges();
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(person.Email)) person.Email = request.Email;
                        if (!person.StateId.HasValue) person.StateId = request.StateId;
                        if (!person.DistrictId.HasValue) person.DistrictId = request.DistrictId;
                        if (!person.ParliamentaryConstituencyId.HasValue) person.ParliamentaryConstituencyId = parliamentaryConstituencyId;
                        if (!person.AssemblyConstituencyId.HasValue) person.AssemblyConstituencyId = request.AssemblyConstituencyId;
                        if (!person.BlockId.HasValue) person.BlockId = request.BlockId;
                        if (!person.GramPanchayatId.HasValue) person.GramPanchayatId = request.GramPanchayatId;
                        if (!person.VillageId.HasValue) person.VillageId = request.VillageId;
                        person.WhatsAppConsent = person.WhatsAppConsent || request.WhatsAppConsent;
                        person.SmsConsent = person.SmsConsent || request.SmsConsent;
                        person.EmailConsent = person.EmailConsent || request.EmailConsent;
                        person.VoiceCallConsent = person.VoiceCallConsent || request.VoiceConsent;
                        person.ConsentDate = now;
                        person.ConsentSource = "Citizen Connect Volunteer Form";
                        person.UpdatedBy = actor;
                        person.UpdatedDate = now;
                    }

                    VolunteerProfile volunteer = db.VolunteerProfiles
                        .FirstOrDefault(x => x.PersonId == person.PersonId && !x.IsDeleted);

                    if (volunteer == null)
                    {
                        volunteer = new VolunteerProfile
                        {
                            PersonId = person.PersonId,
                            PreferredRole = request.PreferredRole,
                            Skills = request.Skills,
                            AvailableDays = request.AvailableDays,
                            AvailableTime = request.AvailableTime,
                            Status = "Pending",
                            VerificationStatus = "Pending",
                            JoiningDate = now.Date,
                            Notes = "Created from Citizen Connect request #" + request.CitizenConnectId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = actor,
                            CreatedDate = now
                        };
                        db.VolunteerProfiles.Add(volunteer);
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(volunteer.PreferredRole)) volunteer.PreferredRole = request.PreferredRole;
                        if (String.IsNullOrWhiteSpace(volunteer.Skills)) volunteer.Skills = request.Skills;
                        if (String.IsNullOrWhiteSpace(volunteer.AvailableDays)) volunteer.AvailableDays = request.AvailableDays;
                        if (String.IsNullOrWhiteSpace(volunteer.AvailableTime)) volunteer.AvailableTime = request.AvailableTime;
                        volunteer.UpdatedBy = actor;
                        volunteer.UpdatedDate = now;
                    }

                    request.ConvertedPersonId = person.PersonId;
                    request.ConvertedDate = now;
                    request.ConvertedBy = userId > 0 ? (int?)userId : null;
                    request.Status = "Converted";
                    request.UpdatedDate = now;

                    db.SaveChanges();
                    transaction.Commit();
                    return person.PersonId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public bool Delete(int id)
        {
            EnsureSchema();
            CitizenConnectRequest entity = db.CitizenConnectRequests
                .FirstOrDefault(x => x.CitizenConnectId == id && !x.IsDeleted);

            if (entity == null) return false;
            if (entity.ConvertedPersonId.HasValue)
                throw new InvalidOperationException("A converted volunteer request cannot be deleted.");

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;
            db.SaveChanges();
            return true;
        }

        public void LoadGeography(CitizenConnectVM model)
        {
            model.States = geography.GetOptions("State", null, null, model.StateId);
            model.Districts = model.StateId.HasValue ? geography.GetOptions("District", model.StateId, "State", model.DistrictId) : new List<SelectListItem>();
            model.ParliamentaryConstituencies = model.StateId.HasValue ? geography.GetOptions("ParliamentaryConstituency", model.StateId, "State", model.ParliamentaryConstituencyId) : new List<SelectListItem>();
            model.AssemblyConstituencies = model.ParliamentaryConstituencyId.HasValue
                ? geography.GetOptions("AssemblyConstituency", model.ParliamentaryConstituencyId, "ParliamentaryConstituency", model.AssemblyConstituencyId)
                : model.StateId.HasValue ? geography.GetOptions("AssemblyConstituency", model.StateId, "State", model.AssemblyConstituencyId) : new List<SelectListItem>();
            model.Blocks = model.DistrictId.HasValue ? geography.GetOptions("Block", model.DistrictId, "District", model.BlockId) : new List<SelectListItem>();
            model.GramPanchayats = model.BlockId.HasValue ? geography.GetOptions("GramPanchayat", model.BlockId, "Block", model.GramPanchayatId) : new List<SelectListItem>();
            model.Villages = model.GramPanchayatId.HasValue
                ? geography.GetOptions("Village", model.GramPanchayatId, "GramPanchayat", model.VillageId)
                : model.BlockId.HasValue ? geography.GetOptions("Village", model.BlockId, "Block", model.VillageId) : new List<SelectListItem>();
        }

        public IList<SelectListItem> GetGeography(string type, int? parentId, string parentType = null)
        {
            return geography.GetOptions(type, parentId, parentType);
        }

        private void EnsureSchema()
        {
            try
            {
                db.Database.SqlQuery<int>("SELECT TOP 1 CitizenConnectId FROM dbo.CitizenConnectRequest").FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Citizen Connect database schema is missing or outdated. Run App_Data/SQL/03_CitizenConnect_Module_Upgrade.sql, then refresh the EDMX if required.", ex);
            }
        }

        private void EnsurePeopleSchema()
        {
            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();
                if (!TableExists(cn, "PersonMaster") || !TableExists(cn, "VolunteerProfile"))
                    throw new InvalidOperationException("People/Volunteer database tables are missing. Run the People Volunteer database upgrade script first.");
            }
        }

        private static CitizenConnectVM Map(CitizenConnectRequest x)
        {
            return new CitizenConnectVM
            {
                CitizenConnectId = x.CitizenConnectId,
                RequestType = x.RequestType,
                FullName = x.FullName,
                MobileNumber = x.MobileNumber,
                Email = x.Email,
                District = x.District,
                Assembly = x.Assembly,
                StateId = x.StateId,
                DistrictId = x.DistrictId,
                AssemblyConstituencyId = x.AssemblyConstituencyId,
                BlockId = x.BlockId,
                GramPanchayatId = x.GramPanchayatId,
                VillageId = x.VillageId,
                Subject = x.Subject,
                Message = x.Message,
                PreferredRole = x.PreferredRole,
                Skills = x.Skills,
                AvailableDays = x.AvailableDays,
                AvailableTime = x.AvailableTime,
                WhatsAppConsent = x.WhatsAppConsent,
                SmsConsent = x.SmsConsent,
                EmailConsent = x.EmailConsent,
                VoiceConsent = x.VoiceConsent,
                PrivacyConsent = x.PrivacyConsent,
                Status = x.Status,
                AdminRemarks = x.AdminRemarks,
                CreatedDate = x.CreatedDate,
                ConvertedPersonId = x.ConvertedPersonId,
                ConvertedDate = x.ConvertedDate
            };
        }

        private static void Normalize(CitizenConnectVM model)
        {
            model.RequestType = NormalizeRequestType(model.RequestType);
            model.FullName = Clean(model.FullName);
            model.MobileNumber = Clean(model.MobileNumber);
            model.Email = Clean(model.Email);
            model.District = Clean(model.District);
            model.Assembly = Clean(model.Assembly);
            model.Subject = Clean(model.Subject);
            model.Message = Clean(model.Message);
            model.PreferredRole = Clean(model.PreferredRole);
            model.Skills = Clean(model.Skills);
            model.AvailableDays = Clean(model.AvailableDays);
            model.AvailableTime = Clean(model.AvailableTime);

            if (model.RequestType == "Volunteer" && String.IsNullOrWhiteSpace(model.Subject))
                model.Subject = "Volunteer Application";
        }

        private static string NormalizeRequestType(string value)
        {
            if (String.Equals(value, "Volunteer", StringComparison.OrdinalIgnoreCase)) return "Volunteer";
            if (String.Equals(value, "Suggestion", StringComparison.OrdinalIgnoreCase)) return "Suggestion";
            if (String.Equals(value, "Issue", StringComparison.OrdinalIgnoreCase)) return "Issue";
            if (String.Equals(value, "Appointment", StringComparison.OrdinalIgnoreCase)) return "Appointment";
            if (String.Equals(value, "Invitation", StringComparison.OrdinalIgnoreCase)) return "Invitation";
            return "Contact";
        }

        private static string NormalizeStatus(string value)
        {
            string[] allowed = { "New", "In Review", "Need More Information", "Resolved", "Closed", "Rejected", "Converted" };
            string match = allowed.FirstOrDefault(x => String.Equals(x, value, StringComparison.OrdinalIgnoreCase));
            return match ?? "In Review";
        }

        private static string NormalizeMobile(string value)
        {
            string digits = new string((value ?? "").Where(Char.IsDigit).ToArray());
            if (digits.Length == 12 && digits.StartsWith("91")) digits = digits.Substring(2);
            if (digits.Length != 10) throw new InvalidOperationException("Mobile number must contain exactly 10 digits.");
            return digits;
        }

        private static string Clean(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool TableExists(SqlConnection cn, string table)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM sys.tables WHERE name=@Name AND schema_id=SCHEMA_ID('dbo')", cn))
            {
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 128).Value = table;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool ColumnExists(SqlConnection cn, string table, string column)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM sys.columns WHERE object_id=OBJECT_ID(@Table) AND name=@Column", cn))
            {
                cmd.Parameters.Add("@Table", SqlDbType.NVarChar, 260).Value = "dbo." + table;
                cmd.Parameters.Add("@Column", SqlDbType.NVarChar, 128).Value = column;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static string FirstColumn(SqlConnection cn, string table, IEnumerable<string> candidates)
        {
            foreach (string candidate in candidates)
                if (ColumnExists(cn, table, candidate)) return candidate;
            return null;
        }
    }
}
