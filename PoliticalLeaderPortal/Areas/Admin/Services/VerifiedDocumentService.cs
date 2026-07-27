using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;
using PoliticalLeaderPortal.Areas.Admin.ViewModels;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class VerifiedDocumentService
    {
        private readonly string _connectionString;

        public VerifiedDocumentService()
        {
            var entity = new EntityConnectionStringBuilder(
                ConfigurationManager.ConnectionStrings["PoliticalLeaderPortalDbEntities1"].ConnectionString);
            _connectionString = entity.ProviderConnectionString;
        }

        public IList<VerifiedDocumentVM> GetAll()
        {
            const string sql = @"
SELECT d.*, c.CampaignName
FROM dbo.VerifiedDocument d
LEFT JOIN dbo.ElectionCampaign c ON c.CampaignId = d.CampaignId
ORDER BY d.CreatedOn DESC;";
            return Query(sql, null);
        }

        public IEnumerable<SelectListItem> GetCampaigns()
        {
            var result = new List<SelectListItem>();
            const string sql = @"
SELECT CampaignId, CampaignName
FROM dbo.ElectionCampaign
WHERE ISNULL(IsActive, 1) = 1
ORDER BY CampaignName;";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        result.Add(new SelectListItem
                        {
                            Value = Convert.ToString(reader["CampaignId"]),
                            Text = Convert.ToString(reader["CampaignName"])
                        });
            }
            return result;
        }

        public VerifiedDocumentVM GetById(int id)
        {
            const string sql = @"
SELECT d.*, c.CampaignName
FROM dbo.VerifiedDocument d
LEFT JOIN dbo.ElectionCampaign c ON c.CampaignId = d.CampaignId
WHERE d.VerifiedDocumentId = @Id;";
            var rows = Query(sql, new SqlParameter("@Id", id));
            return rows.Count == 0 ? null : rows[0];
        }

        public VerifiedDocumentVM GetByCode(string code)
        {
            const string sql = @"
SELECT d.*, c.CampaignName
FROM dbo.VerifiedDocument d
LEFT JOIN dbo.ElectionCampaign c ON c.CampaignId = d.CampaignId
WHERE d.VerificationCode = @Code;";
            var rows = Query(sql, new SqlParameter("@Code", code ?? String.Empty));
            return rows.Count == 0 ? null : rows[0];
        }

        public int Create(VerifiedDocumentVM model, string createdBy)
        {
            string prefix = model.DocumentType == "DigitalCard" ? "CARD" : "LTR";
            string number = prefix + "-" + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + "-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            string code = Guid.NewGuid().ToString("N");

            const string sql = @"
INSERT dbo.VerifiedDocument
(DocumentNumber, VerificationCode, DocumentType, RecipientName, RecipientReference,
 RecipientRole, RecipientPhotoPath, CampaignId, Subject, BodyText, IssueDate, ExpiryDate, Status,
 IssuedByName, IssuedByDesignation, CreatedBy)
VALUES
(@Number, @Code, @Type, @Name, @Reference, @Role, @PhotoPath, @CampaignId, @Subject,
 @Body, @IssueDate, @ExpiryDate, N'Active', @Issuer, @IssuerRole, @CreatedBy);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Number", number);
                command.Parameters.AddWithValue("@Code", code);
                command.Parameters.AddWithValue("@Type", model.DocumentType);
                AddNullable(command, "@Name", model.RecipientName);
                AddNullable(command, "@Reference", model.RecipientReference);
                AddNullable(command, "@Role", model.RecipientRole);
                AddNullable(command, "@PhotoPath", model.RecipientPhotoPath);
                AddNullable(command, "@CampaignId", model.CampaignId);
                AddNullable(command, "@Subject", model.Subject);
                AddNullable(command, "@Body", model.BodyText);
                command.Parameters.AddWithValue("@IssueDate", model.IssueDate.Date);
                AddNullable(command, "@ExpiryDate", model.ExpiryDate.HasValue ? (object)model.ExpiryDate.Value.Date : null);
                AddNullable(command, "@Issuer", model.IssuedByName);
                AddNullable(command, "@IssuerRole", model.IssuedByDesignation);
                AddNullable(command, "@CreatedBy", createdBy);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public void Revoke(int id, string reason, string revokedBy)
        {
            const string sql = @"
UPDATE dbo.VerifiedDocument
SET Status=N'Revoked', RevokedOn=SYSDATETIME(), RevokedBy=@By, RevocationReason=@Reason
WHERE VerifiedDocumentId=@Id AND Status=N'Active';";
            Execute(sql,
                new SqlParameter("@Id", id),
                new SqlParameter("@By", (object)revokedBy ?? DBNull.Value),
                new SqlParameter("@Reason", (object)reason ?? DBNull.Value));
        }

        private IList<VerifiedDocumentVM> Query(string sql, params SqlParameter[] parameters)
        {
            var result = new List<VerifiedDocumentVM>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new VerifiedDocumentVM
                        {
                            VerifiedDocumentId = Convert.ToInt32(reader["VerifiedDocumentId"]),
                            DocumentNumber = Convert.ToString(reader["DocumentNumber"]),
                            VerificationCode = Convert.ToString(reader["VerificationCode"]),
                            DocumentType = Convert.ToString(reader["DocumentType"]),
                            RecipientName = Convert.ToString(reader["RecipientName"]),
                            RecipientReference = DbString(reader["RecipientReference"]),
                            RecipientRole = DbString(reader["RecipientRole"]),
                            RecipientPhotoPath = DbString(reader["RecipientPhotoPath"]),
                            CampaignId = DbInt(reader["CampaignId"]),
                            CampaignName = DbString(reader["CampaignName"]),
                            Subject = DbString(reader["Subject"]),
                            BodyText = DbString(reader["BodyText"]),
                            IssueDate = Convert.ToDateTime(reader["IssueDate"]),
                            ExpiryDate = DbDate(reader["ExpiryDate"]),
                            Status = Convert.ToString(reader["Status"]),
                            IssuedByName = DbString(reader["IssuedByName"]),
                            IssuedByDesignation = DbString(reader["IssuedByDesignation"]),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                        });
                    }
                }
            }
            return result;
        }

        private void Execute(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void AddNullable(SqlCommand command, string name, object value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        private static string DbString(object value) { return value == DBNull.Value ? null : Convert.ToString(value); }
        private static int? DbInt(object value) { return value == DBNull.Value ? (int?)null : Convert.ToInt32(value); }
        private static DateTime? DbDate(object value) { return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value); }
    }
}
