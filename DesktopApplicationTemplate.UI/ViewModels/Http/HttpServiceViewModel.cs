using System.Collections.ObjectModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.IO;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;

namespace DesktopApplicationTemplate.UI.ViewModels.Http
{
public class HttpServiceViewModel : ValidatableViewModelBase, ILoggingViewModel
    {
        public ObservableCollection<string> Methods { get; } = new() { "GET", "POST", "PUT", "DELETE" };

        public class HeaderItem
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public ObservableCollection<HeaderItem> Headers { get; } = new();

        private HeaderItem _selectedHeader = new HeaderItem();
        public HeaderItem SelectedHeader
        {
            get => _selectedHeader;
            set { _selectedHeader = value; OnPropertyChanged(); }
        }

        public ICommand AddHeaderCommand { get; }
        public ICommand RemoveHeaderCommand { get; }

        private string _selectedMethod = "GET";
        public string SelectedMethod
        {
            get => _selectedMethod;
            set { _selectedMethod = value; OnPropertyChanged(); }
        }

        private string _url = string.Empty;
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        ClearErrors(nameof(Url));
                        Logger?.Log("Valid HTTP URL set", LogLevel.Debug);
                    }
                    else
                    {
                        AddError(nameof(Url), "Invalid URL");
                        Logger?.Log("Invalid HTTP URL entered", LogLevel.Warning);
                    }
                }
                else
                {
                    ClearErrors(nameof(Url));
                }
                OnPropertyChanged();
            }
        }

        private string _requestBody = string.Empty;
        public string RequestBody
        {
            get => _requestBody;
            set { _requestBody = value; OnPropertyChanged(); }
        }

        private string _responseBody = string.Empty;
        public string ResponseBody
        {
            get => _responseBody;
            set { _responseBody = value; OnPropertyChanged(); }
        }

        private int _statusCode;
        public int StatusCode
        {
            get => _statusCode;
            set { _statusCode = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }
        public ICommand SaveCommand { get; }

        private ILoggingService? _logger;
        public ILoggingService? Logger
        {
            get => _logger;
            set
            {
                if (_logger == value) return;
                if (_logger is not null)
                    _logger.LogAdded -= OnLogAdded;
                _logger = value;
                if (_logger is not null)
                    _logger.LogAdded += OnLogAdded;
            }
        }

        public ObservableCollection<LogEntry> Logs { get; } = new();

        private LogLevel _logLevelFilter = LogLevel.Debug;
        public LogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set
            {
                if (_logLevelFilter == value) return;
                _logLevelFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogs));
            }
        }

        public IEnumerable<LogEntry> DisplayLogs => Logs.Where(l => l.Level >= LogLevelFilter);

        public ICommand RefreshLogCommand { get; }
        public ICommand ExportLogCommand { get; }
        public ICommand ClearLogCommand { get; }

        /// <summary>
        /// Optional handler used for testing to intercept HTTP requests.
        /// </summary>
        public HttpMessageHandler? MessageHandler { get; set; }

        private readonly SaveConfirmationHelper _saveHelper;
        private readonly IHttpClientService _httpClientService;
        private bool _isExportingLogs;
        private readonly AsyncRelayCommand _exportLogCommand;

        public HttpServiceViewModel(SaveConfirmationHelper saveHelper, IHttpClientService httpClientService)
        {
            _saveHelper = saveHelper;
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            SendCommand = new AsyncRelayCommand(SendRequestAsync);
            AddHeaderCommand = new RelayCommand(() => Headers.Add(new HeaderItem()));
            RemoveHeaderCommand = new RelayCommand(() =>
            {
                if (SelectedHeader != null)
                    Headers.Remove(SelectedHeader);
            });
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            RefreshLogCommand = new RelayCommand(() => OnPropertyChanged(nameof(DisplayLogs)));
            _exportLogCommand = new AsyncRelayCommand(ExportLogsAsync, () => !_isExportingLogs);
            ExportLogCommand = _exportLogCommand;
            ClearLogCommand = new RelayCommand(ClearLogs);
        }

        private Task SaveAsync() => _saveHelper.ShowAsync();

        public async Task SendRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                ResponseBody = "URL is required";
                Logger?.Log("SendRequestAsync called with empty URL", LogLevel.Warning);
                return;
            }

            Logger?.Log("Starting HTTP request", LogLevel.Debug);

            try
            {
                Logger?.Log($"Preparing {SelectedMethod} request to {Url}", LogLevel.Debug);
                var request = new HttpExecutionRequest
                {
                    RequestUri = new Uri(Url),
                    Method = new HttpMethod(SelectedMethod),
                    MessageHandler = MessageHandler
                };

                foreach (var h in Headers)
                {
                    if (!string.IsNullOrWhiteSpace(h.Key))
                    {
                        request.Headers[h.Key] = h.Value;
                    }
                }

                if (SelectedMethod != "GET" && SelectedMethod != "DELETE")
                {
                    request.Body = RequestBody ?? string.Empty;
                    Logger?.Log($"Request Body: {RequestBody}", LogLevel.Debug);
                }

                var headerSummary = string.Join(", ", Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)).Select(h => $"{h.Key}:{h.Value}"));
                if (!string.IsNullOrWhiteSpace(headerSummary))
                    Logger?.Log($"Headers: {headerSummary}", LogLevel.Debug);

                HttpExecutionResult result = await _httpClientService.ExecuteAsync(request).ConfigureAwait(false);
                StatusCode = (int)result.StatusCode;
                ResponseBody = result.Body;
                Logger?.Log($"Received response with status {StatusCode}", LogLevel.Debug);
                Logger?.Log($"Response Body: {ResponseBody}", LogLevel.Debug);
                Logger?.Log("HTTP request completed", LogLevel.Debug);
            }
            catch (HttpRequestException ex)
            {
                ResponseBody = $"Error: {ex.Message}";
                Logger?.Log($"HTTP error: {ex.Message}", LogLevel.Error);
            }
            catch (System.Exception ex)
            {
                ResponseBody = $"Unexpected error: {ex.Message}";
                Logger?.Log($"Critical error: {ex.Message}", LogLevel.Critical);
            }
            finally
            {
                Logger?.Log("SendRequestAsync finished", LogLevel.Debug);
            }
        }

        private void ClearLogs()
        {
            Logs.Clear();
            OnPropertyChanged(nameof(DisplayLogs));
            Logger?.Log("HTTP logs cleared", LogLevel.Debug);
        }

        private async Task ExportLogsAsync()
        {
            if (_isExportingLogs)
            {
                return;
            }

            _isExportingLogs = true;
            _exportLogCommand.RaiseCanExecuteChanged();

            var path = Path.Combine(Path.GetTempPath(), "http_logs.txt");

            try
            {
                var lines = DisplayLogs.Select(l => l.Message).ToList();
                await File.WriteAllLinesAsync(path, lines);
                Logger?.Log($"HTTP logs exported to {path}", LogLevel.Debug);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Logger?.Log($"Failed to export HTTP logs to {path}: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                _isExportingLogs = false;
                _exportLogCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnLogAdded(LogEntry entry) => Logs.Insert(0, entry);

        // OnPropertyChanged is inherited from ViewModelBase
    }
}
