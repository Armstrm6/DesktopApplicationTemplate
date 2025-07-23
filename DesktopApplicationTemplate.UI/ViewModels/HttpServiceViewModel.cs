using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class HttpServiceViewModel : INotifyPropertyChanged
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
            set { _url = value; OnPropertyChanged(); }
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

        private void Save()
        {
            MessageBox.Show("Configuration saved.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task SendRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                ResponseBody = "URL is required";
                return;
            }

            using HttpClient client = new();
            try
            {
                using var request = new HttpRequestMessage(new HttpMethod(SelectedMethod), Url);

                foreach (var h in Headers)
                {
                    if (!string.IsNullOrWhiteSpace(h.Key))
                        request.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                if (SelectedMethod != "GET" && SelectedMethod != "DELETE")
                {
                    request.Content = new StringContent(RequestBody ?? string.Empty, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.SendAsync(request);
                StatusCode = (int)response.StatusCode;
                ResponseBody = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                ResponseBody = $"Error: {ex.Message}";
            }
            catch (System.Exception ex)
            {
                ResponseBody = $"Unexpected error: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
