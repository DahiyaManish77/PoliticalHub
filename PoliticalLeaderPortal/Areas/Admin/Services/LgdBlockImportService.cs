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

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    /// <summary>
    /// Imports the official LGD "All Development Blocks of State" XLSX report.
    /// District mapping is resolved from DistrictMaster using the official LGD District Code.
    /// Existing blocks are matched by StateId + official Development Block Code.
    /// </summary>
    public sealed class LgdBlockImportService
    {
        private readonly string _connectionString;

        private static readonly string[] RequiredHeaders =
        {
            "districtcode",
            "developmentblockcode",
            "developmentblocknameinenglish"
        };

        public LgdBlockImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                _connectionString = db.Database.Connection.ConnectionString;
            }
        }

        public GovernmentImportResultVM ImportOfficialBlockXlsx(
            Stream xlsxStream,
            string fileName,
            int selectedStateId,
            bool updateExisting,
            string sourceName,
            int userId)
        {
            if (xlsxStream == null)
                throw new ArgumentNullException("xlsxStream");

            if (String.IsNullOrWhiteSpace(fileName))
                throw new InvalidDataException("Uploaded file name is missing.");

            if (!String.Equals(
                Path.GetExtension(fileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Please upload the official LGD Development Block XLSX report.");
            }

            var selectedState = GetState(selectedStateId);

            if (selectedState == null)
            {
                throw new InvalidDataException(
                    "The selected State/UT was not found in StateMaster.");
            }

            if (xlsxStream.CanSeek)
                xlsxStream.Position = 0;

            IWorkbook workbook;

            try
            {
                workbook = new XSSFWorkbook(xlsxStream);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    "The workbook could not be opened as XLSX. " +
                    "Use the normalized LGD workbook supplied with this update. " +
                    "Details: " + ex.Message,
                    ex);
            }

            var result = new GovernmentImportResultVM
            {
                FileName = fileName,
                EntityType = "Development Block",
                ReportStateCode = selectedState.Code,
                ReportStateName = selectedState.NameEnglish
            };

            using (workbook)
            {
                if (workbook.NumberOfSheets == 0)
                    throw new InvalidDataException("The workbook contains no worksheet.");

                var sheet = workbook.GetSheetAt(0);

                if (sheet == null)
                    throw new InvalidDataException("The first worksheet could not be read.");

                var formatter = new DataFormatter(CultureInfo.InvariantCulture);
                var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

                var title = GetCellText(
                    sheet.GetRow(0),
                    0,
                    formatter,
                    evaluator);

                var reportStateName = ExtractStateName(title);

                if (String.IsNullOrWhiteSpace(reportStateName))
                {
                    throw new InvalidDataException(
                        "The State name could not be detected from the LGD report title.");
                }

                if (!StateNamesEqual(
                    reportStateName,
                    selectedState.NameEnglish))
                {
                    throw new InvalidDataException(
                        "The uploaded report belongs to " +
                        reportStateName +
                        ", but the selected State is " +
                        selectedState.NameEnglish +
                        " (" + selectedState.Code + ").");
                }

                result.ReportStateName = selectedState.NameEnglish;
                result.ReportStateCode = selectedState.Code;

                var headerRowIndex = FindHeaderRow(
                    sheet,
                    formatter,
                    evaluator);

                if (headerRowIndex < 0)
                {
                    throw new InvalidDataException(
                        "The official LGD Development Block header row could not be found.");
                }

                var headers = BuildHeaderMap(
                    sheet.GetRow(headerRowIndex),
                    formatter,
                    evaluator);

                ValidateHeaders(headers);

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
                                    var importRow = ReadRow(
                                        row,
                                        headers,
                                        selectedState.StateId,
                                        formatter,
                                        evaluator);

                                    importRow.DistrictId = FindDistrictId(
                                        connection,
                                        transaction,
                                        selectedState.StateId,
                                        importRow.DistrictCode);

                                    if (!importRow.DistrictId.HasValue)
                                    {
                                        throw new InvalidDataException(
                                            "District LGD Code " +
                                            importRow.DistrictCode +
                                            " was not found in DistrictMaster. " +
                                            "Import Districts first.");
                                    }

                                    SaveBlock(
                                        connection,
                                        transaction,
                                        importRow,
                                        updateExisting,
                                        sourceName,
                                        userId,
                                        result);
                                }
                                catch (Exception ex)
                                {
                                    result.Failed++;

                                    if (result.Errors != null &&
                                        result.Errors.Count < 200)
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
                        NameEnglish = Convert.ToString(reader["NameEnglish"]).Trim()
                    };
                }
            }
        }

        private static int? FindDistrictId(
            SqlConnection connection,
            SqlTransaction transaction,
            int stateId,
            string districtCode)
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

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                command.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value =
                    districtCode;

                var value = command.ExecuteScalar();

                return value == null || value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(value);
            }
        }

        private static void SaveBlock(
            SqlConnection connection,
            SqlTransaction transaction,
            BlockImportRow row,
            bool updateExisting,
            string sourceName,
            int userId,
            GovernmentImportResultVM result)
        {
            var existingId = FindBlockId(
                connection,
                transaction,
                row.StateId,
                row.Code);

            if (existingId.HasValue)
            {
                if (!updateExisting)
                {
                    result.Skipped++;
                    return;
                }

                UpdateBlock(
                    connection,
                    transaction,
                    existingId.Value,
                    row,
                    sourceName,
                    userId);

                result.Updated++;
                return;
            }

            InsertBlock(
                connection,
                transaction,
                row,
                sourceName,
                userId);

            result.Inserted++;
        }

        private static int? FindBlockId(
            SqlConnection connection,
            SqlTransaction transaction,
            int stateId,
            string code)
        {
            const string sql = @"
SELECT TOP 1 BlockId
FROM dbo.BlockMaster
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

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                command.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code;

                var value = command.ExecuteScalar();

                return value == null || value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(value);
            }
        }

        private static void InsertBlock(
            SqlConnection connection,
            SqlTransaction transaction,
            BlockImportRow row,
            string sourceName,
            int userId)
        {
            const string sql = @"
INSERT INTO dbo.BlockMaster
(
    StateId,
    DistrictId,
    Code,
    NameEnglish,
    NameHindi,
    LGDVersion,
    SourceName,
    IsActive,
    IsDeleted,
    CreatedBy,
    CreatedDate
)
VALUES
(
    @StateId,
    @DistrictId,
    @Code,
    @NameEnglish,
    @NameHindi,
    @LGDVersion,
    @SourceName,
    1,
    0,
    @UserId,
    GETDATE()
);";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                AddParameters(command, row, sourceName, userId);
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateBlock(
            SqlConnection connection,
            SqlTransaction transaction,
            int blockId,
            BlockImportRow row,
            string sourceName,
            int userId)
        {
            const string sql = @"
UPDATE dbo.BlockMaster
SET
    StateId = @StateId,
    DistrictId = @DistrictId,
    Code = @Code,
    NameEnglish = @NameEnglish,
    NameHindi = @NameHindi,
    LGDVersion = @LGDVersion,
    SourceName = @SourceName,
    IsActive = 1,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE BlockId = @BlockId
  AND IsDeleted = 0;";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                AddParameters(command, row, sourceName, userId);
                command.Parameters.Add("@BlockId", SqlDbType.Int).Value = blockId;
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(
            SqlCommand command,
            BlockImportRow row,
            string sourceName,
            int userId)
        {
            command.Parameters.Add("@StateId", SqlDbType.Int).Value = row.StateId;
            command.Parameters.Add("@DistrictId", SqlDbType.Int).Value =
                row.DistrictId.Value;
            command.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value =
                row.Code;
            command.Parameters.Add("@NameEnglish", SqlDbType.NVarChar, 200).Value =
                row.NameEnglish;
            command.Parameters.Add("@NameHindi", SqlDbType.NVarChar, 200).Value =
                DbValue(row.NameHindi);
            command.Parameters.Add("@LGDVersion", SqlDbType.Int).Value =
                DbValue(row.LGDVersion);
            command.Parameters.Add("@SourceName", SqlDbType.NVarChar, 100).Value =
                String.IsNullOrWhiteSpace(sourceName)
                    ? "LGD"
                    : sourceName.Trim();
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        }

        private static BlockImportRow ReadRow(
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

            var blockCode = ReadCode(
                row,
                headers,
                "developmentblockcode",
                formatter,
                evaluator);

            var nameEnglish = ReadText(
                row,
                headers,
                "developmentblocknameinenglish",
                formatter,
                evaluator);

            if (String.IsNullOrWhiteSpace(districtCode))
                throw new InvalidDataException("District Code is required.");

            if (String.IsNullOrWhiteSpace(blockCode))
                throw new InvalidDataException("Development Block Code is required.");

            if (String.IsNullOrWhiteSpace(nameEnglish))
            {
                throw new InvalidDataException(
                    "Development Block Name (In English) is required.");
            }

            return new BlockImportRow
            {
                StateId = stateId,
                DistrictCode = districtCode,
                Code = blockCode,
                NameEnglish = nameEnglish.Trim(),
                NameHindi = ReadText(
                    row,
                    headers,
                    "developmentblocknameinlocal",
                    formatter,
                    evaluator),
                LGDVersion = ReadNullableInt(
                    row,
                    headers,
                    "developmentblockversion",
                    formatter,
                    evaluator)
            };
        }

        private static int FindHeaderRow(
            ISheet sheet,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var lastRow = Math.Min(sheet.LastRowNum, 19);

            for (var rowIndex = 0; rowIndex <= lastRow; rowIndex++)
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
                    values.Contains("developmentblockcode") &&
                    values.Contains("developmentblocknameinenglish"))
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

            for (var columnIndex = 0;
                 headerRow != null && columnIndex < headerRow.LastCellNum;
                 columnIndex++)
            {
                var original = GetCellText(
                    headerRow,
                    columnIndex,
                    formatter,
                    evaluator);

                var normalized = NormalizeHeader(original);

                if (String.IsNullOrWhiteSpace(normalized))
                    continue;

                if (headers.ContainsKey(normalized))
                {
                    throw new InvalidDataException(
                        "Duplicate XLSX column header: " + original);
                }

                headers.Add(normalized, columnIndex);
            }

            return headers;
        }

        private static void ValidateHeaders(
            IDictionary<string, int> headers)
        {
            foreach (var requiredHeader in RequiredHeaders)
            {
                if (!headers.ContainsKey(requiredHeader))
                {
                    throw new InvalidDataException(
                        "Required LGD column was not found: " +
                        requiredHeader);
                }
            }
        }

        private static string ExtractStateName(string title)
        {
            if (String.IsNullOrWhiteSpace(title))
                return null;

            var match = Regex.Match(
                title.Trim(),
                @"All\s+Development\s+Blocks\s+of\s+(.+?)\s+State\s*$",
                RegexOptions.IgnoreCase);

            return match.Success
                ? match.Groups[1].Value.Trim()
                : null;
        }

        private static bool StateNamesEqual(
            string left,
            string right)
        {
            return String.Equals(
                NormalizeName(left),
                NormalizeName(right),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            return new String(
                value.Trim()
                     .Where(Char.IsLetterOrDigit)
                     .ToArray())
                .ToLowerInvariant();
        }

        private static string NormalizeHeader(string value)
        {
            return NormalizeName(value);
        }

        private static string ReadText(
            IRow row,
            IDictionary<string, int> headers,
            string header,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            int columnIndex;

            if (!headers.TryGetValue(header, out columnIndex))
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
            string header,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var value = ReadText(
                row,
                headers,
                header,
                formatter,
                evaluator);

            if (String.IsNullOrWhiteSpace(value))
                return null;

            decimal number;

            if (Decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out number))
            {
                return number.ToString(
                    "0",
                    CultureInfo.InvariantCulture);
            }

            return value.Trim();
        }

        private static int? ReadNullableInt(
            IRow row,
            IDictionary<string, int> headers,
            string header,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            var value = ReadCode(
                row,
                headers,
                header,
                formatter,
                evaluator);

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

            var value = formatter.FormatCellValue(
                cell,
                evaluator);

            return String.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool IsEmptyRow(
            IRow row,
            DataFormatter formatter,
            IFormulaEvaluator evaluator)
        {
            if (row == null)
                return true;

            var firstCell = row.FirstCellNum < 0
                ? 0
                : row.FirstCellNum;

            for (var columnIndex = firstCell;
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
    N'Block',
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

        private static object DbValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            var text = value as string;

            if (text != null &&
                String.IsNullOrWhiteSpace(text))
            {
                return DBNull.Value;
            }

            return value;
        }

        private sealed class StateLookup
        {
            public int StateId { get; set; }
            public string Code { get; set; }
            public string NameEnglish { get; set; }
        }

        private sealed class BlockImportRow
        {
            public int StateId { get; set; }
            public int? DistrictId { get; set; }
            public string DistrictCode { get; set; }
            public string Code { get; set; }
            public string NameEnglish { get; set; }
            public string NameHindi { get; set; }
            public int? LGDVersion { get; set; }
        }
    }
}
