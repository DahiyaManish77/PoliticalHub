using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    /// <summary>
    /// Imports village-level PRI local bodies (Gram Panchayats) from an
    /// official LGD XLSX report. Header aliases cover the naming used by
    /// LGD PRI Local Body and village-panchayat exports.
    /// </summary>
    public class LgdGramPanchayatImportService
    {
        private readonly string _connectionString;

        public LgdGramPanchayatImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public GovernmentImportResultVM ImportOfficialGramPanchayatXlsx(
            Stream stream, string fileName, int stateId, bool updateExisting,
            string sourceName, int userId)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!String.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Please upload an official LGD Gram Panchayat XLSX report.");

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
                    throw new InvalidDataException("Gram Panchayat columns were not found. Use the official LGD PRI Local Body XLSX report.");

                var headers = HeaderMap(sheet.GetRow(headerRowNumber), formatter);
                var districtColumn = NeedHeader(headers, "districtcode");
                var blockColumn = NeedHeader(headers, "developmentblockcode", "blockcode");
                var codeColumn = NeedHeader(headers, "localbodycode", "villagepanchayatcode", "grampanchayatcode");
                var nameColumn = NeedHeader(headers, "localbodynameinenglish", "villagepanchayatnameinenglish", "grampanchayatnameinenglish");
                var localNameColumn = OptionalHeader(headers, "localbodynameinlocal", "villagepanchayatnameinlocal", "grampanchayatnameinlocal");
                var typeColumn = OptionalHeader(headers, "localbodytypename", "localbodytype");

                var result = new GovernmentImportResultVM
                {
                    FileName = fileName,
                    EntityType = "Gram Panchayat"
                };

                using (var cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    EnsureState(cn, stateId, result);
                    for (var rowNumber = headerRowNumber + 1; rowNumber <= sheet.LastRowNum; rowNumber++)
                    {
                        var row = sheet.GetRow(rowNumber);
                        if (row == null) continue;
                        var code = Cell(row, codeColumn, formatter);
                        var name = Cell(row, nameColumn, formatter);
                        if (String.IsNullOrWhiteSpace(code) && String.IsNullOrWhiteSpace(name)) continue;

                        result.TotalRows++;
                        try
                        {
                            var type = typeColumn.HasValue ? Cell(row, typeColumn.Value, formatter) : null;
                            if (!String.IsNullOrWhiteSpace(type) &&
                                type.IndexOf("village", StringComparison.OrdinalIgnoreCase) < 0 &&
                                type.IndexOf("gram", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                result.Skipped++;
                                continue;
                            }

                            if (String.IsNullOrWhiteSpace(code) || String.IsNullOrWhiteSpace(name))
                                throw new InvalidDataException("Gram Panchayat code and English name are required.");

                            var districtCode = Cell(row, districtColumn, formatter);
                            var blockCode = Cell(row, blockColumn, formatter);
                            var districtId = FindRelatedId(cn, "DistrictMaster", "DistrictId", districtCode, "StateId", stateId);
                            if (!districtId.HasValue) throw new InvalidDataException("District code " + districtCode + " is not present for the selected State.");
                            var blockId = FindRelatedId(cn, "BlockMaster", "BlockId", blockCode, "DistrictId", districtId.Value);
                            if (!blockId.HasValue) throw new InvalidDataException("Block code " + blockCode + " is not present in the matched District.");

                            var existingId = FindRelatedId(cn, "GramPanchayatMaster", "GramPanchayatId", code, "StateId", stateId);
                            if (existingId.HasValue)
                            {
                                if (!updateExisting) { result.Skipped++; continue; }
                                Execute(cn, @"UPDATE dbo.GramPanchayatMaster SET DistrictId=@DistrictId,BlockId=@BlockId,
Code=@Code,NameEnglish=@NameEnglish,NameHindi=@NameHindi,IsActive=1,UpdatedBy=@UserId,UpdatedDate=GETDATE()
WHERE GramPanchayatId=@Id",
                                    stateId, districtId.Value, blockId.Value, code, name,
                                    localNameColumn.HasValue ? Cell(row, localNameColumn.Value, formatter) : null,
                                    userId, existingId.Value);
                                result.Updated++;
                            }
                            else
                            {
                                Execute(cn, @"INSERT dbo.GramPanchayatMaster
(StateId,DistrictId,BlockId,Code,NameEnglish,NameHindi,IsActive,IsDeleted,CreatedBy,CreatedDate)
VALUES(@StateId,@DistrictId,@BlockId,@Code,@NameEnglish,@NameHindi,1,0,@UserId,GETDATE())",
                                    stateId, districtId.Value, blockId.Value, code, name,
                                    localNameColumn.HasValue ? Cell(row, localNameColumn.Value, formatter) : null,
                                    userId, null);
                                result.Inserted++;
                            }
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

        private static int? FindRelatedId(SqlConnection cn, string table, string idColumn, string code, string parentColumn, int parentId)
        {
            if (String.IsNullOrWhiteSpace(code)) return null;
            var sql = "SELECT TOP 1 " + idColumn + " FROM dbo." + table +
                      " WHERE IsDeleted=0 AND " + parentColumn + "=@ParentId AND (Code=@Code OR (TRY_CONVERT(INT,Code)=TRY_CONVERT(INT,@Code) AND TRY_CONVERT(INT,@Code) IS NOT NULL))";
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                var value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
            }
        }

        private static void Execute(SqlConnection cn, string sql, int stateId, int districtId, int blockId,
            string code, string name, string localName, int userId, int? id)
        {
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                cmd.Parameters.Add("@DistrictId", SqlDbType.Int).Value = districtId;
                cmd.Parameters.Add("@BlockId", SqlDbType.Int).Value = blockId;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                cmd.Parameters.Add("@NameEnglish", SqlDbType.NVarChar, 200).Value = name.Trim();
                cmd.Parameters.Add("@NameHindi", SqlDbType.NVarChar, 200).Value = String.IsNullOrWhiteSpace(localName) ? (object)DBNull.Value : localName.Trim();
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                if (id.HasValue) cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id.Value;
                cmd.ExecuteNonQuery();
            }
        }

        private static int FindHeaderRow(ISheet sheet, DataFormatter formatter)
        {
            for (var i = 0; i <= Math.Min(sheet.LastRowNum, 30); i++)
            {
                var map = HeaderMap(sheet.GetRow(i), formatter);
                if (map.ContainsKey("districtcode") &&
                    (map.ContainsKey("developmentblockcode") || map.ContainsKey("blockcode")) &&
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
    }
}
