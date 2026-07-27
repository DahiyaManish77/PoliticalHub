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
    /// Imports ECI constituency control/master XLSX data. Parliamentary and
    /// Assembly imports are intentionally separate, and AC rows must resolve
    /// to an existing PC in the selected State.
    /// </summary>
    public class EciConstituencyImportService
    {
        private readonly string _connectionString;

        public EciConstituencyImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
                _connectionString = db.Database.Connection.ConnectionString;
        }

        public GovernmentImportResultVM ImportParliamentaryXlsx(
            Stream stream, string fileName, int stateId, bool updateExisting, int userId)
        {
            return Import(stream, fileName, stateId, updateExisting, userId, false);
        }

        public GovernmentImportResultVM ImportAssemblyXlsx(
            Stream stream, string fileName, int stateId, bool updateExisting, int userId)
        {
            return Import(stream, fileName, stateId, updateExisting, userId, true);
        }

        private GovernmentImportResultVM Import(
            Stream stream, string fileName, int stateId, bool updateExisting, int userId, bool assembly)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!String.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Please upload an official ECI XLSX workbook.");

            IWorkbook workbook;
            try { workbook = new XSSFWorkbook(stream); }
            catch (Exception ex) { throw new InvalidDataException("The uploaded ECI XLSX workbook could not be opened.", ex); }

            using (workbook)
            {
                var sheet = workbook.NumberOfSheets == 0 ? null : workbook.GetSheetAt(0);
                if (sheet == null) throw new InvalidDataException("The workbook contains no worksheet.");
                var formatter = new DataFormatter();
                var headerRowNumber = FindHeaderRow(sheet, formatter, assembly);
                if (headerRowNumber < 0)
                    throw new InvalidDataException(assembly
                        ? "Assembly Constituency number/name and Parliamentary Constituency reference columns were not found."
                        : "Parliamentary Constituency number and name columns were not found.");

                var headers = HeaderMap(sheet.GetRow(headerRowNumber), formatter);
                var codeColumn = NeedHeader(headers, assembly
                    ? new[] { "acno", "acnumber", "assemblyconstituencyno", "assemblyconstituencynumber", "constituencyno" }
                    : new[] { "pcno", "pcnumber", "parliamentaryconstituencyno", "parliamentaryconstituencynumber", "constituencyno" });
                var nameColumn = NeedHeader(headers, assembly
                    ? new[] { "acname", "assemblyconstituencyname", "constituencyname" }
                    : new[] { "pcname", "parliamentaryconstituencyname", "constituencyname" });
                var reservationColumn = OptionalHeader(headers, "reservation", "reservationstatus", "reservedfor", "category");
                var pcCodeColumn = assembly
                    ? NeedHeader(headers, "pcno", "pcnumber", "parliamentaryconstituencyno", "parliamentaryconstituencynumber")
                    : -1;

                var result = new GovernmentImportResultVM
                {
                    FileName = fileName,
                    EntityType = assembly ? "Assembly Constituency" : "Parliamentary Constituency"
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
                            if (String.IsNullOrWhiteSpace(code) || String.IsNullOrWhiteSpace(name))
                                throw new InvalidDataException("Constituency number and name are required.");
                            var reservation = NormalizeReservation(
                                reservationColumn.HasValue ? Cell(row, reservationColumn.Value, formatter) : null,
                                name);
                            int? pcId = null;
                            if (assembly)
                            {
                                var pcCode = Cell(row, pcCodeColumn, formatter);
                                pcId = FindId(cn, "ParliamentaryConstituencyMaster", "ParliamentaryConstituencyId", stateId, pcCode);
                                if (!pcId.HasValue)
                                    throw new InvalidDataException("Parliamentary Constituency " + pcCode + " does not exist in the selected State.");
                            }

                            var table = assembly ? "AssemblyConstituencyMaster" : "ParliamentaryConstituencyMaster";
                            var idColumn = assembly ? "AssemblyConstituencyId" : "ParliamentaryConstituencyId";
                            var existingId = FindId(cn, table, idColumn, stateId, code);
                            if (existingId.HasValue)
                            {
                                if (!updateExisting) { result.Skipped++; continue; }
                                Save(cn, assembly, true, existingId.Value, stateId, pcId, code, name, reservation, userId);
                                result.Updated++;
                            }
                            else
                            {
                                Save(cn, assembly, false, 0, stateId, pcId, code, name, reservation, userId);
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

        private static void Save(SqlConnection cn, bool assembly, bool update, int id, int stateId,
            int? pcId, string code, string name, string reservation, int userId)
        {
            string sql;
            if (assembly)
                sql = update
                    ? @"UPDATE dbo.AssemblyConstituencyMaster SET StateId=@StateId,ParliamentaryConstituencyId=@PcId,Code=@Code,NameEnglish=@Name,ReservationCategory=@Reservation,IsActive=1,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE AssemblyConstituencyId=@Id"
                    : @"INSERT dbo.AssemblyConstituencyMaster(StateId,ParliamentaryConstituencyId,Code,NameEnglish,ReservationCategory,IsActive,IsDeleted,CreatedBy,CreatedDate) VALUES(@StateId,@PcId,@Code,@Name,@Reservation,1,0,@UserId,GETDATE())";
            else
                sql = update
                    ? @"UPDATE dbo.ParliamentaryConstituencyMaster SET StateId=@StateId,Code=@Code,NameEnglish=@Name,ReservationCategory=@Reservation,IsActive=1,UpdatedBy=@UserId,UpdatedDate=GETDATE() WHERE ParliamentaryConstituencyId=@Id"
                    : @"INSERT dbo.ParliamentaryConstituencyMaster(StateId,Code,NameEnglish,ReservationCategory,IsActive,IsDeleted,CreatedBy,CreatedDate) VALUES(@StateId,@Code,@Name,@Reservation,1,0,@UserId,GETDATE())";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                if (assembly) cmd.Parameters.Add("@PcId", SqlDbType.Int).Value = pcId.Value;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = CleanName(name);
                cmd.Parameters.Add("@Reservation", SqlDbType.NVarChar, 30).Value = (object)reservation ?? DBNull.Value;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                if (update) cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.ExecuteNonQuery();
            }
        }

        private static int? FindId(SqlConnection cn, string table, string idColumn, int stateId, string code)
        {
            if (String.IsNullOrWhiteSpace(code)) return null;
            using (var cmd = new SqlCommand("SELECT TOP 1 " + idColumn + " FROM dbo." + table +
                " WHERE StateId=@StateId AND IsDeleted=0 AND (Code=@Code OR (TRY_CONVERT(INT,Code)=TRY_CONVERT(INT,@Code) AND TRY_CONVERT(INT,@Code) IS NOT NULL))", cn))
            {
                cmd.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code.Trim();
                var value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
            }
        }

        private static string NormalizeReservation(string value, string name)
        {
            var source = ((value ?? String.Empty) + " " + (name ?? String.Empty)).ToUpperInvariant();
            if (Regex.IsMatch(source, @"\(\s*ST\s*\)|\bST\b")) return "ST";
            if (Regex.IsMatch(source, @"\(\s*SC\s*\)|\bSC\b")) return "SC";
            return "General";
        }

        private static string CleanName(string name)
        {
            return Regex.Replace(name ?? String.Empty, @"\s*\(\s*(SC|ST)\s*\)\s*$", String.Empty, RegexOptions.IgnoreCase).Trim();
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

        private static int FindHeaderRow(ISheet sheet, DataFormatter formatter, bool assembly)
        {
            for (var i = 0; i <= Math.Min(sheet.LastRowNum, 30); i++)
            {
                var map = HeaderMap(sheet.GetRow(i), formatter);
                var hasName = assembly
                    ? HasAny(map, "acname", "assemblyconstituencyname", "constituencyname")
                    : HasAny(map, "pcname", "parliamentaryconstituencyname", "constituencyname");
                var hasNumber = assembly
                    ? HasAny(map, "acno", "acnumber", "assemblyconstituencyno", "assemblyconstituencynumber", "constituencyno")
                    : HasAny(map, "pcno", "pcnumber", "parliamentaryconstituencyno", "parliamentaryconstituencynumber", "constituencyno");
                if (hasName && hasNumber && (!assembly || HasAny(map, "pcno", "pcnumber", "parliamentaryconstituencyno", "parliamentaryconstituencynumber"))) return i;
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

        private static bool HasAny(IDictionary<string, int> map, params string[] names)
        {
            foreach (var name in names) if (map.ContainsKey(name)) return true;
            return false;
        }

        private static int NeedHeader(IDictionary<string, int> map, params string[] names)
        {
            var value = OptionalHeader(map, names);
            if (!value.HasValue) throw new InvalidDataException("Required ECI column was not found: " + String.Join(" / ", names));
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
