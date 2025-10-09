using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace DesktopApplicationTemplate.BuildDiagnostics;

public sealed class ExcelErrorTrackingService : IErrorTrackingService
{
    private const string ErrorsByFileSheetName = "ErrorsByFile";
    private const string ErrorsByCodeSheetName = "ErrorsByCode";
    private const string DetailedErrorsSheetName = "DetailedErrors";

    private readonly string _errorsDirectory;
    private readonly string _buildErrorsWorkbookPath;
    private readonly string _detailedErrorsWorkbookPath;
    private readonly object _syncRoot = new();

    public ExcelErrorTrackingService(string? rootDirectory = null)
    {
        var baseDirectory = string.IsNullOrWhiteSpace(rootDirectory)
            ? AppContext.BaseDirectory
            : rootDirectory!;

        _errorsDirectory = Path.Combine(baseDirectory, "Build Errors");
        _buildErrorsWorkbookPath = Path.Combine(_errorsDirectory, "BuildErrors.xlsx");
        _detailedErrorsWorkbookPath = Path.Combine(_errorsDirectory, "Detailed Errors.xlsx");
    }

    public void RecordBuildError(string errorCode, string filePath, string description)
    {
        var normalizedErrorCode = NormalizeErrorCode(errorCode);
        var normalizedFileName = NormalizeFileName(filePath);
        var normalizedDescription = NormalizeDescription(description);

        lock (_syncRoot)
        {
            EnsureDirectoryExists();
            UpdateBuildErrorsWorkbook(normalizedErrorCode, normalizedFileName);
            UpdateDetailedErrorsWorkbook(normalizedErrorCode, normalizedFileName, normalizedDescription);
        }
    }

    public void RecordRuntimeException(Exception exception, string? filePath)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var errorCode = NormalizeErrorCode(exception.GetType().FullName ?? exception.GetType().Name);
        var fileName = NormalizeFileName(filePath);
        var description = NormalizeDescription(exception.ToString());

        lock (_syncRoot)
        {
            EnsureDirectoryExists();
            UpdateBuildErrorsWorkbook(errorCode, fileName);
            UpdateDetailedErrorsWorkbook(errorCode, fileName, description);
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_errorsDirectory))
        {
            Directory.CreateDirectory(_errorsDirectory);
        }
    }

    private void UpdateBuildErrorsWorkbook(string errorCode, string fileName)
    {
        using var workbook = LoadOrCreateWorkbook(_buildErrorsWorkbookPath);
        var errorsByFile = GetOrCreateWorksheet(workbook, ErrorsByFileSheetName, "Error Code", "File Name", "Count");
        var errorsByCode = GetOrCreateWorksheet(workbook, ErrorsByCodeSheetName, "Error Code", "Count");

        IncrementCount(errorsByFile, new[] { errorCode, fileName });
        IncrementCount(errorsByCode, new[] { errorCode });

        workbook.SaveAs(_buildErrorsWorkbookPath);
    }

    private void UpdateDetailedErrorsWorkbook(string errorCode, string fileName, string description)
    {
        using var workbook = LoadOrCreateWorkbook(_detailedErrorsWorkbookPath);
        var worksheet = GetOrCreateWorksheet(workbook, DetailedErrorsSheetName, "Error Code", "File Name", "Description", "Count");

        IncrementCount(worksheet, new[] { errorCode, fileName, description });

        workbook.SaveAs(_detailedErrorsWorkbookPath);
    }

    private static XLWorkbook LoadOrCreateWorkbook(string path)
    {
        return File.Exists(path) ? new XLWorkbook(path) : new XLWorkbook();
    }

    private static IXLWorksheet GetOrCreateWorksheet(XLWorkbook workbook, string worksheetName, params string[] headers)
    {
        if (!workbook.TryGetWorksheet(worksheetName, out var worksheet))
        {
            worksheet = workbook.Worksheets.Add(worksheetName);
            for (var index = 0; index < headers.Length; index++)
            {
                worksheet.Cell(1, index + 1).Value = headers[index];
            }
        }

        return worksheet;
    }

    private static void IncrementCount(IXLWorksheet worksheet, IReadOnlyList<string> keyValues)
    {
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
        {
            if (!RowMatches(worksheet, row, keyValues))
            {
                continue;
            }

            var countCell = worksheet.Cell(row, keyValues.Count + 1);
            var currentValue = countCell.GetValue<int>();
            countCell.Value = currentValue + 1;
            return;
        }

        var newRow = lastRow + 1;
        for (var column = 0; column < keyValues.Count; column++)
        {
            worksheet.Cell(newRow, column + 1).Value = keyValues[column];
        }

        worksheet.Cell(newRow, keyValues.Count + 1).Value = 1;
    }

    private static bool RowMatches(IXLWorksheet worksheet, int rowNumber, IReadOnlyList<string> keyValues)
    {
        for (var column = 0; column < keyValues.Count; column++)
        {
            var cellValue = worksheet.Cell(rowNumber, column + 1).GetString();
            if (!string.Equals(cellValue, keyValues[column], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeErrorCode(string errorCode)
    {
        return string.IsNullOrWhiteSpace(errorCode)
            ? "UNKNOWN"
            : errorCode.Trim();
    }

    private static string NormalizeFileName(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "Unknown";
        }

        var fileName = Path.GetFileName(filePath);
        return string.IsNullOrWhiteSpace(fileName) ? filePath.Trim() : fileName;
    }

    private static string NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Unspecified";
        }

        return description.Trim();
    }
}
