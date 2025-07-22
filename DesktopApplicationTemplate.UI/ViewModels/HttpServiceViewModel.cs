using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class HttpServiceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Methods { get; } = new() { "GET", "POST", "PUT", "DELETE" };

        private string _selectedMethod = "GET";
        public string SelectedMethod
        {
            get => _selectedMethod;
            set { _selectedMethod = value; OnPropertyChanged(); }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(); }
        }

        private string _requestBody;
        public string RequestBody
        {
            get => _requestBody;
            set { _requestBody = value; OnPropertyChanged(); }
        }

        private string _responseBody;
        public string ResponseBody
        {
            get => _responseBody;
            set { _responseBody = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public HttpServiceViewModel()
        {
            SendCommand = new RelayCommand(async () => await SendRequestAsync());
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
                HttpResponseMessage response = SelectedMethod switch
                {
                    "GET" => await client.GetAsync(Url),
                    "POST" => await client.PostAsync(Url, new StringContent(RequestBody, Encoding.UTF8, "application/json")),
                    "PUT" => await client.PutAsync(Url, new StringContent(RequestBody, Encoding.UTF8, "application/json")),
                    "DELETE" => await client.DeleteAsync(Url),
                    _ => null
                };

                if (response != null)
                    ResponseBody = await response.Content.ReadAsStringAsync();
                else
                    ResponseBody = "Invalid Method";
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
