using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Options;
using DesktopApplicationTemplate.UI;

namespace DesktopApplicationTemplate.UI.Services
{
    public static class ServicePersistence
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceViewModel> services, ILoggingService? logger = null)
        {
            var data = new List<ServiceInfo>();
            var index = 0;
            foreach (var s in services)
            {
                TcpServiceOptions? tcp = null;
                FtpServerOptions? ftp = null;
                HttpServiceOptions? http = null;
                if (s.ServiceType == "TCP" && s.TcpOptions != null)
                {
                    tcp = new TcpServiceOptions
                    {
                        Host = s.TcpOptions.Host,
                        Port = s.TcpOptions.Port,
                        UseUdp = s.TcpOptions.UseUdp,
                        Mode = s.TcpOptions.Mode
                    };
                }

                if ((s.ServiceType == "FTP Server" || s.ServiceType == "FTP") && s.FtpOptions != null)
                {
                    ftp = new FtpServerOptions
                    {
                        Port = s.FtpOptions.Port,
                        RootPath = s.FtpOptions.RootPath,
                        AllowAnonymous = s.FtpOptions.AllowAnonymous,
                        Username = s.FtpOptions.Username,
                        Password = s.FtpOptions.Password
                    };
                }

                if (s.ServiceType == "HTTP" && s.HttpOptions != null)
                {
                    http = new HttpServiceOptions
                    {
                        BaseUrl = s.HttpOptions.BaseUrl,
                        Username = s.HttpOptions.Username,
                        Password = s.HttpOptions.Password,
                        ClientCertificatePath = s.HttpOptions.ClientCertificatePath
                    };
                }

                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    ServiceType = s.ServiceType,
                    IsActive = s.IsActive,
                    Created = DateTime.Now,
                    Order = index++,
                    AssociatedServices = new List<string>(s.AssociatedServices),
                    TcpOptions = tcp,
                    FtpOptions = ftp,
                    HttpOptions = http
                });
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var json = JsonSerializer.Serialize(data, options);
                logger?.Log($"Persisting services to {FilePath}", LogLevel.Debug);
                using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var sw = new StreamWriter(fs);
                sw.Write(json);
                logger?.Log($"Saved {data.Count} services to {FilePath}", LogLevel.Debug);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(data, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "services_dump.json");
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving services. Dump written to {temp}");
            }
        }

        public static List<ServiceInfo> Load(ILoggingService? logger = null)
        {
            if (!File.Exists(FilePath))
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }
            var json = File.ReadAllText(FilePath);
            try
            {
                var result = JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();

                foreach (var info in result)
                {
                    if (info.ServiceType == "TCP" && info.TcpOptions != null)
                    {
                        try
                        {
                            var opt = App.AppHost?.Services.GetService(typeof(IOptions<TcpServiceOptions>)) as IOptions<TcpServiceOptions>;
                            if (opt != null)
                            {
                                var value = opt.Value;
                                value.Host = info.TcpOptions.Host;
                                value.Port = info.TcpOptions.Port;
                                value.UseUdp = info.TcpOptions.UseUdp;
                                value.Mode = info.TcpOptions.Mode;
                            }
                        }
                        catch
                        {
                            // ignore missing options during tests or early startup
                        }
                    }
                    if ((info.ServiceType == "FTP Server" || info.ServiceType == "FTP") && info.FtpOptions != null)
                    {
                        try
                        {
                            var opt = App.AppHost?.Services.GetService(typeof(IOptions<FtpServerOptions>)) as IOptions<FtpServerOptions>;
                            if (opt != null)
                            {
                                var value = opt.Value;
                                value.Port = info.FtpOptions.Port;
                                value.RootPath = info.FtpOptions.RootPath;
                                value.AllowAnonymous = info.FtpOptions.AllowAnonymous;
                                value.Username = info.FtpOptions.Username;
                                value.Password = info.FtpOptions.Password;
                            }
                        }
                        catch
                        {
                            // ignore missing options during tests or early startup
                        }
                    }
                }

                logger?.Log($"Loaded {result.Count} services", LogLevel.Debug);
                return result;
            }
            catch
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceInfo>();
            }
        }
    }

    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string> AssociatedServices { get; set; } = new();
        public TcpServiceOptions? TcpOptions { get; set; }
        public FtpServerOptions? FtpOptions { get; set; }
        public HttpServiceOptions? HttpOptions { get; set; }
    }
}
