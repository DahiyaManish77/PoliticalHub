using PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class ConstituencyMasterService
    {
        private readonly string _connectionString;
        private readonly GeographyLookupService _geography = new GeographyLookupService();
        private static readonly IDictionary<string, string> Tables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"State", "StateMaster"}, {"District", "DistrictMaster"}, {"Tehsil", "TehsilMaster"}, {"Block", "BlockMaster"},
            {"ParliamentaryConstituency", "ParliamentaryConstituencyMaster"}, {"AssemblyConstituency", "AssemblyConstituencyMaster"},
            {"Ward", "WardMaster"}, {"GramPanchayat", "GramPanchayatMaster"}, {"Village", "VillageMaster"}
        };

        public ConstituencyMasterService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public GeographyDashboardVM GetDashboard(string entityType, string keyword, int page = 1)
        {
            entityType = Tables.ContainsKey(entityType ?? "") ? entityType : "State";
            const int pageSize = 50;
            page = Math.Max(1, page);
            var vm = new GeographyDashboardVM { EntityType = entityType, Keyword = keyword, Page = page, PageSize = pageSize };
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                vm.States = Count(cn, "StateMaster"); vm.Districts = Count(cn, "DistrictMaster"); vm.Tehsils = Count(cn, "TehsilMaster");
                vm.Blocks = Count(cn, "BlockMaster"); vm.ParliamentaryConstituencies = Count(cn, "ParliamentaryConstituencyMaster");
                vm.AssemblyConstituencies = Count(cn, "AssemblyConstituencyMaster"); vm.Wards = Count(cn, "WardMaster");
                vm.GramPanchayats = Count(cn, "GramPanchayatMaster"); vm.Villages = Count(cn, "VillageMaster");
                vm.TotalRows = CountRows(cn, entityType, keyword);
                vm.TotalPages = Math.Max(1, (int)Math.Ceiling(vm.TotalRows / (double)pageSize));
                vm.Page = Math.Min(vm.Page, vm.TotalPages);
                vm.Rows = GetRows(cn, entityType, keyword, vm.Page, pageSize);
            }
            return vm;
        }

        public GeographyEditVM Get(int id, string entityType)
        {
            EnsureEntity(entityType); var table = Tables[entityType];
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT * FROM dbo." + table + " WHERE " + IdColumn(entityType) + "=@Id AND IsDeleted=0", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id; cn.Open();
                using (var r = cmd.ExecuteReader()) return r.Read() ? MapEdit(r, entityType) : null;
            }
        }

        public void Save(GeographyEditVM m, int userId)
        {
            EnsureEntity(m.EntityType);
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open(); var columns = Columns(m.EntityType);
                var names = new List<string> { "Code", "NameEnglish", "NameHindi", "IsActive", "CreatedBy" }; names.AddRange(columns);
                var sql = "INSERT dbo." + Tables[m.EntityType] + "(" + string.Join(",", names) + ") VALUES(@Code,@NameEnglish,@NameHindi,@IsActive,@UserId" + string.Concat(columns.Select(x => ",@" + x)) + ")";
                using (var cmd = new SqlCommand(sql, cn)) { AddParameters(cmd, m, userId, columns); cmd.ExecuteNonQuery(); }
            }
        }

        public void Update(GeographyEditVM m, int userId)
        {
            EnsureEntity(m.EntityType); var columns = Columns(m.EntityType);
            var sets = new List<string> { "Code=@Code", "NameEnglish=@NameEnglish", "NameHindi=@NameHindi", "IsActive=@IsActive", "UpdatedBy=@UserId", "UpdatedDate=GETDATE()" }; sets.AddRange(columns.Select(x => x + "=@" + x));
            var sql = "UPDATE dbo." + Tables[m.EntityType] + " SET " + string.Join(",", sets) + " WHERE " + IdColumn(m.EntityType) + "=@Id AND IsDeleted=0";
            using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand(sql, cn)) { AddParameters(cmd, m, userId, columns); cmd.Parameters.Add("@Id", SqlDbType.Int).Value = m.Id; cn.Open(); cmd.ExecuteNonQuery(); }
        }

        public bool Delete(int id, string entityType, int userId)
        {
            EnsureEntity(entityType);
            if (HasActiveChildren(id, entityType)) return false;
            var sql = "UPDATE dbo." + Tables[entityType] + " SET IsDeleted=1,IsActive=0,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE " + IdColumn(entityType) + "=@Id";
            using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand(sql, cn)) { cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id; cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId; cn.Open(); return cmd.ExecuteNonQuery() > 0; }
        }

        public bool Exists(string entityType, string code, int excludeId)
        {
            EnsureEntity(entityType); var sql = "SELECT COUNT(1) FROM dbo." + Tables[entityType] + " WHERE IsDeleted=0 AND Code=@Code AND " + IdColumn(entityType) + "<>@Id";
            using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand(sql, cn)) { cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code; cmd.Parameters.Add("@Id", SqlDbType.Int).Value = excludeId; cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()) > 0; }
        }

        public IList<string> ValidateHierarchy(GeographyEditVM model)
        {
            EnsureEntity(model.EntityType);
            var errors = new List<string>();

            if (model.EntityType != "State" && !model.StateId.HasValue)
                errors.Add("State is required.");
            if (new[] { "Tehsil", "Block", "Ward", "GramPanchayat", "Village" }.Contains(model.EntityType) && !model.DistrictId.HasValue)
                errors.Add("District is required.");
            if ((model.EntityType == "GramPanchayat" || model.EntityType == "Village") && !model.BlockId.HasValue)
                errors.Add("Development Block is required.");
            if (model.EntityType == "Village" && !model.TehsilId.HasValue)
                errors.Add("Tehsil / Sub-District is required.");
            if (model.EntityType == "AssemblyConstituency" && !model.ParliamentaryConstituencyId.HasValue)
                errors.Add("Parliamentary Constituency is required.");
            if (model.EntityType == "Ward" && !model.AssemblyConstituencyId.HasValue)
                errors.Add("Assembly Constituency is required.");

            if (errors.Count > 0)
                return errors;

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                if (model.DistrictId.HasValue && !Related(cn, "DistrictMaster", "DistrictId", model.DistrictId.Value, "StateId", model.StateId))
                    errors.Add("The selected District does not belong to the selected State.");
                if (model.TehsilId.HasValue && !Related(cn, "TehsilMaster", "TehsilId", model.TehsilId.Value, "DistrictId", model.DistrictId))
                    errors.Add("The selected Tehsil does not belong to the selected District.");
                if (model.BlockId.HasValue && !Related(cn, "BlockMaster", "BlockId", model.BlockId.Value, "DistrictId", model.DistrictId))
                    errors.Add("The selected Block does not belong to the selected District.");
                if (model.GramPanchayatId.HasValue && !Related(cn, "GramPanchayatMaster", "GramPanchayatId", model.GramPanchayatId.Value, "BlockId", model.BlockId))
                    errors.Add("The selected Gram Panchayat does not belong to the selected Block.");
                if (model.ParliamentaryConstituencyId.HasValue && !Related(cn, "ParliamentaryConstituencyMaster", "ParliamentaryConstituencyId", model.ParliamentaryConstituencyId.Value, "StateId", model.StateId))
                    errors.Add("The selected Parliamentary Constituency does not belong to the selected State.");
                if (model.AssemblyConstituencyId.HasValue && !Related(cn, "AssemblyConstituencyMaster", "AssemblyConstituencyId", model.AssemblyConstituencyId.Value, "StateId", model.StateId))
                    errors.Add("The selected Assembly Constituency does not belong to the selected State.");
            }
            return errors;
        }

        public IList<SelectListItem> Options(string entityType, int? parentId = null, string parentType = null, int? selectedId = null)
        {
            EnsureEntity(entityType);
            return _geography.GetOptions(entityType, parentId, parentType, selectedId);
        }

        public Tuple<int, int, IList<string>> ImportCsv(Stream stream, string entityType, bool updateExisting, int userId)
        {
            EnsureEntity(entityType); int inserted = 0, updated = 0; var errors = new List<string>();
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                string header = reader.ReadLine(); if (String.IsNullOrWhiteSpace(header)) throw new InvalidDataException("CSV file is empty.");
                var headers = ParseCsv(header); int lineNo = 1; string line;
                while ((line = reader.ReadLine()) != null) { lineNo++; if (String.IsNullOrWhiteSpace(line)) continue; try { var values = ParseCsv(line); var row = headers.Select((h, i) => new { h, v = i < values.Count ? values[i] : "" }).ToDictionary(x => x.h.Trim(), x => x.v, StringComparer.OrdinalIgnoreCase); var m = FromCsv(row, entityType); if (Exists(entityType, m.Code, 0)) { if (updateExisting) { m.Id = FindId(entityType, m.Code); Update(m, userId); updated++; } } else { Save(m, userId); inserted++; } } catch (Exception ex) { errors.Add("Line " + lineNo + ": " + ex.Message); } }
            }
            return Tuple.Create(inserted, updated, (IList<string>)errors);
        }

        private int FindId(string e, string code) { using (var cn = new SqlConnection(_connectionString)) using (var cmd = new SqlCommand("SELECT " + IdColumn(e) + " FROM dbo." + Tables[e] + " WHERE Code=@Code AND IsDeleted=0", cn)) { cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code; cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); } }
        private static List<string> ParseCsv(string line) { var result = new List<string>(); var sb = new StringBuilder(); bool q = false; for (int i = 0; i < line.Length; i++) { char c = line[i]; if (c == '"') { if (q && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; } else q = !q; } else if (c == ',' && !q) { result.Add(sb.ToString()); sb.Clear(); } else sb.Append(c); } result.Add(sb.ToString()); return result; }
        private GeographyEditVM FromCsv(IDictionary<string, string> r, string e) { int n; var m = new GeographyEditVM { EntityType = e, Code = Need(r, "Code"), NameEnglish = Need(r, "NameEnglish"), NameHindi = Get(r, "NameHindi"), IsActive = true, AreaType = Get(r, "AreaType"), ReservationCategory = Get(r, "ReservationCategory") }; if (Int32.TryParse(Get(r, "StateId"), out n)) m.StateId = n; if (Int32.TryParse(Get(r, "DistrictId"), out n)) m.DistrictId = n; if (Int32.TryParse(Get(r, "TehsilId"), out n)) m.TehsilId = n; if (Int32.TryParse(Get(r, "BlockId"), out n)) m.BlockId = n; if (Int32.TryParse(Get(r, "ParliamentaryConstituencyId"), out n)) m.ParliamentaryConstituencyId = n; if (Int32.TryParse(Get(r, "AssemblyConstituencyId"), out n)) m.AssemblyConstituencyId = n; if (Int32.TryParse(Get(r, "GramPanchayatId"), out n)) m.GramPanchayatId = n; return m; }
        private static string Need(IDictionary<string, string> r, string k) { var v = Get(r, k); if (String.IsNullOrWhiteSpace(v)) throw new InvalidDataException(k + " is required."); return v.Trim(); }
        private static string Get(IDictionary<string, string> r, string k) { string v; return r.TryGetValue(k, out v) ? v : null; }
        private static int Count(SqlConnection cn, string t) { using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo." + t + " WHERE IsDeleted=0", cn)) return Convert.ToInt32(cmd.ExecuteScalar()); }
        private static bool Related(SqlConnection cn, string table, string idColumn, int id, string parentColumn, int? parentId)
        {
            if (!parentId.HasValue) return false;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo." + table + " WHERE " + idColumn + "=@Id AND " + parentColumn + "=@ParentId AND IsDeleted=0", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId.Value;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        private int CountRows(SqlConnection cn, string e, string keyword) { using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo." + Tables[e] + " x WHERE x.IsDeleted=0 AND (@Keyword IS NULL OR x.Code LIKE '%'+@Keyword+'%' OR x.NameEnglish LIKE '%'+@Keyword+'%' OR x.NameHindi LIKE '%'+@Keyword+'%')", cn)) { cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200).Value = (object)keyword ?? DBNull.Value; return Convert.ToInt32(cmd.ExecuteScalar()); } }
        private IList<GeographyRowVM> GetRows(SqlConnection cn, string e, string keyword, int page, int pageSize) { string parentJoin = ParentJoin(e); string parentName = e == "Village" ? "COALESCE(p.NameEnglish,t.NameEnglish)" : parentJoin == null ? "CAST(NULL AS NVARCHAR(200))" : "p.NameEnglish"; string sql = "SELECT x." + IdColumn(e) + ",x.Code,x.NameEnglish,x.NameHindi," + parentName + ",x.IsActive FROM dbo." + Tables[e] + " x " + (parentJoin ?? "") + " WHERE x.IsDeleted=0 AND (@Keyword IS NULL OR x.Code LIKE '%'+@Keyword+'%' OR x.NameEnglish LIKE '%'+@Keyword+'%' OR x.NameHindi LIKE '%'+@Keyword+'%') ORDER BY x.NameEnglish,x." + IdColumn(e) + " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"; using (var cmd = new SqlCommand(sql, cn)) { cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 200).Value = (object)keyword ?? DBNull.Value; cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = (page - 1) * pageSize; cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize; var list = new List<GeographyRowVM>(); using (var r = cmd.ExecuteReader()) while (r.Read()) list.Add(new GeographyRowVM { Id = Convert.ToInt32(r[0]), Code = Convert.ToString(r[1]), NameEnglish = Convert.ToString(r[2]), NameHindi = Convert.ToString(r[3]), ParentName = Convert.ToString(r[4]), IsActive = Convert.ToBoolean(r[5]), EntityType = e }); return list; } }
        private static string ParentJoin(string e) { var p = ParentColumn(e); if (p == null) return null; if (e == "Village") return "LEFT JOIN dbo.GramPanchayatMaster p ON p.GramPanchayatId=x.GramPanchayatId LEFT JOIN dbo.TehsilMaster t ON t.TehsilId=x.TehsilId"; string pe = e == "District" ? "State" : e == "Tehsil" ? "District" : e == "Block" ? "District" : e == "ParliamentaryConstituency" ? "State" : e == "AssemblyConstituency" ? "State" : e == "Ward" ? "AssemblyConstituency" : e == "GramPanchayat" ? "Block" : null; return "LEFT JOIN dbo." + Tables[pe] + " p ON p." + IdColumn(pe) + "=x." + p; }
        private static string ParentColumn(string e) { switch (e) { case "District": return "StateId"; case "Tehsil": return "DistrictId"; case "Block": return "DistrictId"; case "ParliamentaryConstituency": return "StateId"; case "AssemblyConstituency": return "StateId"; case "Ward": return "AssemblyConstituencyId"; case "GramPanchayat": return "BlockId"; case "Village": return "GramPanchayatId"; default: return null; } }
        private static string IdColumn(string e) { return e + "Id"; }
        private static IList<string> Columns(string e) { switch (e) { case "District": return new[] { "StateId" }; case "Tehsil": return new[] { "StateId", "DistrictId" }; case "Block": return new[] { "StateId", "DistrictId" }; case "ParliamentaryConstituency": return new[] { "StateId", "ReservationCategory" }; case "AssemblyConstituency": return new[] { "StateId", "ParliamentaryConstituencyId", "ReservationCategory" }; case "Ward": return new[] { "StateId", "DistrictId", "AssemblyConstituencyId", "AreaType" }; case "GramPanchayat": return new[] { "StateId", "DistrictId", "BlockId" }; case "Village": return new[] { "StateId", "DistrictId", "TehsilId", "BlockId", "GramPanchayatId", "AreaType" }; default: return new string[0]; } }
        private static void AddParameters(SqlCommand c, GeographyEditVM m, int uid, IList<string> cols) { c.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = m.Code.Trim(); c.Parameters.Add("@NameEnglish", SqlDbType.NVarChar, 200).Value = m.NameEnglish.Trim(); c.Parameters.Add("@NameHindi", SqlDbType.NVarChar, 200).Value = (object)m.NameHindi ?? DBNull.Value; c.Parameters.Add("@IsActive", SqlDbType.Bit).Value = m.IsActive; c.Parameters.Add("@UserId", SqlDbType.Int).Value = uid; foreach (var x in cols) c.Parameters.Add("@" + x, x.EndsWith("Id") ? SqlDbType.Int : SqlDbType.NVarChar).Value = Value(m, x) ?? DBNull.Value; }
        private static object Value(GeographyEditVM m, string x) { switch (x) { case "StateId": return m.StateId; case "DistrictId": return m.DistrictId; case "TehsilId": return m.TehsilId; case "BlockId": return m.BlockId; case "ParliamentaryConstituencyId": return m.ParliamentaryConstituencyId; case "AssemblyConstituencyId": return m.AssemblyConstituencyId; case "GramPanchayatId": return m.GramPanchayatId; case "ReservationCategory": return m.ReservationCategory; case "AreaType": return m.AreaType; default: return null; } }
        private static GeographyEditVM MapEdit(IDataRecord r, string e) { var m = new GeographyEditVM { Id = Convert.ToInt32(r[IdColumn(e)]), EntityType = e, Code = Convert.ToString(r["Code"]), NameEnglish = Convert.ToString(r["NameEnglish"]), NameHindi = Convert.ToString(r["NameHindi"]), IsActive = Convert.ToBoolean(r["IsActive"]) }; foreach (var x in Columns(e)) { if (r[x] == DBNull.Value) continue; switch (x) { case "StateId": m.StateId = Convert.ToInt32(r[x]); break; case "DistrictId": m.DistrictId = Convert.ToInt32(r[x]); break; case "TehsilId": m.TehsilId = Convert.ToInt32(r[x]); break; case "BlockId": m.BlockId = Convert.ToInt32(r[x]); break; case "ParliamentaryConstituencyId": m.ParliamentaryConstituencyId = Convert.ToInt32(r[x]); break; case "AssemblyConstituencyId": m.AssemblyConstituencyId = Convert.ToInt32(r[x]); break; case "GramPanchayatId": m.GramPanchayatId = Convert.ToInt32(r[x]); break; case "ReservationCategory": m.ReservationCategory = Convert.ToString(r[x]); break; case "AreaType": m.AreaType = Convert.ToString(r[x]); break; } } return m; }
        private static void EnsureEntity(string e) { if (String.IsNullOrWhiteSpace(e) || !Tables.ContainsKey(e)) throw new ArgumentException("Invalid geography entity type."); }
        private bool HasActiveChildren(int id, string entityType)
        {
            var relations = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "State", new[] { "DistrictMaster:StateId", "ParliamentaryConstituencyMaster:StateId", "AssemblyConstituencyMaster:StateId" } },
                { "District", new[] { "TehsilMaster:DistrictId", "BlockMaster:DistrictId", "GramPanchayatMaster:DistrictId", "VillageMaster:DistrictId", "WardMaster:DistrictId" } },
                { "Tehsil", new[] { "VillageMaster:TehsilId" } },
                { "Block", new[] { "GramPanchayatMaster:BlockId", "VillageMaster:BlockId" } },
                { "ParliamentaryConstituency", new[] { "AssemblyConstituencyMaster:ParliamentaryConstituencyId" } },
                { "AssemblyConstituency", new[] { "WardMaster:AssemblyConstituencyId" } },
                { "GramPanchayat", new[] { "VillageMaster:GramPanchayatId" } }
            };
            string[] children;
            if (!relations.TryGetValue(entityType, out children)) return false;
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                foreach (var relation in children)
                {
                    var parts = relation.Split(':');
                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo." + parts[0] + " WHERE " + parts[1] + "=@Id AND IsDeleted=0", cn))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) return true;
                    }
                }
            }
            return false;
        }
    }
}
