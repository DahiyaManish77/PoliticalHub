using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PoliticalLeaderPortal.Areas.Admin.ViewModels.ConstituencyMaster;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    /// <summary>
    /// Imports the official LGD "All Districts of a State" XLSX report.
    /// Uses NPOI because LGD-generated XLSX files may use ZIP data descriptors
    /// that older EPPlus/DotNetZip versions reject with "Bad signature".
    /// </summary>
    public class GovernmentGeographyImportService
    {
        private readonly string _connectionString;

        private static readonly string[] RequiredDistrictHeaders =
        {
            "districtcode",
            "districtnameinenglish"
        };

        public GovernmentGeographyImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                _connectionString = db.Database.Connection.ConnectionString;
            }
        }

        public IList<SelectListItem> GetStateOptions(int? selectedStateId)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = String.Empty,
                    Text = "Select State / Union Territory"
                }
            };

            const string sql = @"
SELECT StateId, Code, NameEnglish
FROM dbo.StateMaster
WHERE IsDeleted = 0
  AND IsActive = 1
ORDER BY NameEnglish;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var stateId = Convert.ToInt32(reader["StateId"]);
                        var code = Convert.ToString(reader["Code"]);
                        var name = Convert.ToString(reader["NameEnglish"]);

                        items.Add(new SelectListItem
                        {
                            Value = stateId.ToString(CultureInfo.InvariantCulture),
                            Text = name + " (" + code + ")",
                            Selected = selectedStateId.HasValue &&
                                       selectedStateId.Value == stateId
                        });
                    }
                }
            }

            return items;
        }

        public IList<SelectListItem> GetImportEntityOptions(string selected)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "District",
                    Text = "District",
                    Selected = true
                }
            };
        }

        /// <summary>
        /// Compatibility method used by ConstituencyMasterController.
        /// </summary>
        public GovernmentImportResultVM ImportOfficialDistrictXlsx(
            Stream xlsxStream,
            string fileName,
            int selectedStateId,
            bool updateExisting,
            string sourceName,
            int userId)
        {
            return ImportDistrictXlsx(
                xlsxStream,
                fileName,
                selectedStateId,
                updateExisting,
                sourceName,
                userId);
        }

        public GovernmentPackageImportResultVM ImportGovernmentPackage(
            Stream packageStream,
            string packageFileName,
            int stateId,
            bool updateExisting,
            string sourceName,
            int userId)
        {
            if (packageStream == null)
                throw new ArgumentNullException("packageStream");

            if (String.IsNullOrWhiteSpace(packageFileName))
                throw new InvalidDataException("Uploaded file name is missing.");

            var extension = Path.GetExtension(packageFileName);

            if (!String.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Please upload the official LGD District XLSX report.");
            }

            var importResult = ImportDistrictXlsx(
                packageStream,
                packageFileName,
                stateId,
                updateExisting,
                sourceName,
                userId);

            var packageResult = new GovernmentPackageImportResultVM
            {
                PackageName = packageFileName,
                FilesDetected = 1,
                FilesImported = 1,
                TotalRows = importResult.TotalRows,
                Inserted = importResult.Inserted,
                Updated = importResult.Updated,
                Skipped = importResult.Skipped,
                Failed = importResult.Failed
            };

            packageResult.Files.Add(new GovernmentPackageFileResultVM
            {
                FileName = packageFileName,
                EntityType = "District",
                Status = importResult.Failed == 0
                    ? "Imported"
                    : "Imported with errors",
                TotalRows = importResult.TotalRows,
                Inserted = importResult.Inserted,
                Updated = importResult.Updated,
                Skipped = importResult.Skipped,
                Failed = importResult.Failed,
                Message = importResult.Errors.Count == 0
                    ? "District import completed successfully."
                    : String.Join(" | ", importResult.Errors.Take(5))
            });

            foreach (var error in importResult.Errors.Take(200))
                packageResult.Errors.Add(error);

            return packageResult;
        }

        public GovernmentImportResultVM ImportDistrictXlsx(
            Stream xlsxStream,
            string fileName,
            int selectedStateId,
            bool updateExisting,
            string sourceName,
            int userId)
        {
            if (xlsxStream == null)
                throw new ArgumentNullException("xlsxStream");

            var selectedState = GetState(selectedStateId);

            if (selectedState == null)
                throw new InvalidDataException(
                    "The selected State/UT was not found in StateMaster.");

            if (xlsxStream.CanSeek)
                xlsxStream.Position = 0;

            var result = new GovernmentImportResultVM();

            IWorkbook workbook;

            try
            {
                workbook = new XSSFWorkbook(xlsxStream);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    "The uploaded file could not be opened as an XLSX workbook. " +
                    "Please upload the unedited LGD XLSX report. Details: " +
                    ex.Message,
                    ex);
            }

            using (workbook)
            {
                if (workbook.NumberOfSheets == 0)
                    throw new InvalidDataException(
                        "The uploaded XLSX workbook does not contain a worksheet.");

                var sheet = workbook.GetSheetAt(0);

                if (sheet == null)
                    throw new InvalidDataException(
                        "The first worksheet could not be read.");

                var formatter = new DataFormatter(CultureInfo.InvariantCulture);
                var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

                var title = GetCellText(
                    sheet.GetRow(0),
                    0,
                    formatter,
                    evaluator);

                var fileStateCode = ExtractStateCode(title);

                if (String.IsNullOrWhiteSpace(fileStateCode))
                {
                    throw new InvalidDataException(
                        "The LGD State Code could not be detected from the first row.");
                }

                if (!CodesEqual(fileStateCode, selectedState.Code))
                {
                    throw new InvalidDataException(
                        "The uploaded report belongs to State Code " +
                        fileStateCode +
                        ", but the selected State is " +
                        selectedState.NameEnglish +
                        " (" + selectedState.Code + ").");
                }

                // Populate report metadata for the Import Result screen.
                result.FileName = fileName;
                result.EntityType = "District";
                result.ReportStateCode = selectedState.Code;
                result.ReportStateName = selectedState.NameEnglish;

                var headerRowIndex = FindHeaderRow(
                    sheet,
                    formatter,
                    evaluator);

                if (headerRowIndex < 0)
                    throw new InvalidDataException(
                        "The LGD district header row could not be found.");

                var headers = BuildHeaderMap(
                    sheet.GetRow(headerRowIndex),
                    formatter,
                    evaluator);

                ValidateDistrictHeaders(headers);

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (var rowIndex = headerRowIndex + 1;
                                 rowIndex <= sheet.LastRowNum;
                                 rowIndex++)
                            {
                                var row = sheet.GetRow(rowIndex);

                                if (IsEmptyRow(row, formatter, evaluator))
                                    continue;

                                result.TotalRows++;

                                try
                                {
                                    var district = ReadDistrictRow(
                                        row,
                                        headers,
                                        selectedState.StateId,
                                        formatter,
                                        evaluator);

                                    SaveDistrict(
                                        connection,
                                        transaction,
                                        district,
                                        updateExisting,
                                        sourceName,
                                        userId,
                                        result);
                                }
                                catch (Exception ex)
                                {
                                    result.Failed++;

                                    if (result.Errors.Count < 200)
                                    {
                                        result.Errors.Add(
                                            "Row " + (rowIndex + 1) +
                                            ": " + ex.Message);
                                    }
                                }
                            }

                            SaveHistory(
                                connection,
                                transaction,
                                sourceName,
                                fileName,
                                result,
                                userId);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }

            return result;
        }

        private StateLookup GetState(int stateId)
        {
            const string sql = @"
SELECT TOP 1 StateId, Code, NameEnglish
FROM dbo.StateMaster
WHERE StateId = @StateId
  AND IsDeleted = 0;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new StateLookup
                    {
                        StateId = Convert.ToInt32(reader["StateId"]),
                        Code = Convert.ToString(reader["Code"]).Trim(),
                        NameEnglish =
                            Convert.ToString(reader["NameEnglish"]).Trim()
                    };
                }
            }
        }

        private static string ExtractStateCode(string title)
        {
            if (String.IsNullOrWhiteSpace(title))
                return null;

            var match = Regex.Match(
                title,
                @"State\s*Code\s*:\s*(\d+)",
                RegexOptions.IgnoreCase);

            return match.Success
                ? match.Groups[1].Value.Trim()
                : null;
        }

        private static int FindHeaderRow(
            ISheet sheet,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var maximumRow = Math.Min(sheet.LastRowNum, 19);

            for (var rowIndex = 0; rowIndex <= maximumRow; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                if (row == null)
                    continue;

                var values = new List<string>();

                for (var columnIndex = 0;
                     columnIndex < row.LastCellNum;
                     columnIndex++)
                {
                    values.Add(
                        NormalizeHeader(
                            GetCellText(
                                row,
                                columnIndex,
                                formatter,
                                evaluator)));
                }

                if (values.Contains("districtcode") &&
                    values.Contains("districtnameinenglish"))
                {
                    return rowIndex;
                }
            }

            return -1;
        }

        private static IDictionary<string, int> BuildHeaderMap(
            IRow headerRow,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var headers = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            if (headerRow == null)
                return headers;

            for (var columnIndex = 0;
                 columnIndex < headerRow.LastCellNum;
                 columnIndex++)
            {
                var originalHeader = GetCellText(
                    headerRow,
                    columnIndex,
                    formatter,
                    evaluator);

                var normalizedHeader = NormalizeHeader(originalHeader);

                if (String.IsNullOrWhiteSpace(normalizedHeader))
                    continue;

                if (headers.ContainsKey(normalizedHeader))
                {
                    throw new InvalidDataException(
                        "Duplicate XLSX column header: " + originalHeader);
                }

                headers.Add(normalizedHeader, columnIndex);
            }

            return headers;
        }

        private static void ValidateDistrictHeaders(
            IDictionary<string, int> headers)
        {
            foreach (var requiredHeader in RequiredDistrictHeaders)
            {
                if (!headers.ContainsKey(requiredHeader))
                {
                    throw new InvalidDataException(
                        "Required LGD column was not found: " +
                        requiredHeader);
                }
            }
        }

        private static DistrictImportRow ReadDistrictRow(
            IRow row,
            IDictionary<string, int> headers,
            int stateId,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var districtCode = ReadCode(
                row,
                headers,
                "districtcode",
                formatter,
                evaluator);

            var districtNameEnglish = ReadText(
                row,
                headers,
                "districtnameinenglish",
                formatter,
                evaluator);

            if (String.IsNullOrWhiteSpace(districtCode))
                throw new InvalidDataException("District Code is required.");

            if (String.IsNullOrWhiteSpace(districtNameEnglish))
            {
                throw new InvalidDataException(
                    "District Name (In English) is required.");
            }

            return new DistrictImportRow
            {
                StateId = stateId,
                Code = districtCode,
                NameEnglish = districtNameEnglish.Trim(),
                NameHindi = ReadText(
                    row,
                    headers,
                    "districtnameinlocal",
                    formatter,
                    evaluator),
                LGDVersion = ReadNullableInt(
                    row,
                    headers,
                    "districtversion",
                    formatter,
                    evaluator),
                Census2001Code = ReadCode(
                    row,
                    headers,
                    "census2001code",
                    formatter,
                    evaluator),
                Census2011Code = ReadCode(
                    row,
                    headers,
                    "census2011code",
                    formatter,
                    evaluator)
            };
        }

        private static bool IsEmptyRow(
            IRow row,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            if (row == null)
                return true;

            for (var columnIndex = row.FirstCellNum < 0 ? 0 : row.FirstCellNum;
                 columnIndex < row.LastCellNum;
                 columnIndex++)
            {
                if (!String.IsNullOrWhiteSpace(
                    GetCellText(
                        row,
                        columnIndex,
                        formatter,
                        evaluator)))
                {
                    return false;
                }
            }

            return true;
        }

        private static string ReadText(
            IRow row,
            IDictionary<string, int> headers,
            string normalizedHeader,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            int columnIndex;

            if (!headers.TryGetValue(normalizedHeader, out columnIndex))
                return null;

            var value = GetCellText(
                row,
                columnIndex,
                formatter,
                evaluator);

            return String.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string ReadCode(
            IRow row,
            IDictionary<string, int> headers,
            string normalizedHeader,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var value = ReadText(
                row,
                headers,
                normalizedHeader,
                formatter,
                evaluator);

            if (String.IsNullOrWhiteSpace(value))
                return null;

            decimal numericValue;

            if (Decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out numericValue))
            {
                return numericValue.ToString(
                    "0",
                    CultureInfo.InvariantCulture);
            }

            return value.Trim();
        }

        private static int? ReadNullableInt(
            IRow row,
            IDictionary<string, int> headers,
            string normalizedHeader,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var value = ReadCode(
                row,
                headers,
                normalizedHeader,
                formatter,
                evaluator);

            if (String.IsNullOrWhiteSpace(value))
                return null;

            int number;

            return Int32.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out number)
                ? number
                : (int?)null;
        }

        private static string GetCellText(
            IRow row,
            int columnIndex,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            if (row == null)
                return null;

            var cell = row.GetCell(
                columnIndex,
                MissingCellPolicy.RETURN_BLANK_AS_NULL);

            if (cell == null)
                return null;

            var text = formatter.FormatCellValue(cell, evaluator);

            return String.IsNullOrWhiteSpace(text)
                ? null
                : text.Trim();
        }

        private static void SaveDistrict(
            SqlConnection connection,
            SqlTransaction transaction,
            DistrictImportRow row,
            bool updateExisting,
            string sourceName,
            int userId,
            GovernmentImportResultVM result)
        {
            var existingDistrictId = FindDistrictId(
                connection,
                transaction,
                row.StateId,
                row.Code);

            if (existingDistrictId.HasValue)
            {
                if (!updateExisting)
                {
                    result.Skipped++;
                    return;
                }

                UpdateDistrict(
                    connection,
                    transaction,
                    existingDistrictId.Value,
                    row,
                    sourceName,
                    userId);

                result.Updated++;
                return;
            }

            InsertDistrict(
                connection,
                transaction,
                row,
                sourceName,
                userId);

            result.Inserted++;
        }

        private static int? FindDistrictId(
            SqlConnection connection,
            SqlTransaction transaction,
            int stateId,
            string code)
        {
            const string sql = @"
SELECT TOP 1 DistrictId
FROM dbo.DistrictMaster
WHERE StateId = @StateId
  AND IsDeleted = 0
  AND
  (
      Code = @Code
      OR
      (
          TRY_CONVERT(INT, Code) IS NOT NULL
          AND TRY_CONVERT(INT, @Code) IS NOT NULL
          AND TRY_CONVERT(INT, Code) = TRY_CONVERT(INT, @Code)
      )
  );";

            using (var command = new SqlCommand(
                sql,
                connection,
                transaction))
            {
                command.Parameters.Add(
                    "@StateId",
                    SqlDbType.Int).Value = stateId;

                command.Parameters.Add(
                    "@Code",
                    SqlDbType.NVarChar,
                    20).Value = code;

                var value = command.ExecuteScalar();

                return value == null || value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(value);
            }
        }

        private static void InsertDistrict(
            SqlConnection connection,
            SqlTransaction transaction,
            DistrictImportRow row,
            string sourceName,
            int userId)
        {
            const string sql = @"
INSERT INTO dbo.DistrictMaster
(
    StateId,
    Code,
    NameEnglish,
    NameHindi,
    LGDVersion,
    Census2001Code,
    Census2011Code,
    SourceName,
    IsActive,
    IsDeleted,
    CreatedBy,
    CreatedDate
)
VALUES
(
    @StateId,
    @Code,
    @NameEnglish,
    @NameHindi,
    @LGDVersion,
    @Census2001Code,
    @Census2011Code,
    @SourceName,
    1,
    0,
    @UserId,
    GETDATE()
);";

            using (var command = new SqlCommand(
                sql,
                connection,
                transaction))
            {
                AddDistrictParameters(
                    command,
                    row,
                    sourceName,
                    userId);

                command.ExecuteNonQuery();
            }
        }

        private static void UpdateDistrict(
            SqlConnection connection,
            SqlTransaction transaction,
            int districtId,
            DistrictImportRow row,
            string sourceName,
            int userId)
        {
            const string sql = @"
UPDATE dbo.DistrictMaster
SET
    StateId = @StateId,
    Code = @Code,
    NameEnglish = @NameEnglish,
    NameHindi = @NameHindi,
    LGDVersion = @LGDVersion,
    Census2001Code = @Census2001Code,
    Census2011Code = @Census2011Code,
    SourceName = @SourceName,
    IsActive = 1,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE DistrictId = @DistrictId
  AND IsDeleted = 0;";

            using (var command = new SqlCommand(
                sql,
                connection,
                transaction))
            {
                AddDistrictParameters(
                    command,
                    row,
                    sourceName,
                    userId);

                command.Parameters.Add(
                    "@DistrictId",
                    SqlDbType.Int).Value = districtId;

                command.ExecuteNonQuery();
            }
        }

        private static void AddDistrictParameters(
            SqlCommand command,
            DistrictImportRow row,
            string sourceName,
            int userId)
        {
            command.Parameters.Add(
                "@StateId",
                SqlDbType.Int).Value = row.StateId;

            command.Parameters.Add(
                "@Code",
                SqlDbType.NVarChar,
                20).Value = row.Code;

            command.Parameters.Add(
                "@NameEnglish",
                SqlDbType.NVarChar,
                200).Value = row.NameEnglish;

            command.Parameters.Add(
                "@NameHindi",
                SqlDbType.NVarChar,
                200).Value = DbValue(row.NameHindi);

            command.Parameters.Add(
                "@LGDVersion",
                SqlDbType.Int).Value = DbValue(row.LGDVersion);

            command.Parameters.Add(
                "@Census2001Code",
                SqlDbType.NVarChar,
                20).Value = DbValue(row.Census2001Code);

            command.Parameters.Add(
                "@Census2011Code",
                SqlDbType.NVarChar,
                20).Value = DbValue(row.Census2011Code);

            command.Parameters.Add(
                "@SourceName",
                SqlDbType.NVarChar,
                100).Value =
                    String.IsNullOrWhiteSpace(sourceName)
                        ? "LGD"
                        : sourceName.Trim();

            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int).Value = userId;
        }

        private static void SaveHistory(
            SqlConnection connection,
            SqlTransaction transaction,
            string sourceName,
            string fileName,
            GovernmentImportResultVM result,
            int userId)
        {
            const string sql = @"
INSERT INTO dbo.GeographyImportHistory
(
    EntityType,
    SourceName,
    FileName,
    InsertedCount,
    UpdatedCount,
    ErrorCount,
    ImportedBy,
    ImportedDate
)
VALUES
(
    N'District',
    @SourceName,
    @FileName,
    @InsertedCount,
    @UpdatedCount,
    @ErrorCount,
    @ImportedBy,
    GETDATE()
);";

            using (var command = new SqlCommand(
                sql,
                connection,
                transaction))
            {
                command.Parameters.Add(
                    "@SourceName",
                    SqlDbType.NVarChar,
                    100).Value =
                        String.IsNullOrWhiteSpace(sourceName)
                            ? "LGD"
                            : sourceName.Trim();

                command.Parameters.Add(
                    "@FileName",
                    SqlDbType.NVarChar,
                    260).Value = DbValue(fileName);

                command.Parameters.Add(
                    "@InsertedCount",
                    SqlDbType.Int).Value = result.Inserted;

                command.Parameters.Add(
                    "@UpdatedCount",
                    SqlDbType.Int).Value = result.Updated;

                command.Parameters.Add(
                    "@ErrorCount",
                    SqlDbType.Int).Value = result.Failed;

                command.Parameters.Add(
                    "@ImportedBy",
                    SqlDbType.Int).Value = userId;

                command.ExecuteNonQuery();
            }
        }

        private static bool CodesEqual(string left, string right)
        {
            left = (left ?? String.Empty).Trim();
            right = (right ?? String.Empty).Trim();

            int leftNumber;
            int rightNumber;

            if (Int32.TryParse(left, out leftNumber) &&
                Int32.TryParse(right, out rightNumber))
            {
                return leftNumber == rightNumber;
            }

            return String.Equals(
                left,
                right,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeHeader(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            return new String(
                value
                    .Trim()
                    .Where(Char.IsLetterOrDigit)
                    .ToArray())
                .ToLowerInvariant();
        }

        private static object DbValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            var text = value as string;

            if (text != null && String.IsNullOrWhiteSpace(text))
                return DBNull.Value;

            return value;
        }

        private sealed class StateLookup
        {
            public int StateId { get; set; }
            public string Code { get; set; }
            public string NameEnglish { get; set; }
        }

        private sealed class DistrictImportRow
        {
            public int StateId { get; set; }
            public string Code { get; set; }
            public string NameEnglish { get; set; }
            public string NameHindi { get; set; }
            public int? LGDVersion { get; set; }
            public string Census2001Code { get; set; }
            public string Census2011Code { get; set; }
        }
    }
}