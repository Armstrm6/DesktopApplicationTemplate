using DesktopApplicationTemplate.UI.ViewModels;
using System.Text;

namespace DesktopApplicationTemplate.UI.Services
{
    public class CsvService
    {
        private readonly CsvViewerViewModel _viewModel;
        private int _index = 0;

        public CsvService(CsvViewerViewModel vm)
        {
            _viewModel = vm;
        }

        public void AppendRow(IEnumerable<string> values)
        {
            string fileName = BuildFileName();
            var line = string.Join(",", values);
            System.IO.File.AppendAllText(fileName, line + System.Environment.NewLine, Encoding.UTF8);
        }

        private string BuildFileName()
        {
            string pattern = _viewModel.Configuration.FileNamePattern;
            string name = pattern.Replace("{index}", _index.ToString());
            if (!name.Contains("{index}"))
                _index++;
            return name;
        }
    }
}
