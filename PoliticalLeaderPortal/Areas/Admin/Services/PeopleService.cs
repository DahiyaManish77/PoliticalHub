using PoliticalLeaderPortal.Areas.Admin.ViewModels.People;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class PeopleService
    {
        private readonly string _connectionString;
        private readonly GeographyLookupService _geography = new GeographyLookupService();

        public PeopleService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                _connectionString = db.Database.Connection.ConnectionString;
            }
        }

        public bool IsInstalled()
        {
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    return TableExists(cn, "PersonMaster") && TableExists(cn, "VolunteerProfile");
                }
            }
            catch
            {
                return false;
            }
        }

        public PeopleIndexVM GetIndex(string keyword, string status, int? assemblyId, bool volunteersOnly)
        {
            var vm = new PeopleIndexVM
            {
                Keyword = keyword,
                Status = status,
                AssemblyConstituencyId = assemblyId,
                VolunteersOnly = volunteersOnly,
                Rows = new List<PersonListItemVM>(),
                AssemblyOptions = new List<SelectListItem>()
            };

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                EnsureModule(cn);
                vm.TotalPeople = Scalar(cn, "SELECT COUNT(1) FROM dbo.PersonMaster WHERE IsDeleted=0");
                vm.ActivePeople = Scalar(cn, "SELECT COUNT(1) FROM dbo.PersonMaster WHERE IsDeleted=0 AND IsActive=1");
                vm.TotalVolunteers = Scalar(cn, "SELECT COUNT(1) FROM dbo.VolunteerProfile WHERE IsDeleted=0 AND IsActive=1");
                vm.PendingVerification = Scalar(cn, "SELECT COUNT(1) FROM dbo.VolunteerProfile WHERE IsDeleted=0 AND ISNULL(VerificationStatus,'Pending')='Pending'");
                vm.Rows = ReadRows(cn, keyword, status, assemblyId, volunteersOnly);
                vm.AssemblyOptions = _geography.GetOptions("AssemblyConstituency", null, null, assemblyId);
            }
            return vm;
        }

        public PersonEditVM Get(int id)
        {
            const string sql = @"SELECT p.*,v.VolunteerProfileId,v.PreferredRole,v.Skills,v.AvailableDays,v.AvailableTime,
 v.EmergencyContactName,v.EmergencyContactMobile,v.Status AS VolunteerStatus,v.VerificationStatus,
 v.JoiningDate,v.Notes
 FROM dbo.PersonMaster p
 LEFT JOIN dbo.VolunteerProfile v ON v.PersonId=p.PersonId AND v.IsDeleted=0
 WHERE p.PersonId=@Id AND p.IsDeleted=0";

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cn.Open();
                EnsureModule(cn);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    var model = Map(r);
                    r.Close();
                    LoadOptions(model);
                    return model;
                }
            }
        }

        public void LoadOptions(PersonEditVM model)
        {
            if (model == null) return;
            model.States = _geography.GetOptions("State", null, null, model.StateId);
            model.Districts = model.StateId.HasValue
                ? _geography.GetOptions("District", model.StateId, "State", model.DistrictId)
                : new List<SelectListItem>();
            model.ParliamentaryConstituencies = model.StateId.HasValue
                ? _geography.GetOptions("ParliamentaryConstituency", model.StateId, "State", model.ParliamentaryConstituencyId)
                : new List<SelectListItem>();
            model.AssemblyConstituencies = model.ParliamentaryConstituencyId.HasValue
                ? _geography.GetOptions("AssemblyConstituency", model.ParliamentaryConstituencyId, "ParliamentaryConstituency", model.AssemblyConstituencyId)
                : model.StateId.HasValue
                    ? _geography.GetOptions("AssemblyConstituency", model.StateId, "State", model.AssemblyConstituencyId)
                    : new List<SelectListItem>();
            model.Tehsils = model.DistrictId.HasValue
                ? _geography.GetOptions("Tehsil", model.DistrictId, "District", model.TehsilId)
                : new List<SelectListItem>();
            model.Blocks = model.DistrictId.HasValue
                ? _geography.GetOptions("Block", model.DistrictId, "District", model.BlockId)
                : new List<SelectListItem>();
            model.GramPanchayats = model.BlockId.HasValue
                ? _geography.GetOptions("GramPanchayat", model.BlockId, "Block", model.GramPanchayatId)
                : new List<SelectListItem>();
            model.Villages = model.GramPanchayatId.HasValue
                ? _geography.GetOptions("Village", model.GramPanchayatId, "GramPanchayat", model.VillageId)
                : model.BlockId.HasValue
                    ? _geography.GetOptions("Village", model.BlockId, "Block", model.VillageId)
                    : model.TehsilId.HasValue
                        ? _geography.GetOptions("Village", model.TehsilId, "Tehsil", model.VillageId)
                        : new List<SelectListItem>();

            model.Wards = _geography.GetOptions("Ward", model.AssemblyConstituencyId, "AssemblyConstituency", model.WardId);
            model.Mandals = _geography.GetOptions("Mandal", model.AssemblyConstituencyId, "AssemblyConstituency", model.MandalId);
            model.Sectors = _geography.GetOptions("Sector", model.MandalId, "Mandal", model.SectorId);
            model.Booths = _geography.GetOptions("Booth", model.AssemblyConstituencyId, "AssemblyConstituency", model.BoothId);
        }

        public int Save(PersonEditVM model, int userId)
        {
            if (model == null) throw new ArgumentNullException("model");
            ValidateGeography(model);
            string normalized = NormalizeMobile(model.MobileNumber);
            if (String.IsNullOrWhiteSpace(normalized)) throw new InvalidOperationException("A valid mobile number is required.");

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                EnsureModule(cn);
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        if (DuplicateMobile(cn, tx, normalized, model.PersonId))
                            throw new InvalidOperationException("An active person with this mobile number already exists.");

                        int personId = model.PersonId == 0
                            ? InsertPerson(cn, tx, model, normalized, userId)
                            : UpdatePerson(cn, tx, model, normalized, userId);

                        SaveVolunteer(cn, tx, personId, model, userId);
                        tx.Commit();
                        return personId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool Delete(int id, int userId)
        {
            const string sql = @"UPDATE dbo.PersonMaster SET IsActive=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE PersonId=@Id AND IsDeleted=0;
UPDATE dbo.VolunteerProfile SET IsActive=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE PersonId=@Id AND IsDeleted=0;";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                AddNullable(cmd, "@UserId", SqlDbType.NVarChar, 100, UserValue(userId));
                cn.Open();
                EnsureModule(cn);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public IList<SelectListItem> GetGeography(string type, int? parentId, string parentType = null)
        {
            return _geography.GetOptions(type, parentId, parentType);
        }

        private void ValidateGeography(PersonEditVM model)
        {
            if (!_geography.IsValidSelection("District", model.DistrictId, model.StateId, "State"))
                throw new InvalidOperationException("The selected District does not belong to the selected State.");
            if (!_geography.IsValidSelection("ParliamentaryConstituency", model.ParliamentaryConstituencyId, model.StateId, "State"))
                throw new InvalidOperationException("The selected Parliamentary Constituency does not belong to the selected State.");
            if (!_geography.IsValidSelection("AssemblyConstituency", model.AssemblyConstituencyId,
                model.ParliamentaryConstituencyId ?? model.StateId,
                model.ParliamentaryConstituencyId.HasValue ? "ParliamentaryConstituency" : "State"))
                throw new InvalidOperationException("The selected Assembly Constituency is not valid for the selected hierarchy.");
            if (!_geography.IsValidSelection("Tehsil", model.TehsilId, model.DistrictId, "District"))
                throw new InvalidOperationException("The selected Tehsil does not belong to the selected District.");
            if (!_geography.IsValidSelection("Block", model.BlockId, model.DistrictId, "District"))
                throw new InvalidOperationException("The selected Block does not belong to the selected District.");
            if (!_geography.IsValidSelection("GramPanchayat", model.GramPanchayatId, model.BlockId, "Block"))
                throw new InvalidOperationException("The selected Gram Panchayat does not belong to the selected Block.");
            if (!_geography.IsValidSelection("Village", model.VillageId,
                model.GramPanchayatId ?? model.BlockId ?? model.TehsilId,
                model.GramPanchayatId.HasValue ? "GramPanchayat" : model.BlockId.HasValue ? "Block" : "Tehsil"))
                throw new InvalidOperationException("The selected Village is not valid for the selected hierarchy.");
        }

        private IList<PersonListItemVM> ReadRows(SqlConnection cn, string keyword, string status, int? assemblyId, bool volunteersOnly)
        {
            const string sql = @"SELECT TOP 500 p.PersonId,p.FullName,p.MobileNumber,p.Email,p.IsActive,p.CreatedDate,
 CASE WHEN v.VolunteerProfileId IS NULL THEN 0 ELSE 1 END AS IsVolunteer,
 ISNULL(v.PreferredRole,'') AS PreferredRole,
 ISNULL(v.Status,'') AS VolunteerStatus,
 ISNULL(v.VerificationStatus,'') AS VerificationStatus
 FROM dbo.PersonMaster p
 LEFT JOIN dbo.VolunteerProfile v ON v.PersonId=p.PersonId AND v.IsDeleted=0
 WHERE p.IsDeleted=0
 AND (@Keyword IS NULL OR p.FullName LIKE '%'+@Keyword+'%' OR p.MobileNumber LIKE '%'+@Keyword+'%' OR p.Email LIKE '%'+@Keyword+'%')
 AND (@Status IS NULL OR (@Status='Active' AND p.IsActive=1) OR (@Status='Inactive' AND p.IsActive=0) OR v.VerificationStatus=@Status OR v.Status=@Status)
 AND (@AssemblyId IS NULL OR p.AssemblyConstituencyId=@AssemblyId)
 AND (@VolunteersOnly=0 OR v.VolunteerProfileId IS NOT NULL)
 ORDER BY p.CreatedDate DESC";

            using (var cmd = new SqlCommand(sql, cn))
            {
                AddNullable(cmd, "@Keyword", SqlDbType.NVarChar, 100, keyword);
                AddNullable(cmd, "@Status", SqlDbType.NVarChar, 30, status);
                AddNullableInt(cmd, "@AssemblyId", assemblyId);
                cmd.Parameters.Add("@VolunteersOnly", SqlDbType.Bit).Value = volunteersOnly;
                var rows = new List<PersonListItemVM>();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        rows.Add(new PersonListItemVM
                        {
                            PersonId = Convert.ToInt32(r["PersonId"]),
                            PersonCode = "P-" + Convert.ToInt32(r["PersonId"]).ToString("D6"),
                            FullName = Convert.ToString(r["FullName"]),
                            MobileNumber = MaskMobile(Convert.ToString(r["MobileNumber"])),
                            Email = Convert.ToString(r["Email"]),
                            IsActive = Convert.ToBoolean(r["IsActive"]),
                            CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                            IsVolunteer = Convert.ToBoolean(r["IsVolunteer"]),
                            PreferredRole = Convert.ToString(r["PreferredRole"]),
                            VolunteerStatus = Convert.ToString(r["VolunteerStatus"]),
                            VerificationStatus = Convert.ToString(r["VerificationStatus"]),
                            AreaName = String.Empty
                        });
                    }
                }
                return rows;
            }
        }

        private int InsertPerson(SqlConnection cn, SqlTransaction tx, PersonEditVM m, string normalized, int userId)
        {
            const string sql = @"INSERT dbo.PersonMaster
(FullName,MobileNumber,NormalizedMobile,Email,Gender,DateOfBirth,AddressLine,Landmark,PinCode,StateId,DistrictId,ParliamentaryConstituencyId,AssemblyConstituencyId,TehsilId,BlockId,GramPanchayatId,VillageId,WardId,MandalId,SectorId,BoothId,PreferredLanguage,WhatsAppConsent,SmsConsent,EmailConsent,VoiceCallConsent,ConsentDate,ConsentSource,IsOptedOut,OptOutDate,IsActive,IsDeleted,CreatedBy,CreatedDate)
VALUES(@FullName,@Mobile,@NormalizedMobile,@Email,@Gender,@Dob,@Address,@Landmark,@Pin,@StateId,@DistrictId,@PcId,@AcId,@TehsilId,@BlockId,@GpId,@VillageId,@WardId,@MandalId,@SectorId,@BoothId,@Language,@Wa,@Sms,@EmailConsent,@Voice,@ConsentDate,@ConsentSource,@OptOut,@OptOutDate,@Active,0,@UserId,GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                AddPersonParameters(cmd, m, userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int UpdatePerson(SqlConnection cn, SqlTransaction tx, PersonEditVM m, string normalized, int userId)
        {
            const string sql = @"UPDATE dbo.PersonMaster SET
FullName=@FullName,MobileNumber=@Mobile,NormalizedMobile=@NormalizedMobile,Email=@Email,Gender=@Gender,DateOfBirth=@Dob,AddressLine=@Address,Landmark=@Landmark,PinCode=@Pin,
StateId=@StateId,DistrictId=@DistrictId,ParliamentaryConstituencyId=@PcId,AssemblyConstituencyId=@AcId,TehsilId=@TehsilId,BlockId=@BlockId,
GramPanchayatId=@GpId,VillageId=@VillageId,WardId=@WardId,MandalId=@MandalId,SectorId=@SectorId,BoothId=@BoothId,
PreferredLanguage=@Language,WhatsAppConsent=@Wa,SmsConsent=@Sms,EmailConsent=@EmailConsent,VoiceCallConsent=@Voice,
ConsentDate=@ConsentDate,ConsentSource=@ConsentSource,IsOptedOut=@OptOut,OptOutDate=@OptOutDate,
IsActive=@Active,UpdatedBy=@UserId,UpdatedDate=GETDATE()
WHERE PersonId=@Id AND IsDeleted=0";
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                AddPersonParameters(cmd, m, userId);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = m.PersonId;
                if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Person record was not found.");
                return m.PersonId;
            }
        }

        private void SaveVolunteer(SqlConnection cn, SqlTransaction tx, int personId, PersonEditVM m, int userId)
        {
            if (!m.IsVolunteer)
            {
                using (var cmd = new SqlCommand("UPDATE dbo.VolunteerProfile SET IsActive=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE PersonId=@PersonId AND IsDeleted=0", cn, tx))
                {
                    cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                    AddNullable(cmd, "@UserId", SqlDbType.NVarChar, 100, UserValue(userId));
                    cmd.ExecuteNonQuery();
                }
                return;
            }

            const string sql = @"IF EXISTS(SELECT 1 FROM dbo.VolunteerProfile WHERE PersonId=@PersonId AND IsDeleted=0)
UPDATE dbo.VolunteerProfile SET PreferredRole=@Role,Skills=@Skills,AvailableDays=@Days,AvailableTime=@Time,
EmergencyContactName=@EmergencyName,EmergencyContactMobile=@EmergencyMobile,Status=@ApprovalStatus,VerificationStatus=@Verification,
JoiningDate=@Joining,Notes=@Notes,IsActive=1,UpdatedBy=@UserId,UpdatedDate=GETDATE()
WHERE PersonId=@PersonId AND IsDeleted=0
ELSE
INSERT dbo.VolunteerProfile(PersonId,PreferredRole,Skills,AvailableDays,AvailableTime,EmergencyContactName,EmergencyContactMobile,Status,VerificationStatus,JoiningDate,Notes,IsActive,IsDeleted,CreatedBy,CreatedDate)
VALUES(@PersonId,@Role,@Skills,@Days,@Time,@EmergencyName,@EmergencyMobile,@ApprovalStatus,@Verification,@Joining,@Notes,1,0,@UserId,GETDATE())";

            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                AddNullable(cmd, "@Role", SqlDbType.NVarChar, 100, m.PreferredRole);
                AddNullable(cmd, "@Skills", SqlDbType.NVarChar, 500, m.Skills);
                AddNullable(cmd, "@Days", SqlDbType.NVarChar, 150, m.AvailableDays);
                AddNullable(cmd, "@Time", SqlDbType.NVarChar, 100, m.AvailableTime);
                AddNullable(cmd, "@EmergencyName", SqlDbType.NVarChar, 150, m.EmergencyContactName);
                AddNullable(cmd, "@EmergencyMobile", SqlDbType.VarChar, 15, m.EmergencyContactMobile);
                AddNullable(cmd, "@ApprovalStatus", SqlDbType.NVarChar, 30, String.IsNullOrWhiteSpace(m.ApprovalStatus) ? "Pending" : m.ApprovalStatus);
                AddNullable(cmd, "@Verification", SqlDbType.NVarChar, 30, String.IsNullOrWhiteSpace(m.VerificationStatus) ? "Pending" : m.VerificationStatus);
                cmd.Parameters.Add("@Joining", SqlDbType.Date).Value = m.JoiningDate.HasValue ? (object)m.JoiningDate.Value : DBNull.Value;
                AddNullable(cmd, "@Notes", SqlDbType.NVarChar, 1000, m.Notes);
                AddNullable(cmd, "@UserId", SqlDbType.NVarChar, 100, UserValue(userId));
                cmd.ExecuteNonQuery();
            }
        }

        private static void AddPersonParameters(SqlCommand cmd, PersonEditVM m, int userId)
        {
            AddNullable(cmd, "@FullName", SqlDbType.NVarChar, 150, m.FullName);
            AddNullable(cmd, "@Mobile", SqlDbType.VarChar, 15, m.MobileNumber);
            AddNullable(cmd, "@NormalizedMobile", SqlDbType.VarChar, 15, NormalizeMobile(m.MobileNumber));
            AddNullable(cmd, "@Email", SqlDbType.NVarChar, 200, m.Email);
            AddNullable(cmd, "@Gender", SqlDbType.NVarChar, 20, m.Gender);
            cmd.Parameters.Add("@Dob", SqlDbType.Date).Value = m.DateOfBirth.HasValue ? (object)m.DateOfBirth.Value : DBNull.Value;
            AddNullable(cmd, "@Address", SqlDbType.NVarChar, 500, m.AddressLine);
            AddNullable(cmd, "@Landmark", SqlDbType.NVarChar, 200, m.Landmark);
            AddNullable(cmd, "@Pin", SqlDbType.VarChar, 10, m.PinCode);
            AddNullableInt(cmd, "@StateId", m.StateId);
            AddNullableInt(cmd, "@DistrictId", m.DistrictId);
            AddNullableInt(cmd, "@PcId", m.ParliamentaryConstituencyId);
            AddNullableInt(cmd, "@AcId", m.AssemblyConstituencyId);
            AddNullableInt(cmd, "@TehsilId", m.TehsilId);
            AddNullableInt(cmd, "@BlockId", m.BlockId);
            AddNullableInt(cmd, "@GpId", m.GramPanchayatId);
            AddNullableInt(cmd, "@VillageId", m.VillageId);
            AddNullableInt(cmd, "@WardId", m.WardId);
            AddNullableInt(cmd, "@MandalId", m.MandalId);
            AddNullableInt(cmd, "@SectorId", m.SectorId);
            AddNullableInt(cmd, "@BoothId", m.BoothId);
            AddNullable(cmd, "@Language", SqlDbType.NVarChar, 20, m.PreferredLanguage);
            cmd.Parameters.Add("@Wa", SqlDbType.Bit).Value = m.WhatsAppConsent;
            cmd.Parameters.Add("@Sms", SqlDbType.Bit).Value = m.SmsConsent;
            cmd.Parameters.Add("@EmailConsent", SqlDbType.Bit).Value = m.EmailConsent;
            cmd.Parameters.Add("@Voice", SqlDbType.Bit).Value = m.VoiceConsent;
            bool anyConsent = m.WhatsAppConsent || m.SmsConsent || m.EmailConsent || m.VoiceConsent;
            cmd.Parameters.Add("@ConsentDate", SqlDbType.DateTime).Value = anyConsent ? (object)DateTime.Now : DBNull.Value;
            AddNullable(cmd, "@ConsentSource", SqlDbType.NVarChar, 50, m.ConsentSource);
            cmd.Parameters.Add("@OptOut", SqlDbType.Bit).Value = m.IsOptedOut;
            cmd.Parameters.Add("@OptOutDate", SqlDbType.DateTime).Value = m.IsOptedOut ? (object)DateTime.Now : DBNull.Value;
            cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = m.IsActive;
            AddNullable(cmd, "@UserId", SqlDbType.NVarChar, 100, UserValue(userId));
        }

        private PersonEditVM Map(IDataRecord r)
        {
            return new PersonEditVM
            {
                PersonId = I(r, "PersonId"),
                VolunteerProfileId = NI(r, "VolunteerProfileId"),
                FullName = S(r, "FullName"),
                MobileNumber = S(r, "MobileNumber"),
                Email = S(r, "Email"),
                Gender = S(r, "Gender"),
                DateOfBirth = D(r, "DateOfBirth"),
                AddressLine = S(r, "AddressLine"),
                Landmark = S(r, "Landmark"),
                PinCode = S(r, "PinCode"),
                PreferredLanguage = S(r, "PreferredLanguage"),
                StateId = NI(r, "StateId"),
                DistrictId = NI(r, "DistrictId"),
                ParliamentaryConstituencyId = NI(r, "ParliamentaryConstituencyId"),
                AssemblyConstituencyId = NI(r, "AssemblyConstituencyId"),
                TehsilId = NI(r, "TehsilId"),
                BlockId = NI(r, "BlockId"),
                GramPanchayatId = NI(r, "GramPanchayatId"),
                VillageId = NI(r, "VillageId"),
                WardId = NI(r, "WardId"),
                MandalId = NI(r, "MandalId"),
                SectorId = NI(r, "SectorId"),
                BoothId = NI(r, "BoothId"),
                WhatsAppConsent = B(r, "WhatsAppConsent"),
                SmsConsent = B(r, "SmsConsent"),
                EmailConsent = B(r, "EmailConsent"),
                VoiceConsent = B(r, "VoiceCallConsent"),
                ConsentSource = S(r, "ConsentSource"),
                IsOptedOut = B(r, "IsOptedOut"),
                VerificationStatus = S(r, "VerificationStatus"),
                IsActive = B(r, "IsActive"),
                IsVolunteer = NI(r, "VolunteerProfileId").HasValue,
                PreferredRole = S(r, "PreferredRole"),
                Skills = S(r, "Skills"),
                AvailableDays = S(r, "AvailableDays"),
                AvailableTime = S(r, "AvailableTime"),
                EmergencyContactName = S(r, "EmergencyContactName"),
                EmergencyContactMobile = S(r, "EmergencyContactMobile"),
                ApprovalStatus = S(r, "VolunteerStatus"),
                JoiningDate = D(r, "JoiningDate"),
                Notes = S(r, "Notes")
            };
        }

        private bool DuplicateMobile(SqlConnection cn, SqlTransaction tx, string mobile, int excludeId)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.PersonMaster WHERE IsDeleted=0 AND NormalizedMobile=@Mobile AND PersonId<>@Id", cn, tx))
            {
                cmd.Parameters.Add("@Mobile", SqlDbType.VarChar, 15).Value = mobile;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = excludeId;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool TableExists(SqlConnection cn, string table)
        {
            using (var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@Name,'U') IS NULL THEN 0 ELSE 1 END", cn))
            {
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 256).Value = "dbo." + table;
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static bool ColumnExists(SqlConnection cn, string table, string column)
        {
            using (var cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@TableName,@ColumnName) IS NULL THEN 0 ELSE 1 END", cn))
            {
                cmd.Parameters.Add("@TableName", SqlDbType.NVarChar, 256).Value = "dbo." + table;
                cmd.Parameters.Add("@ColumnName", SqlDbType.NVarChar, 128).Value = column;
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static void EnsureModule(SqlConnection cn)
        {
            if (!TableExists(cn, "PersonMaster") || !TableExists(cn, "VolunteerProfile"))
                throw new InvalidOperationException("People module database tables are missing. Run 01_PeopleVolunteerModuleUpgrade.sql first.");
        }

        private static int Scalar(SqlConnection cn, string sql)
        {
            using (var cmd = new SqlCommand(sql, cn)) return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static string NormalizeMobile(string value)
        {
            return new string((value ?? String.Empty).Where(Char.IsDigit).ToArray());
        }

        private static string UserValue(int userId)
        {
            return userId > 0 ? Convert.ToString(userId) : null;
        }

        private static string MaskMobile(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length <= 4) return value;
            return new string('*', value.Length - 4) + value.Substring(value.Length - 4);
        }

        private static void AddNullable(SqlCommand cmd, string name, SqlDbType type, int size, string value)
        {
            var parameter = cmd.Parameters.Add(name, type, size);
            parameter.Value = String.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static void AddNullableInt(SqlCommand cmd, string name, int? value)
        {
            cmd.Parameters.Add(name, SqlDbType.Int).Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static int I(IDataRecord r, string name) { return Convert.ToInt32(r[name]); }
        private static int? NI(IDataRecord r, string name) { return r[name] == DBNull.Value ? (int?)null : Convert.ToInt32(r[name]); }
        private static string S(IDataRecord r, string name) { return r[name] == DBNull.Value ? null : Convert.ToString(r[name]); }
        private static bool B(IDataRecord r, string name) { return r[name] != DBNull.Value && Convert.ToBoolean(r[name]); }
        private static DateTime? D(IDataRecord r, string name) { return r[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[name]); }
    }
}
