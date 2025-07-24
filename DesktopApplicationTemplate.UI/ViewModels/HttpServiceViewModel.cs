using System.Collections.ObjectModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
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

        public ILoggingService? Logger { get; set; }

        /// <summary>
        /// Optional handler used for testing to intercept HTTP requests.
        /// </summary>
        public HttpMessageHandler? MessageHandler { get; set; }

        public HttpServiceViewModel()
        {
            SendCommand = new RelayCommand(async () => await SendRequestAsync());
            AddHeaderCommand = new RelayCommand(() => Headers.Add(new HeaderItem()));
            RemoveHeaderCommand = new RelayCommand(() =>
            {
                if (SelectedHeader != null)
                    Headers.Remove(SelectedHeader);
            });
            SaveCommand = new RelayCommand(Save);
        }

        private void Save() => SaveConfirmationHelper.Show();

        public async Task SendRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                ResponseBody = "URL is required";
                Logger?.Log("SendRequestAsync called with empty URL", LogLevel.Warning);
                return;
            }

            using HttpClient client = MessageHandler != null ? new HttpClient(MessageHandler) : new HttpClient();
            try
            {
                Logger?.Log($"Preparing {SelectedMethod} request to {Url}", LogLevel.Debug);
                var request = new HttpRequestMessage(new HttpMethod(SelectedMethod), Url);

                foreach (var h in Headers)
                {
                    if (!string.IsNullOrWhiteSpace(h.Key))
                        request.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }
                var headerSummary = string.Join(", ", Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)).Select(h => $"{h.Key}:{h.Value}"));
                if (!string.IsNullOrWhiteSpace(headerSummary))
                    Logger?.Log($"Headers: {headerSummary}", LogLevel.Debug);

                if (SelectedMethod != "GET" && SelectedMethod != "DELETE")
                {
                    request.Content = new StringContent(RequestBody ?? string.Empty, Encoding.UTF8, "application/json");
                    Logger?.Log($"Request Body: {RequestBody}", LogLevel.Debug);
                }

                HttpResponseMessage response = await client.SendAsync(request);
                StatusCode = (int)response.StatusCode;
                ResponseBody = await response.Content.ReadAsStringAsync();
                Logger?.Log($"Received response with status {StatusCode}", LogLevel.Debug);
                Logger?.Log($"Response Body: {ResponseBody}", LogLevel.Debug);
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
        }

        // OnPropertyChanged is inherited from ViewModelBase
    }
}
