using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionCampaignMaster;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class ElectionCampaignMasterService
    {
        private readonly string _connectionString;

        public ElectionCampaignMasterService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                _connectionString = db.Database.Connection.ConnectionString;
            }
        }

        public ElectionCampaignDashboardVM GetDashboard(string keyword = null)
        {
            var model = new ElectionCampaignDashboardVM();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.usp_ElectionCampaignMaster_Dashboard", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Keyword", (object)keyword ?? DBNull.Value);
                cn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) { model.TotalElections = r.GetInt32(0); model.ActiveElections = r.GetInt32(1); model.TotalCampaigns = r.GetInt32(2); model.ActiveCampaigns = r.GetInt32(3); }
                    if (r.NextResult()) while (r.Read()) model.Elections.Add(MapElection(r));
                    if (r.NextResult()) while (r.Read()) model.Campaigns.Add(MapCampaign(r));
                }
            }
            return model;
        }

        public ElectionMasterVM GetElection(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT * FROM dbo.ElectionMaster WHERE ElectionId=@Id AND IsDeleted=0", cn))
            { cmd.Parameters.AddWithValue("@Id", id); cn.Open(); using (var r = cmd.ExecuteReader()) return r.Read() ? MapElection(r) : null; }
        }

        public CampaignMasterVM GetCampaign(int id)
        {
            const string sql = @"SELECT c.*, e.ElectionName FROM dbo.CampaignMaster c INNER JOIN dbo.ElectionMaster e ON e.ElectionId=c.ElectionId WHERE c.CampaignMasterId=@Id AND c.IsDeleted=0";
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            { cmd.Parameters.AddWithValue("@Id", id); cn.Open(); using (var r = cmd.ExecuteReader()) return r.Read() ? MapCampaign(r) : null; }
        }

        public IList<System.Web.Mvc.SelectListItem> GetElectionOptions(int? selected = null)
        {
            var items = new List<System.Web.Mvc.SelectListItem>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT ElectionId,ElectionName FROM dbo.ElectionMaster WHERE IsDeleted=0 AND IsActive=1 ORDER BY ElectionYear DESC,ElectionName", cn))
            { cn.Open(); using (var r = cmd.ExecuteReader()) while (r.Read()) items.Add(new System.Web.Mvc.SelectListItem { Value = Convert.ToString(r[0]), Text = Convert.ToString(r[1]), Selected = selected == Convert.ToInt32(r[0]) }); }
            return items;
        }

        public bool ElectionNameExists(string name, int excludeId)
        {
            return Exists("SELECT COUNT(1) FROM dbo.ElectionMaster WHERE IsDeleted=0 AND ElectionName=@Value AND ElectionId<>@Id", name, excludeId);
        }
        public bool CampaignCodeExists(string code, int excludeId)
        {
            return Exists("SELECT COUNT(1) FROM dbo.CampaignMaster WHERE IsDeleted=0 AND CampaignCode=@Value AND CampaignMasterId<>@Id", code, excludeId);
        }

        public int SaveElection(ElectionMasterVM m, int userId)
        {
            const string sql = @"INSERT dbo.ElectionMaster(ElectionName,ElectionType,ElectionYear,StateName,DistrictName,NotificationDate,NominationStartDate,NominationEndDate,PollingDate,CountingDate,Status,Description,IsActive,CreatedBy)
VALUES(@ElectionName,@ElectionType,@ElectionYear,@StateName,@DistrictName,@NotificationDate,@NominationStartDate,@NominationEndDate,@PollingDate,@CountingDate,@Status,@Description,@IsActive,@UserId); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return Convert.ToInt32(ExecuteScalar(sql, ElectionParameters(m, userId)));
        }
        public void UpdateElection(ElectionMasterVM m, int userId)
        {
            const string sql = @"UPDATE dbo.ElectionMaster SET ElectionName=@ElectionName,ElectionType=@ElectionType,ElectionYear=@ElectionYear,StateName=@StateName,DistrictName=@DistrictName,NotificationDate=@NotificationDate,NominationStartDate=@NominationStartDate,NominationEndDate=@NominationEndDate,PollingDate=@PollingDate,CountingDate=@CountingDate,Status=@Status,Description=@Description,IsActive=@IsActive,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE ElectionId=@ElectionId AND IsDeleted=0";
            var p = ElectionParameters(m, userId); p.Add(new SqlParameter("@ElectionId", m.ElectionId)); ExecuteNonQuery(sql, p);
        }
        public void DeleteElection(int id, int userId) { ExecuteNonQuery("UPDATE dbo.ElectionMaster SET IsDeleted=1,IsActive=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE ElectionId=@Id AND NOT EXISTS(SELECT 1 FROM dbo.CampaignMaster WHERE ElectionId=@Id AND IsDeleted=0)", new List<SqlParameter> { new SqlParameter("@Id", id), new SqlParameter("@UserId", userId) }); }

        public int SaveCampaign(CampaignMasterVM m, int userId)
        {
            if (m.IsPrimary) ClearPrimary(m.ElectionId);
            const string sql = @"INSERT dbo.CampaignMaster(ElectionId,CampaignName,CampaignCode,CandidateName,ConstituencyName,ConstituencyNumber,CampaignOwner,StartDate,EndDate,Status,ProgressPercent,Goals,Description,IsPrimary,IsActive,CreatedBy)
VALUES(@ElectionId,@CampaignName,@CampaignCode,@CandidateName,@ConstituencyName,@ConstituencyNumber,@CampaignOwner,@StartDate,@EndDate,@Status,@ProgressPercent,@Goals,@Description,@IsPrimary,@IsActive,@UserId); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            int campaignMasterId = Convert.ToInt32(ExecuteScalar(sql, CampaignParameters(m, userId)));
            SynchronizeOperationalCampaign(campaignMasterId, userId);
            return campaignMasterId;
        }
        public void UpdateCampaign(CampaignMasterVM m, int userId)
        {
            if (m.IsPrimary) ClearPrimary(m.ElectionId, m.CampaignMasterId);
            const string sql = @"UPDATE dbo.CampaignMaster SET ElectionId=@ElectionId,CampaignName=@CampaignName,CampaignCode=@CampaignCode,CandidateName=@CandidateName,ConstituencyName=@ConstituencyName,ConstituencyNumber=@ConstituencyNumber,CampaignOwner=@CampaignOwner,StartDate=@StartDate,EndDate=@EndDate,Status=@Status,ProgressPercent=@ProgressPercent,Goals=@Goals,Description=@Description,IsPrimary=@IsPrimary,IsActive=@IsActive,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE CampaignMasterId=@CampaignMasterId AND IsDeleted=0";
            var p = CampaignParameters(m, userId); p.Add(new SqlParameter("@CampaignMasterId", m.CampaignMasterId)); ExecuteNonQuery(sql, p);
            SynchronizeOperationalCampaign(m.CampaignMasterId, userId);
        }
        public void DeleteCampaign(int id, int userId) { ExecuteNonQuery("UPDATE dbo.CampaignMaster SET IsDeleted=1,IsActive=0,IsPrimary=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE CampaignMasterId=@Id; UPDATE operational SET operational.IsActive=0,operational.Status='Cancelled',operational.UpdatedBy=@UserId,operational.UpdatedDate=GETDATE() FROM dbo.ElectionCampaign operational INNER JOIN dbo.CampaignContextMap map ON map.OperationalCampaignId=operational.CampaignId WHERE map.CampaignMasterId=@Id; UPDATE dbo.CampaignContextMap SET IsActive=0,UpdatedDate=GETDATE() WHERE CampaignMasterId=@Id", new List<SqlParameter> { new SqlParameter("@Id", id), new SqlParameter("@UserId", userId) }); }
        public void SetCampaignStatus(int id, string status, int userId) { ExecuteNonQuery("UPDATE dbo.CampaignMaster SET Status=@Status,IsActive=CASE WHEN @Status IN ('Closed','Cancelled') THEN 0 ELSE IsActive END,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE CampaignMasterId=@Id AND IsDeleted=0", new List<SqlParameter> { new SqlParameter("@Id", id), new SqlParameter("@Status", status), new SqlParameter("@UserId", userId) }); SynchronizeOperationalCampaign(id, userId); }

        private void SynchronizeOperationalCampaign(int campaignMasterId, int userId)
        {
            const string sql = @"
DECLARE @OperationalId INT =
(
    SELECT OperationalCampaignId
    FROM dbo.CampaignContextMap
    WHERE CampaignMasterId=@CampaignMasterId
);

IF @OperationalId IS NULL
BEGIN
    INSERT dbo.ElectionCampaign
    (
        CampaignName,ElectionType,StateId,StartDate,EndDate,Status,
        Description,IsActive,CreatedDate,CreatedBy
    )
    SELECT c.CampaignName,e.ElectionType,NULL,c.StartDate,c.EndDate,c.Status,
           c.Description,c.IsActive,GETDATE(),@UserId
    FROM dbo.CampaignMaster c
    INNER JOIN dbo.ElectionMaster e ON e.ElectionId=c.ElectionId
    WHERE c.CampaignMasterId=@CampaignMasterId AND c.IsDeleted=0;

    SET @OperationalId=CONVERT(INT,SCOPE_IDENTITY());

    IF @OperationalId IS NOT NULL
        INSERT dbo.CampaignContextMap
        (CampaignMasterId,OperationalCampaignId,IsActive,CreatedDate)
        VALUES(@CampaignMasterId,@OperationalId,1,GETDATE());
END
ELSE
BEGIN
    UPDATE operational
    SET CampaignName=c.CampaignName,
        ElectionType=e.ElectionType,
        StartDate=c.StartDate,
        EndDate=c.EndDate,
        Status=c.Status,
        Description=c.Description,
        IsActive=c.IsActive,
        UpdatedDate=GETDATE(),
        UpdatedBy=@UserId
    FROM dbo.ElectionCampaign operational
    INNER JOIN dbo.CampaignContextMap map
        ON map.OperationalCampaignId=operational.CampaignId
    INNER JOIN dbo.CampaignMaster c
        ON c.CampaignMasterId=map.CampaignMasterId
    INNER JOIN dbo.ElectionMaster e
        ON e.ElectionId=c.ElectionId
    WHERE c.CampaignMasterId=@CampaignMasterId AND c.IsDeleted=0;

    UPDATE dbo.CampaignContextMap
    SET IsActive=1,UpdatedDate=GETDATE()
    WHERE CampaignMasterId=@CampaignMasterId;
END;";
            ExecuteNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@CampaignMasterId", campaignMasterId),
                new SqlParameter("@UserId", userId)
            });
        }

        private void ClearPrimary(int electionId, int excludeId = 0) { ExecuteNonQuery("UPDATE dbo.CampaignMaster SET IsPrimary=0 WHERE ElectionId=@ElectionId AND CampaignMasterId<>@ExcludeId", new List<SqlParameter> { new SqlParameter("@ElectionId", electionId), new SqlParameter("@ExcludeId", excludeId) }); }
        private bool Exists(string sql, string value, int id) { return Convert.ToInt32(ExecuteScalar(sql, new List<SqlParameter> { new SqlParameter("@Value", value), new SqlParameter("@Id", id) })) > 0; }
        private object ExecuteScalar(string sql, IList<SqlParameter> p) { using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand(sql, cn)) { foreach (var x in p) cmd.Parameters.Add(x); cn.Open(); return cmd.ExecuteScalar(); } }
        private void ExecuteNonQuery(string sql, IList<SqlParameter> p) { using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand(sql, cn)) { foreach (var x in p) cmd.Parameters.Add(x); cn.Open(); cmd.ExecuteNonQuery(); } }
        private static object Db(object v) { return v ?? DBNull.Value; }
        private static List<SqlParameter> ElectionParameters(ElectionMasterVM m, int uid) { return new List<SqlParameter> { new SqlParameter("@ElectionName", m.ElectionName), new SqlParameter("@ElectionType", m.ElectionType), new SqlParameter("@ElectionYear", m.ElectionYear), new SqlParameter("@StateName", Db(m.StateName)), new SqlParameter("@DistrictName", Db(m.DistrictName)), new SqlParameter("@NotificationDate", Db(m.NotificationDate)), new SqlParameter("@NominationStartDate", Db(m.NominationStartDate)), new SqlParameter("@NominationEndDate", Db(m.NominationEndDate)), new SqlParameter("@PollingDate", Db(m.PollingDate)), new SqlParameter("@CountingDate", Db(m.CountingDate)), new SqlParameter("@Status", m.Status), new SqlParameter("@Description", Db(m.Description)), new SqlParameter("@IsActive", m.IsActive), new SqlParameter("@UserId", uid) }; }
        private static List<SqlParameter> CampaignParameters(CampaignMasterVM m, int uid) { return new List<SqlParameter> { new SqlParameter("@ElectionId", m.ElectionId), new SqlParameter("@CampaignName", m.CampaignName), new SqlParameter("@CampaignCode", m.CampaignCode), new SqlParameter("@CandidateName", Db(m.CandidateName)), new SqlParameter("@ConstituencyName", Db(m.ConstituencyName)), new SqlParameter("@ConstituencyNumber", Db(m.ConstituencyNumber)), new SqlParameter("@CampaignOwner", Db(m.CampaignOwner)), new SqlParameter("@StartDate", m.StartDate), new SqlParameter("@EndDate", Db(m.EndDate)), new SqlParameter("@Status", m.Status), new SqlParameter("@ProgressPercent", m.ProgressPercent), new SqlParameter("@Goals", Db(m.Goals)), new SqlParameter("@Description", Db(m.Description)), new SqlParameter("@IsPrimary", m.IsPrimary), new SqlParameter("@IsActive", m.IsActive), new SqlParameter("@UserId", uid) }; }
        private static ElectionMasterVM MapElection(IDataRecord r) { return new ElectionMasterVM { ElectionId = Convert.ToInt32(r["ElectionId"]), ElectionName = Convert.ToString(r["ElectionName"]), ElectionType = Convert.ToString(r["ElectionType"]), ElectionYear = Convert.ToInt32(r["ElectionYear"]), StateName = Convert.ToString(r["StateName"]), DistrictName = Convert.ToString(r["DistrictName"]), NotificationDate = r["NotificationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NotificationDate"]), NominationStartDate = r["NominationStartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NominationStartDate"]), NominationEndDate = r["NominationEndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NominationEndDate"]), PollingDate = r["PollingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["PollingDate"]), CountingDate = r["CountingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["CountingDate"]), Status = Convert.ToString(r["Status"]), Description = Convert.ToString(r["Description"]), IsActive = Convert.ToBoolean(r["IsActive"]) }; }
        private static CampaignMasterVM MapCampaign(IDataRecord r) { return new CampaignMasterVM { CampaignMasterId = Convert.ToInt32(r["CampaignMasterId"]), ElectionId = Convert.ToInt32(r["ElectionId"]), ElectionName = Convert.ToString(r["ElectionName"]), CampaignName = Convert.ToString(r["CampaignName"]), CampaignCode = Convert.ToString(r["CampaignCode"]), CandidateName = Convert.ToString(r["CandidateName"]), ConstituencyName = Convert.ToString(r["ConstituencyName"]), ConstituencyNumber = Convert.ToString(r["ConstituencyNumber"]), CampaignOwner = Convert.ToString(r["CampaignOwner"]), StartDate = Convert.ToDateTime(r["StartDate"]), EndDate = r["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["EndDate"]), Status = Convert.ToString(r["Status"]), ProgressPercent = Convert.ToInt32(r["ProgressPercent"]), Goals = Convert.ToString(r["Goals"]), Description = Convert.ToString(r["Description"]), IsPrimary = Convert.ToBoolean(r["IsPrimary"]), IsActive = Convert.ToBoolean(r["IsActive"]) }; }
    }
}
