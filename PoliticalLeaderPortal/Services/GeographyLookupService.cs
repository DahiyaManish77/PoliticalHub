using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Services
{
    public class GeographyLookupService
    {
        private readonly string _connectionString;

        private sealed class GeographyDefinition
        {
            public string Table { get; set; }
            public string IdColumn { get; set; }
            public string[] NameColumns { get; set; }
            public IDictionary<string, string> ParentColumns { get; set; }
        }

        private static readonly IDictionary<string, GeographyDefinition> Definitions =
            new Dictionary<string, GeographyDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "State", Definition("StateMaster", "StateId", null, "NameEnglish", "StateName") },
                { "District", Definition("DistrictMaster", "DistrictId", Parents("State", "StateId"), "NameEnglish", "DistrictName") },
                { "ParliamentaryConstituency", Definition("ParliamentaryConstituencyMaster", "ParliamentaryConstituencyId", Parents("State", "StateId"), "NameEnglish", "ParliamentaryConstituencyName") },
                { "AssemblyConstituency", Definition("AssemblyConstituencyMaster", "AssemblyConstituencyId", Parents("ParliamentaryConstituency", "ParliamentaryConstituencyId", "State", "StateId"), "NameEnglish", "AssemblyConstituencyName", "AssemblyName") },
                { "Tehsil", Definition("TehsilMaster", "TehsilId", Parents("District", "DistrictId"), "NameEnglish", "TehsilName", "SubDistrictName") },
                { "Block", Definition("BlockMaster", "BlockId", Parents("District", "DistrictId"), "NameEnglish", "BlockName") },
                { "GramPanchayat", Definition("GramPanchayatMaster", "GramPanchayatId", Parents("Block", "BlockId", "District", "DistrictId"), "NameEnglish", "GramPanchayatName") },
                { "Village", Definition("VillageMaster", "VillageId", Parents("GramPanchayat", "GramPanchayatId", "Block", "BlockId", "Tehsil", "TehsilId", "District", "DistrictId"), "NameEnglish", "VillageName") },
                { "Ward", Definition("WardMaster", "WardId", Parents("AssemblyConstituency", "AssemblyConstituencyId"), "NameEnglish", "WardName") },
                { "Mandal", Definition("MandalMaster", "MandalId", Parents("AssemblyConstituency", "AssemblyConstituencyId"), "NameEnglish", "MandalName") },
                { "Sector", Definition("SectorMaster", "SectorId", Parents("Mandal", "MandalId"), "NameEnglish", "SectorName") },
                { "Booth", Definition("ElectionBooth", "ElectionBoothId", Parents("AssemblyConstituency", "AssemblyConstituencyId"), "BoothName", "PollingStationName", "NameEnglish", "Name") }
            };

        public GeographyLookupService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public IList<SelectListItem> GetOptions(
            string entityType,
            int? parentId = null,
            string parentType = null,
            int? selectedId = null)
        {
            GeographyDefinition definition;
            entityType = NormalizeType(entityType);
            parentType = NormalizeType(parentType);

            if (!Definitions.TryGetValue(entityType, out definition))
                throw new ArgumentException("Unsupported geography type.", "entityType");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (!TableExists(connection, definition.Table) ||
                    !ColumnExists(connection, definition.Table, definition.IdColumn))
                    return new List<SelectListItem>();

                string nameColumn = definition.NameColumns
                    .FirstOrDefault(x => ColumnExists(connection, definition.Table, x));
                if (String.IsNullOrWhiteSpace(nameColumn))
                    return new List<SelectListItem>();

                var conditions = new List<string>();
                if (ColumnExists(connection, definition.Table, "IsDeleted"))
                    conditions.Add("ISNULL([IsDeleted],0)=0");
                if (ColumnExists(connection, definition.Table, "IsActive"))
                    conditions.Add("ISNULL([IsActive],1)=1");

                string parentColumn = ResolveParentColumn(definition, parentType);
                if (parentId.HasValue && String.IsNullOrWhiteSpace(parentColumn))
                    return new List<SelectListItem>();
                if (parentId.HasValue && !String.IsNullOrWhiteSpace(parentColumn) &&
                    ColumnExists(connection, definition.Table, parentColumn))
                    conditions.Add("[" + parentColumn + "]=@ParentId");

                string sql = "SELECT [" + definition.IdColumn + "],[" + nameColumn + "] FROM dbo.[" +
                    definition.Table + "]" +
                    (conditions.Any() ? " WHERE " + String.Join(" AND ", conditions) : "") +
                    " ORDER BY [" + nameColumn + "]";

                using (var command = new SqlCommand(sql, connection))
                {
                    if (sql.Contains("@ParentId"))
                        command.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId.Value;

                    var items = new List<SelectListItem>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader[0]);
                            items.Add(new SelectListItem
                            {
                                Value = Convert.ToString(id),
                                Text = Convert.ToString(reader[1]),
                                Selected = selectedId.HasValue && selectedId.Value == id
                            });
                        }
                    }
                    return items;
                }
            }
        }

        public int? GetParliamentaryConstituencyId(int? assemblyConstituencyId)
        {
            if (!assemblyConstituencyId.HasValue) return null;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(
                @"SELECT ParliamentaryConstituencyId
                  FROM dbo.AssemblyConstituencyMaster
                  WHERE AssemblyConstituencyId=@Id
                    AND ISNULL(IsDeleted,0)=0", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = assemblyConstituencyId.Value;
                connection.Open();
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        public bool IsValidSelection(string entityType, int? id, int? parentId = null, string parentType = null)
        {
            if (!id.HasValue) return true;
            GeographyDefinition definition;
            entityType = NormalizeType(entityType);
            parentType = NormalizeType(parentType);
            if (!Definitions.TryGetValue(entityType, out definition)) return false;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string parentColumn = ResolveParentColumn(definition, parentType);
                if (parentId.HasValue && String.IsNullOrWhiteSpace(parentColumn)) return false;
                var conditions = new List<string> { "[" + definition.IdColumn + "]=@Id" };
                if (parentId.HasValue) conditions.Add("[" + parentColumn + "]=@ParentId");
                if (ColumnExists(connection, definition.Table, "IsDeleted")) conditions.Add("ISNULL([IsDeleted],0)=0");
                if (ColumnExists(connection, definition.Table, "IsActive")) conditions.Add("ISNULL([IsActive],1)=1");
                using (var command = new SqlCommand(
                    "SELECT COUNT(1) FROM dbo.[" + definition.Table + "] WHERE " + String.Join(" AND ", conditions), connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id.Value;
                    if (parentId.HasValue) command.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId.Value;
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private static string ResolveParentColumn(GeographyDefinition definition, string parentType)
        {
            if (definition.ParentColumns == null || definition.ParentColumns.Count == 0)
                return null;

            string column;
            if (!String.IsNullOrWhiteSpace(parentType) &&
                definition.ParentColumns.TryGetValue(parentType, out column))
                return column;

            return definition.ParentColumns.Count == 1
                ? definition.ParentColumns.Values.First()
                : null;
        }

        private static string NormalizeType(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            string normalized = value.Replace(" ", "").Replace("-", "");
            if (normalized.Equals("Assembly", StringComparison.OrdinalIgnoreCase)) return "AssemblyConstituency";
            if (normalized.Equals("Parliament", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("Parliamentary", StringComparison.OrdinalIgnoreCase))
                return "ParliamentaryConstituency";
            if (normalized.Equals("GP", StringComparison.OrdinalIgnoreCase)) return "GramPanchayat";
            return normalized;
        }

        private static GeographyDefinition Definition(
            string table,
            string idColumn,
            IDictionary<string, string> parents,
            params string[] nameColumns)
        {
            return new GeographyDefinition
            {
                Table = table,
                IdColumn = idColumn,
                ParentColumns = parents,
                NameColumns = nameColumns
            };
        }

        private static IDictionary<string, string> Parents(params string[] pairs)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index + 1 < pairs.Length; index += 2)
                result[pairs[index]] = pairs[index + 1];
            return result;
        }

        private static bool TableExists(SqlConnection connection, string table)
        {
            using (var command = new SqlCommand(
                "SELECT CASE WHEN OBJECT_ID(@Table,'U') IS NULL THEN 0 ELSE 1 END", connection))
            {
                command.Parameters.Add("@Table", SqlDbType.NVarChar, 260).Value = "dbo." + table;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static bool ColumnExists(SqlConnection connection, string table, string column)
        {
            using (var command = new SqlCommand(
                "SELECT CASE WHEN COL_LENGTH(@Table,@Column) IS NULL THEN 0 ELSE 1 END", connection))
            {
                command.Parameters.Add("@Table", SqlDbType.NVarChar, 260).Value = "dbo." + table;
                command.Parameters.Add("@Column", SqlDbType.NVarChar, 128).Value = column;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }
    }
}
