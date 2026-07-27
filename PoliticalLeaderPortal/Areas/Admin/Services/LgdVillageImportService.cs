using NPOI.OpenXml4Net.OPC;
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
    /// Imports the official LGD "All Villages of a State" XLSX report.
    ///
    /// Verified LGD columns:
    /// District Code, District Name (In English), Sub-District Code,
    /// Sub-District Name (In English), Village Code, Village Version,
    /// Village Name (In English), Village Name (In Local), Village Category,
    /// Village Status, Census 2001 Code, Census 2011 Code and Remark.
    ///
    /// The report does not contain Development Block or Gram Panchayat codes.
    /// Villages are therefore mapped to State, District and Tehsil/Sub-District.
    /// </summary>
    public class LgdVillageImportService
    {
        private readonly string _connectionString;

        private static readonly string[] RequiredHeaders =
        {
            "districtcode",
            "districtnameinenglish",
            "subdistrictcode",
            "subdistrictnameinenglish",
            "villagecode",
            "villageversion",
            "villagenameinenglish",
            "villagecategory",
            "villagestatus"
        };

        public LgdVillageImportService()
        {
            using (var db = new PoliticalLeaderPortalDbEntities1())
            {
                _connectionString = db.Database.Connection.ConnectionString;
            }
        }

        public GovernmentImportResultVM ImportOfficialVillageXlsx(
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
                    "Please upload the official LGD All Villages of a State XLSX report.");
            }

            var selectedState = GetSelectedState(selectedStateId);

            if (selectedState == null)
                throw new InvalidDataException(
                    "The selected State/UT was not found in StateMaster.");

            var temporaryFilePath = String.Empty;

            try
            {
                temporaryFilePath = CopyToTemporaryXlsxFile(
                    xlsxStream,
                    fileName);

                IWorkbook workbook;

                try
                {
                    // Opening through OPCPackage by file path avoids the
                    // SharpZipLib ZipInputStream failure produced by some
                    // official LGD workbooks.
                    var package = OPCPackage.Open(
                        temporaryFilePath,
                        PackageAccess.READ);

                    workbook = new XSSFWorkbook(package);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(
                        "The uploaded XLSX package could not be opened. " +
                        "The file may be incomplete or the installed NPOI/" +
                        "SharpZipLib versions may be incompatible.",
                        ex);
                }

                using (workbook)
                {
                    if (workbook.NumberOfSheets == 0)
                        throw new InvalidDataException(
                            "The workbook contains no worksheet.");

                    var sheet = workbook.GetSheetAt(0);
                    var title = ReadCell(sheet.GetRow(0), 0);
                    var reportState = ParseReportState(title);

                    ValidateReportState(reportState, selectedState);

                    var headerRow = FindHeaderRow(sheet);

                    if (headerRow == null)
                    {
                        throw new InvalidDataException(
                            "The official Village report header was not found.");
                    }

                    var headerMap = BuildHeaderMap(headerRow);
                    ValidateHeaders(headerMap);

                    var result = new GovernmentImportResultVM
                    {
                        FileName = fileName,
                        EntityType = "Village",
                        ReportStateName = reportState.Name,
                        ReportStateCode = reportState.Code
                    };

                    ImportRows(
                        sheet,
                        headerRow.RowNum + 1,
                        headerMap,
                        selectedState,
                        updateExisting,
                        String.IsNullOrWhiteSpace(sourceName)
                            ? "LGD"
                            : sourceName.Trim(),
                        userId,
                        result);

                    return result;
                }
            }
            finally
            {
                DeleteTemporaryFile(temporaryFilePath);
            }
        }

        private static string CopyToTemporaryXlsxFile(
            Stream source,
            string originalFileName)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var temporaryFilePath = Path.Combine(
                Path.GetTempPath(),
                "LGD_Village_" +
                Guid.NewGuid().ToString("N") +
                Path.GetExtension(originalFileName));

            if (source.CanSeek)
                source.Position = 0;

            using (var destination = new FileStream(
                temporaryFilePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.SequentialScan))
            {
                source.CopyTo(destination);
                destination.Flush();
            }

            var fileInfo = new FileInfo(temporaryFilePath);

            if (!fileInfo.Exists || fileInfo.Length < 4)
            {
                DeleteTemporaryFile(temporaryFilePath);

                throw new InvalidDataException(
                    "The uploaded XLSX file is empty or incomplete.");
            }

            using (var validationStream = new FileStream(
                temporaryFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                var firstByte = validationStream.ReadByte();
                var secondByte = validationStream.ReadByte();

                // XLSX is an Open XML ZIP package and normally starts with PK.
                if (firstByte != 0x50 || secondByte != 0x4B)
                {
                    DeleteTemporaryFile(temporaryFilePath);

                    throw new InvalidDataException(
                        "The uploaded file is not a valid XLSX Open XML package.");
                }
            }

            return temporaryFilePath;
        }

        private static void DeleteTemporaryFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // Temporary-file cleanup must not hide the import result.
            }
        }

        private void ImportRows(
            ISheet sheet,
            int firstDataRow,
            IDictionary<string, int> headerMap,
            StateInfo selectedState,
            bool updateExisting,
            string sourceName,
            int userId,
            GovernmentImportResultVM result)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var districtMap = LoadCodeMap(
                    connection,
                    "DistrictMaster",
                    "DistrictId",
                    "Code",
                    "StateId",
                    selectedState.Id);

                var tehsilMap = LoadCodeMap(
                    connection,
                    "TehsilMaster",
                    "TehsilId",
                    "Code",
                    "StateId",
                    selectedState.Id);

                if (districtMap.Count == 0)
                {
                    throw new InvalidDataException(
                        "No active districts were found for the selected State. Import Districts first.");
                }

                if (tehsilMap.Count == 0)
                {
                    throw new InvalidDataException(
                        "No active Sub-Districts/Tehsils were found for the selected State. Import Sub-Districts first.");
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        CreateStagingTable(connection, transaction);

                        var batch = CreateVillageDataTable();
                        var seenCodes = new HashSet<string>(
                            StringComparer.OrdinalIgnoreCase);

                        for (var rowIndex = firstDataRow;
                             rowIndex <= sheet.LastRowNum;
                             rowIndex++)
                        {
                            var row = sheet.GetRow(rowIndex);

                            if (row == null || IsEmptyRow(row))
                                continue;

                            result.TotalRows++;

                            try
                            {
                                var districtCode = Code(
                                    Cell(row, headerMap, "districtcode"));

                                var tehsilCode = Code(
                                    Cell(row, headerMap, "subdistrictcode"));

                                var villageCode = Code(
                                    Cell(row, headerMap, "villagecode"));

                                var villageName = Text(
                                    Cell(row, headerMap, "villagenameinenglish"));

                                if (String.IsNullOrWhiteSpace(villageCode))
                                    throw new InvalidDataException("Village Code is missing.");

                                if (String.IsNullOrWhiteSpace(villageName))
                                    throw new InvalidDataException("Village Name (In English) is missing.");

                                if (!seenCodes.Add(villageCode))
                                    throw new InvalidDataException(
                                        "Duplicate Village Code exists in the workbook.");

                                int districtId;

                                if (!districtMap.TryGetValue(districtCode, out districtId))
                                {
                                    throw new InvalidDataException(
                                        "District Code " + districtCode +
                                        " was not found in DistrictMaster.");
                                }

                                int tehsilId;

                                if (!tehsilMap.TryGetValue(tehsilCode, out tehsilId))
                                {
                                    throw new InvalidDataException(
                                        "Sub-District Code " + tehsilCode +
                                        " was not found in TehsilMaster.");
                                }

                                var dataRow = batch.NewRow();
                                dataRow["StateId"] = selectedState.Id;
                                dataRow["DistrictId"] = districtId;
                                dataRow["TehsilId"] = tehsilId;
                                dataRow["Code"] = villageCode;
                                dataRow["NameEnglish"] = Limit(villageName, 250);
                                dataRow["NameHindi"] = DbValue(
                                    Limit(Text(Cell(row, headerMap, "villagenameinlocal")), 250));
                                dataRow["LGDVersion"] = DbInt(
                                    Cell(row, headerMap, "villageversion"));
                                dataRow["VillageCategory"] = DbValue(
                                    Limit(Text(Cell(row, headerMap, "villagecategory")), 50));
                                dataRow["VillageStatus"] = DbValue(
                                    Limit(Text(Cell(row, headerMap, "villagestatus")), 50));
                                dataRow["AreaType"] = DbValue(
                                    Limit(Text(Cell(row, headerMap, "villagecategory")), 50));
                                dataRow["Census2001Code"] = DbValue(
                                    Limit(Code(Cell(row, headerMap, "census2001code")), 50));
                                dataRow["Census2011Code"] = DbValue(
                                    Limit(Code(Cell(row, headerMap, "census2011code")), 50));
                                dataRow["Remark"] = DbValue(
                                    Limit(Text(Cell(row, headerMap, "remark")), 500));
                                dataRow["SourceName"] = Limit(sourceName, 50);
                                dataRow["UserId"] = userId;

                                batch.Rows.Add(dataRow);

                                if (batch.Rows.Count >= 5000)
                                {
                                    BulkCopyBatch(
                                        connection,
                                        transaction,
                                        batch);

                                    batch.Clear();
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Failed++;

                                if (result.Errors.Count < 200)
                                {
                                    result.Errors.Add(
                                        "Row " + (rowIndex + 1).ToString(
                                            CultureInfo.InvariantCulture) +
                                        ": " + ex.Message);
                                }
                            }
                        }

                        if (batch.Rows.Count > 0)
                        {
                            BulkCopyBatch(
                                connection,
                                transaction,
                                batch);
                        }

                        ApplyStagingData(
                            connection,
                            transaction,
                            updateExisting,
                            result);

                        LogImportHistory(
                            connection,
                            transaction,
                            selectedState.Id,
                            fileName: result.FileName,
                            sourceName: sourceName,
                            userId: userId,
                            result: result);

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

        private static void CreateStagingTable(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string sql = @"
CREATE TABLE #VillageImport
(
    StateId            INT            NOT NULL,
    DistrictId         INT            NOT NULL,
    TehsilId           INT            NOT NULL,
    Code                NVARCHAR(30)   NOT NULL,
    NameEnglish         NVARCHAR(250)  NOT NULL,
    NameHindi           NVARCHAR(250)  NULL,
    LGDVersion          INT            NULL,
    VillageCategory     NVARCHAR(50)   NULL,
    VillageStatus       NVARCHAR(50)   NULL,
    AreaType            NVARCHAR(50)   NULL,
    Census2001Code      NVARCHAR(50)   NULL,
    Census2011Code      NVARCHAR(50)   NULL,
    Remark              NVARCHAR(500)  NULL,
    SourceName          NVARCHAR(50)   NOT NULL,
    UserId              INT            NOT NULL
);

CREATE UNIQUE CLUSTERED INDEX IX_TempVillageImport_Code
ON #VillageImport(StateId, Code);";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 120;
                command.ExecuteNonQuery();
            }
        }

        private static DataTable CreateVillageDataTable()
        {
            var table = new DataTable();

            table.Columns.Add("StateId", typeof(int));
            table.Columns.Add("DistrictId", typeof(int));
            table.Columns.Add("TehsilId", typeof(int));
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("NameEnglish", typeof(string));
            table.Columns.Add("NameHindi", typeof(string));
            table.Columns.Add("LGDVersion", typeof(int));
            table.Columns.Add("VillageCategory", typeof(string));
            table.Columns.Add("VillageStatus", typeof(string));
            table.Columns.Add("AreaType", typeof(string));
            table.Columns.Add("Census2001Code", typeof(string));
            table.Columns.Add("Census2011Code", typeof(string));
            table.Columns.Add("Remark", typeof(string));
            table.Columns.Add("SourceName", typeof(string));
            table.Columns.Add("UserId", typeof(int));

            return table;
        }

        private static void BulkCopyBatch(
            SqlConnection connection,
            SqlTransaction transaction,
            DataTable batch)
        {
            using (var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.CheckConstraints |
                SqlBulkCopyOptions.TableLock,
                transaction))
            {
                bulkCopy.DestinationTableName = "#VillageImport";
                bulkCopy.BatchSize = 5000;
                bulkCopy.BulkCopyTimeout = 180;

                foreach (DataColumn column in batch.Columns)
                {
                    bulkCopy.ColumnMappings.Add(
                        column.ColumnName,
                        column.ColumnName);
                }

                bulkCopy.WriteToServer(batch);
            }
        }

        private static void ApplyStagingData(
            SqlConnection connection,
            SqlTransaction transaction,
            bool updateExisting,
            GovernmentImportResultVM result)
        {
            const string countExistingSql = @"
SELECT COUNT_BIG(1)
FROM #VillageImport S
INNER JOIN dbo.VillageMaster V
    ON V.StateId = S.StateId
   AND V.Code = S.Code
   AND V.IsDeleted = 0;";

            long existingCount;

            using (var countCommand = new SqlCommand(
                countExistingSql,
                connection,
                transaction))
            {
                countCommand.CommandTimeout = 180;
                existingCount = Convert.ToInt64(
                    countCommand.ExecuteScalar(),
                    CultureInfo.InvariantCulture);
            }

            var validRows = result.TotalRows - result.Failed;
            var newCount = validRows - existingCount;

            if (newCount < 0)
                newCount = 0;

            if (updateExisting)
            {
                const string updateSql = @"
UPDATE V
SET
    V.DistrictId       = S.DistrictId,
    V.TehsilId          = S.TehsilId,
    V.NameEnglish       = S.NameEnglish,
    V.NameHindi         = S.NameHindi,
    V.LGDVersion        = S.LGDVersion,
    V.VillageCategory   = S.VillageCategory,
    V.VillageStatus     = S.VillageStatus,
    V.AreaType          = S.AreaType,
    V.Census2001Code    = S.Census2001Code,
    V.Census2011Code    = S.Census2011Code,
    V.Remark            = S.Remark,
    V.SourceName        = S.SourceName,
    V.IsActive          = 1,
    V.UpdatedBy         = S.UserId,
    V.UpdatedDate       = GETDATE()
FROM dbo.VillageMaster V
INNER JOIN #VillageImport S
    ON V.StateId = S.StateId
   AND V.Code = S.Code
WHERE V.IsDeleted = 0;";

                using (var updateCommand = new SqlCommand(
                    updateSql,
                    connection,
                    transaction))
                {
                    updateCommand.CommandTimeout = 300;
                    result.Updated = updateCommand.ExecuteNonQuery();
                }
            }
            else
            {
                result.Skipped = Convert.ToInt32(existingCount);
            }

            const string insertSql = @"
INSERT INTO dbo.VillageMaster
(
    StateId,
    DistrictId,
    TehsilId,
    BlockId,
    GramPanchayatId,
    Code,
    NameEnglish,
    NameHindi,
    LGDVersion,
    VillageCategory,
    VillageStatus,
    AreaType,
    Census2001Code,
    Census2011Code,
    Remark,
    SourceName,
    IsActive,
    IsDeleted,
    CreatedBy,
    CreatedDate
)
SELECT
    S.StateId,
    S.DistrictId,
    S.TehsilId,
    NULL,
    NULL,
    S.Code,
    S.NameEnglish,
    S.NameHindi,
    S.LGDVersion,
    S.VillageCategory,
    S.VillageStatus,
    S.AreaType,
    S.Census2001Code,
    S.Census2011Code,
    S.Remark,
    S.SourceName,
    1,
    0,
    S.UserId,
    GETDATE()
FROM #VillageImport S
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.VillageMaster V
    WHERE V.StateId = S.StateId
      AND V.Code = S.Code
      AND V.IsDeleted = 0
);";

            using (var insertCommand = new SqlCommand(
                insertSql,
                connection,
                transaction))
            {
                insertCommand.CommandTimeout = 300;
                result.Inserted = insertCommand.ExecuteNonQuery();
            }
        }

        private static void LogImportHistory(
            SqlConnection connection,
            SqlTransaction transaction,
            int stateId,
            string fileName,
            string sourceName,
            int userId,
            GovernmentImportResultVM result)
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.GeographyImportHistory', N'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.GeographyImportHistory
    (
        EntityType,
        StateId,
        FileName,
        SourceName,
        TotalRows,
        InsertedRows,
        UpdatedRows,
        SkippedRows,
        FailedRows,
        ImportedBy,
        ImportedDate
    )
    VALUES
    (
        N'Village',
        @StateId,
        @FileName,
        @SourceName,
        @TotalRows,
        @InsertedRows,
        @UpdatedRows,
        @SkippedRows,
        @FailedRows,
        @ImportedBy,
        GETDATE()
    );
END;";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                command.Parameters.Add("@FileName", SqlDbType.NVarChar, 260).Value =
                    Limit(fileName, 260);
                command.Parameters.Add("@SourceName", SqlDbType.NVarChar, 50).Value =
                    Limit(sourceName, 50);
                command.Parameters.Add("@TotalRows", SqlDbType.Int).Value =
                    result.TotalRows;
                command.Parameters.Add("@InsertedRows", SqlDbType.Int).Value =
                    result.Inserted;
                command.Parameters.Add("@UpdatedRows", SqlDbType.Int).Value =
                    result.Updated;
                command.Parameters.Add("@SkippedRows", SqlDbType.Int).Value =
                    result.Skipped;
                command.Parameters.Add("@FailedRows", SqlDbType.Int).Value =
                    result.Failed;
                command.Parameters.Add("@ImportedBy", SqlDbType.Int).Value =
                    userId;

                command.ExecuteNonQuery();
            }
        }

        private StateInfo GetSelectedState(int stateId)
        {
            const string sql = @"
SELECT TOP (1)
    StateId,
    Code,
    NameEnglish
FROM dbo.StateMaster
WHERE StateId = @StateId
  AND IsDeleted = 0
  AND IsActive = 1;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value = stateId;
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new StateInfo
                    {
                        Id = Convert.ToInt32(reader["StateId"]),
                        Code = Code(reader["Code"]),
                        Name = Convert.ToString(reader["NameEnglish"])
                    };
                }
            }
        }

        private static IDictionary<string, int> LoadCodeMap(
            SqlConnection connection,
            string table,
            string idColumn,
            string codeColumn,
            string stateColumn,
            int stateId)
        {
            var result = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            var sql = String.Format(
                CultureInfo.InvariantCulture,
                @"SELECT {0}, {1}
FROM dbo.{2}
WHERE {3} = @StateId
  AND IsDeleted = 0
  AND IsActive = 1;",
                idColumn,
                codeColumn,
                table,
                stateColumn);

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StateId", SqlDbType.Int).Value =
                    stateId;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var code = Code(reader[codeColumn]);

                        if (!String.IsNullOrWhiteSpace(code))
                        {
                            result[code] = Convert.ToInt32(
                                reader[idColumn],
                                CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return result;
        }

        private static IRow FindHeaderRow(ISheet sheet)
        {
            var maxRows = Math.Min(sheet.LastRowNum, 20);

            for (var i = 0; i <= maxRows; i++)
            {
                var row = sheet.GetRow(i);

                if (row == null)
                    continue;

                var firstCellIndex = row.FirstCellNum < 0
                    ? 0
                    : Convert.ToInt32(row.FirstCellNum);

                var lastCellIndex = row.LastCellNum < 0
                    ? 0
                    : Convert.ToInt32(row.LastCellNum);

                var cellCount = lastCellIndex - firstCellIndex;

                if (cellCount <= 0)
                    continue;

                var normalized = new HashSet<string>(
                    Enumerable.Range(firstCellIndex, cellCount)
                        .Select(index => NormalizeHeader(ReadCell(row, index)))
                        .Where(value => !String.IsNullOrWhiteSpace(value)),
                    StringComparer.OrdinalIgnoreCase);

                if (RequiredHeaders.All(normalized.Contains))
                    return row;
            }

            return null;
        }

        private static IDictionary<string, int> BuildHeaderMap(IRow row)
        {
            var result = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < row.LastCellNum; index++)
            {
                var header = NormalizeHeader(ReadCell(row, index));

                if (!String.IsNullOrWhiteSpace(header) &&
                    !result.ContainsKey(header))
                {
                    result.Add(header, index);
                }
            }

            return result;
        }

        private static void ValidateHeaders(
            IDictionary<string, int> headerMap)
        {
            var missing = RequiredHeaders
                .Where(header => !headerMap.ContainsKey(header))
                .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidDataException(
                    "This is not the expected official LGD Village report. " +
                    "Missing columns: " + String.Join(", ", missing) + ".");
            }
        }

        private static object Cell(
            IRow row,
            IDictionary<string, int> map,
            string normalizedHeader)
        {
            int index;

            if (!map.TryGetValue(normalizedHeader, out index))
                return null;

            var cell = row.GetCell(index);

            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric)
                return cell.NumericCellValue;

            if (cell.CellType == CellType.Boolean)
                return cell.BooleanCellValue;

            if (cell.CellType == CellType.Formula)
            {
                if (cell.CachedFormulaResultType == CellType.Numeric)
                    return cell.NumericCellValue;

                return cell.ToString();
            }

            return cell.ToString();
        }

        private static string ReadCell(IRow row, int index)
        {
            if (row == null)
                return String.Empty;

            var cell = row.GetCell(index);

            return cell == null
                ? String.Empty
                : cell.ToString().Trim();
        }

        private static bool IsEmptyRow(IRow row)
        {
            if (row == null)
                return true;

            var firstCellIndex = row.FirstCellNum < 0
                ? 0
                : Convert.ToInt32(row.FirstCellNum);

            var lastCellIndex = row.LastCellNum < 0
                ? 0
                : Convert.ToInt32(row.LastCellNum);

            for (var index = firstCellIndex;
                 index < lastCellIndex;
                 index++)
            {
                var cell = row.GetCell(index);

                if (cell != null &&
                    !String.IsNullOrWhiteSpace(cell.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeHeader(string value)
        {
            return Regex.Replace(
                value ?? String.Empty,
                @"[^a-z0-9]",
                String.Empty,
                RegexOptions.IgnoreCase)
                .ToLowerInvariant();
        }

        private static string Text(object value)
        {
            return Convert.ToString(
                value,
                CultureInfo.InvariantCulture).Trim();
        }

        private static string Code(object value)
        {
            if (value == null || value == DBNull.Value)
                return String.Empty;

            if (value is double)
            {
                return Convert.ToInt64(
                    Math.Round((double)value),
                    CultureInfo.InvariantCulture)
                    .ToString(CultureInfo.InvariantCulture);
            }

            if (value is float ||
                value is decimal ||
                value is int ||
                value is long ||
                value is short)
            {
                return Convert.ToDecimal(
                    value,
                    CultureInfo.InvariantCulture)
                    .ToString("0", CultureInfo.InvariantCulture);
            }

            var text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture).Trim();

            decimal numeric;

            if (Decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out numeric))
            {
                return numeric.ToString(
                    "0",
                    CultureInfo.InvariantCulture);
            }

            return text;
        }

        private static object DbValue(string value)
        {
            return String.IsNullOrWhiteSpace(value)
                ? (object)DBNull.Value
                : value;
        }

        private static object DbInt(object value)
        {
            var text = Code(value);
            int number;

            return Int32.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out number)
                ? (object)number
                : DBNull.Value;
        }

        private static string Limit(string value, int maxLength)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }

        private static ReportStateInfo ParseReportState(string title)
        {
            var match = Regex.Match(
                title ?? String.Empty,
                @"All\s+Villages\s+of\s+(?<name>.+?)\s+\(State\s+Code:\s*(?<code>\d+)\)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                throw new InvalidDataException(
                    "The workbook title is not the official LGD " +
                    "\"All Villages of a State\" report.");
            }

            return new ReportStateInfo
            {
                Name = match.Groups["name"].Value.Trim(),
                Code = Code(match.Groups["code"].Value)
            };
        }

        private static void ValidateReportState(
            ReportStateInfo reportState,
            StateInfo selectedState)
        {
            if (!String.Equals(
                Code(reportState.Code),
                Code(selectedState.Code),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The uploaded workbook belongs to " +
                    reportState.Name + " (" + reportState.Code +
                    "), but the selected State is " +
                    selectedState.Name + " (" + selectedState.Code + ").");
            }
        }

        private sealed class StateInfo
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        private sealed class ReportStateInfo
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}