using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    /// <summary>
    /// Applies the official LGD Gram Panchayat-to-village mapping report.
    /// This service only links existing master records; it never creates or
    /// renames a Village, Gram Panchayat, Block or District.
    /// </summary>
    public class LgdVillageMappingImportService
    {
        private readonly string _connectionString;

        public LgdVillageMappingImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public GovernmentImportResultVM ImportOfficialVillageMappingXlsx(
            Stream stream, string fileName, int stateId, int userId)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!String.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Please upload the official LGD Gram Panchayat Mapping to village XLSX report.");

            IWorkbook workbook;
            try { workbook = new XSSFWorkbook(stream); }
            catch (Exception ex) { throw new InvalidDataException("The uploaded XLSX workbook could not be opened.", ex); }

            using (workbook)
            {
                if (workbook.NumberOfSheets == 0) throw new InvalidDataException("The workbook contains no worksheet.");
                var sheet = workbook.GetSheetAt(0);
                var formatter = new DataFormatter();
                var headerRowNumber = FindHeaderRow(sheet, formatter);
                if (headerRowNumber < 0)
                    throw new InvalidDataException("Village and Gram Panchayat mapping columns were not found.");

                var headers = HeaderMap(sheet.GetRow(headerRowNumber), formatter);
                var villageColumn = NeedHeader(headers, "villagecode", "censusvillagecode");
                var gpColumn = NeedHeader(headers, "localbodycode", "villagepanchayatcode", "grampanchayatcode");
                var districtColumn = OptionalHeader(headers, "districtcode");
                var result = new GovernmentImportResultVM
                {
                    FileName = fileName,
                    EntityType = "Village → Gram Panchayat Mapping"
                };

                using (var cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    EnsureState(cn, stateId, result);
                    for (var rowNumber = headerRowNumber + 1; rowNumber <= sheet.LastRowNum; rowNumber++)
                    {
                        var row = sheet.GetRow(rowNumber);
                        if (row == null) continue;
                        var villageCode = Cell(row, villageColumn, formatter);
                        var gpCode = Cell(row, gpColumn, formatter);
                        if (String.IsNullOrWhiteSpace(villageCode) && String.IsNullOrWhiteSpace(gpCode)) continue;
                        result.TotalRows++;

                        try
                        {
                            if (String.IsNullOrWhiteSpace(villageCode) || String.IsNullOrWhiteSpace(gpCode))
                                throw new InvalidDataException("Village Code and Gram Panchayat Code are required.");

                            var village = FindVillage(cn, stateId, villageCode);
                            if (village == null) throw new InvalidDataException("Village code " + villageCode + " does not exist in the selected State.");
                            var gp = FindGramPanchayat(cn, stateId, gpCode);
                            if (gp == null) throw new InvalidDataException("Gram Panchayat code " + gpCode + " does not exist in the selected State.");
                            if (village.DistrictId != gp.DistrictId)
                                throw new InvalidDataException("Village and Gram Panchayat belong to different Districts.");

                            if (districtColumn.HasValue)
                            {
                                var reportDistrictCode = Cell(row, districtColumn.Value, formatter);
                                if (!String.IsNullOrWhiteSpace(reportDistrictCode) &&
                                    !CodeMatches(cn, "DistrictMaster", "DistrictId", village.DistrictId, reportDistrictCode))
                                    throw new InvalidDataException("The report District Code does not match the Village District.");
                            }

                            if (village.GramPanchayatId == gp.Id && village.BlockId == gp.BlockId)
                            {
                                result.Skipped++;
                                continue;
                            }

                            using (var cmd = new SqlCommand(@"UPDATE dbo.VillageMaster
SET BlockId=@BlockId,GramPanchayatId=@GramPanchayatId,UpdatedBy=@UserId,UpdatedDate=GETDATE()
WHERE VillageId=@VillageId AND StateId=@StateId AND IsDeleted=0", cn))
                            {
                                cmd.Parameters.Add("@BlockId", SqlDbType.Int).Value = gp.BlockId;
                                cmd.Parameters.Add("@GramPanchayatId", SqlDbType.Int).Value = gp.Id;
                                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                                cmd.Parameters.Add("@VillageId", SqlDbType.Int).Value = village.Id;
                                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                                if (cmd.ExecuteNonQuery() != 1) throw new DataException("The Village mapping was not updated.");
                            }
                            result.Updated++;
                        }
                        catch (Exception ex)
                        {
                            result.Failed++;
                            if (result.Errors.Count < 100) result.Errors.Add("Row " + (rowNumber + 1) + ": " + ex.Message);
                        }
                    }
                }
                return result;
            }
        }

        private static VillageLink FindVillage(SqlConnection cn, int stateId, string code)
        {
            using (var cmd = new SqlCommand(@"SELECT TOP 1 VillageId,DistrictId,BlockId,GramPanchayatId
FROM dbo.VillageMaster WHERE StateId=@StateId AND IsDeleted=0
AND (Code=@Code OR (TRY_CONVERT(INT,Code)=TRY_CONVERT(INT,@Code) AND TRY_CONVERT(INT,@Code) IS NOT NULL))", cn))
            {
                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new VillageLink
                    {
                        Id = Convert.ToInt32(reader[0]),
                        DistrictId = Convert.ToInt32(reader[1]),
                        BlockId = reader[2] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[2]),
                        GramPanchayatId = reader[3] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[3])
                    };
                }
            }
        }

        private static GramPanchayatLink FindGramPanchayat(SqlConnection cn, int stateId, string code)
        {
            using (var cmd = new SqlCommand(@"SELECT TOP 1 GramPanchayatId,DistrictId,BlockId
FROM dbo.GramPanchayatMaster WHERE StateId=@StateId AND IsDeleted=0 AND IsActive=1
AND (Code=@Code OR (TRY_CONVERT(INT,Code)=TRY_CONVERT(INT,@Code) AND TRY_CONVERT(INT,@Code) IS NOT NULL))", cn))
            {
                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new GramPanchayatLink { Id = Convert.ToInt32(reader[0]), DistrictId = Convert.ToInt32(reader[1]), BlockId = Convert.ToInt32(reader[2]) };
                }
            }
        }

        private static bool CodeMatches(SqlConnection cn, string table, string idColumn, int id, string code)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo." + table + " WHERE " + idColumn + "=@Id AND (Code=@Code OR (TRY_CONVERT(INT,Code)=TRY_CONVERT(INT,@Code) AND TRY_CONVERT(INT,@Code) IS NOT NULL))", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static void EnsureState(SqlConnection cn, int stateId, GovernmentImportResultVM result)
        {
            using (var cmd = new SqlCommand("SELECT Code,NameEnglish FROM dbo.StateMaster WHERE StateId=@Id AND IsDeleted=0", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = stateId;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidDataException("The selected State was not found.");
                    result.ReportStateCode = Convert.ToString(reader[0]);
                    result.ReportStateName = Convert.ToString(reader[1]);
                }
            }
        }

        private static int FindHeaderRow(ISheet sheet, DataFormatter formatter)
        {
            for (var i = 0; i <= Math.Min(sheet.LastRowNum, 30); i++)
            {
                var map = HeaderMap(sheet.GetRow(i), formatter);
                if ((map.ContainsKey("villagecode") || map.ContainsKey("censusvillagecode")) &&
                    (map.ContainsKey("localbodycode") || map.ContainsKey("villagepanchayatcode") || map.ContainsKey("grampanchayatcode")))
                    return i;
            }
            return -1;
        }

        private static IDictionary<string, int> HeaderMap(IRow row, DataFormatter formatter)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (row == null) return map;
            for (var i = row.FirstCellNum < 0 ? 0 : row.FirstCellNum; i < row.LastCellNum; i++)
            {
                var key = Normalize(Cell(row, i, formatter));
                if (!String.IsNullOrWhiteSpace(key) && !map.ContainsKey(key)) map.Add(key, i);
            }
            return map;
        }

        private static int NeedHeader(IDictionary<string, int> map, params string[] names)
        {
            var value = OptionalHeader(map, names);
            if (!value.HasValue) throw new InvalidDataException("Required LGD column was not found: " + String.Join(" / ", names));
            return value.Value;
        }

        private static int? OptionalHeader(IDictionary<string, int> map, params string[] names)
        {
            foreach (var name in names) { int value; if (map.TryGetValue(name, out value)) return value; }
            return null;
        }

        private static string Cell(IRow row, int column, DataFormatter formatter)
        {
            var cell = row == null ? null : row.GetCell(column, MissingCellPolicy.RETURN_BLANK_AS_NULL);
            return cell == null ? null : formatter.FormatCellValue(cell).Trim();
        }

        private static string Normalize(string value)
        {
            return Regex.Replace(value ?? String.Empty, "[^a-z0-9]", String.Empty, RegexOptions.IgnoreCase).ToLowerInvariant();
        }

        private class VillageLink { public int Id; public int DistrictId; public int? BlockId; public int? GramPanchayatId; }
        private class GramPanchayatLink { public int Id; public int DistrictId; public int BlockId; }
    }
}
