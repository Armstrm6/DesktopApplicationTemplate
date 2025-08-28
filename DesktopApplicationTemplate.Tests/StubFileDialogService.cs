using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.Tests;

internal class StubFileDialogService : IFileDialogService
{
    private readonly string? _filePath;
    private readonly string? _folderPath;

    public StubFileDialogService(string? filePath = "stub.txt", string? folderPath = "C:/temp")
    {
        _filePath = filePath;
        _folderPath = folderPath;
    }

    public string? OpenFile() => _filePath;
    public string? SelectFolder() => _folderPath;
}
