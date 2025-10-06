using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.Persistence
{
    public static class ServicePersistence
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceListModel> services, ILoggingService? logger = null)
        {
            var data = new List<ServiceInfo>();
            var index = 0;
            foreach (var s in services)
            {
                TcpServiceOptions? tcp = null;
                CsvServiceOptions? csv = null;
                FtpServerOptions? ftp = null;
                HttpServiceOptions? http = null;
                HeartbeatServiceOptions? heartbeat = null;
                FileObserverServiceOptions? fileObserver = null;
                HidServiceOptions? hid = null;
                ScpServiceOptions? scp = null;
                if (s.Type == ServiceType.Tcp && s.TcpOptions != null)
                {
                    tcp = new TcpServiceOptions
                    {
                        Host = s.TcpOptions.Host,
                        Port = s.TcpOptions.Port,
                        UseUdp = s.TcpOptions.UseUdp,
                        Mode = s.TcpOptions.Mode,
                        ConnectionRole = s.TcpOptions.ConnectionRole,
                        InputMessage = s.TcpOptions.InputMessage,
                        Script = s.TcpOptions.Script,
                        OutputMessage = s.TcpOptions.OutputMessage,
                        LastTestMessage = s.TcpOptions.LastTestMessage
                    };
                }

                if (s.Type == ServiceType.Ftp && s.FtpOptions != null)
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

                if (s.Type == ServiceType.Http && s.HttpOptions != null)
                {
                    http = new HttpServiceOptions
                    {
                        BaseUrl = s.HttpOptions.BaseUrl,
                        Username = s.HttpOptions.Username,
                        Password = s.HttpOptions.Password,
                        ClientCertificatePath = s.HttpOptions.ClientCertificatePath
                    };
                }
                if (s.Type == ServiceType.Csv && s.CsvOptions != null)
                {
                    csv = new CsvServiceOptions
                    {
                        OutputPath = s.CsvOptions?.OutputPath ?? string.Empty,
                        Delimiter = s.CsvOptions?.Delimiter ?? ",",
                        IncludeHeaders = s.CsvOptions?.IncludeHeaders ?? true
                    };
                }

                if (s.Type == ServiceType.Heartbeat && s.HeartbeatOptions != null)
                {
                    heartbeat = new HeartbeatServiceOptions
                    {
                        BaseMessage = s.HeartbeatOptions.BaseMessage,
                        IncludePing = s.HeartbeatOptions.IncludePing,
                        IncludeStatus = s.HeartbeatOptions.IncludeStatus
                    };
                }

                if (s.Type == ServiceType.FileObserver && s.FileObserverOptions != null)
                {
                    fileObserver = new FileObserverServiceOptions
                    {
                        FilePath = s.FileObserverOptions.FilePath,
                        ImageNames = s.FileObserverOptions.ImageNames,
                        SendAllImages = s.FileObserverOptions.SendAllImages,
                        SendFirstX = s.FileObserverOptions.SendFirstX,
                        XCount = s.FileObserverOptions.XCount,
                        SendTcpCommand = s.FileObserverOptions.SendTcpCommand,
                        TcpCommand = s.FileObserverOptions.TcpCommand
                    };
                }

                if (s.Type == ServiceType.Hid && s.HidOptions != null)
                {
                    hid = new HidServiceOptions
                    {
                        MessageTemplate = s.HidOptions.MessageTemplate,
                        UsbProtocol = s.HidOptions.UsbProtocol,
                        AttachedService = s.HidOptions.AttachedService,
                        DebounceTimeMs = s.HidOptions.DebounceTimeMs,
                        KeyDownTimeMs = s.HidOptions.KeyDownTimeMs
                    };
                }

                if (s.Type == ServiceType.Scp && s.ScpOptions != null)
                {
                    scp = new ScpServiceOptions
                    {
                        Host = s.ScpOptions.Host,
                        Port = s.ScpOptions.Port,
                        Username = s.ScpOptions.Username,
                        Password = s.ScpOptions.Password,
                        LocalPath = s.ScpOptions.LocalPath,
                        RemotePath = s.ScpOptions.RemotePath
                    };
                }

                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    ServiceType = s.Type,
                    IsActive = s.IsActive,
                    Created = DateTime.Now,
                    Order = index++,
                    AssociatedServices = new List<string>(s.AssociatedServices),
                    TcpOptions = tcp,
                    FtpOptions = ftp,
                    HttpOptions = http,
                    CsvOptions = csv,
                    HeartbeatOptions = heartbeat,
                    FileObserverOptions = fileObserver,
                    HidOptions = hid,
                    ScpOptions = scp,
                    TotalExecutionTimeMs = s.TotalExecutionTimeMs,
                    ExecutionCount = s.ExecutionCount,
                    Logs = s.Logs
                        .Take(ServiceListModel.MaxLogEntries)
                        .Select(l => new LogEntry
                        {
                            Level = l.Level,
                            Message = l.Message,
                            Color = l.Color
                        })
                        .ToList()
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
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
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
            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (FileNotFoundException)
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }
            try
            {
                try
                {
                    var current = JsonSerializer.Deserialize<List<ServiceInfo>>(json);
                    if (current is { Count: > 0 })
                    {
                        logger?.Log($"Loaded {current.Count} services", LogLevel.Debug);
                        return current;
                    }
                }
                catch (JsonException)
                {
                    // fall back to legacy parsing below
                }

                var legacy = JsonSerializer.Deserialize<List<LegacyServiceInfo>>(json) ?? new List<LegacyServiceInfo>();
                var result = new List<ServiceInfo>();
                foreach (var info in legacy)
                {
                    if (ServiceTypeExtensions.TryParse(info.ServiceType, out var type))
                    {
                        result.Add(new ServiceInfo
                        {
                            DisplayName = info.DisplayName,
                            ServiceType = type,
                            IsActive = info.IsActive,
                            Created = info.Created,
                            Order = info.Order,
                            AssociatedServices = info.AssociatedServices ?? new List<string>(),
                            TcpOptions = info.TcpOptions,
                            FtpOptions = info.FtpOptions,
                            HttpOptions = info.HttpOptions,
                            CsvOptions = info.CsvOptions,
                            HeartbeatOptions = info.HeartbeatOptions,
                            FileObserverOptions = info.FileObserverOptions,
                            HidOptions = info.HidOptions,
                            ScpOptions = info.ScpOptions,
                            TotalExecutionTimeMs = info.TotalExecutionTimeMs,
                            ExecutionCount = info.ExecutionCount,
                            Logs = info.Logs ?? new List<LogEntry>()
                        });
                    }
                    else
                    {
                        logger?.Log($"Unmapped service type '{info.ServiceType}' for '{info.DisplayName}'", LogLevel.Warning);
                    }
                }

                foreach (var info in result)
                {
                    if (info.ServiceType == ServiceType.Tcp && info.TcpOptions != null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<TcpServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            value.Host = info.TcpOptions.Host;
                            value.Port = info.TcpOptions.Port;
                            value.UseUdp = info.TcpOptions.UseUdp;
                            value.Mode = info.TcpOptions.Mode;
                            value.ConnectionRole = info.TcpOptions.ConnectionRole;
                            value.InputMessage = info.TcpOptions.InputMessage;
                            value.Script = info.TcpOptions.Script;
                            value.OutputMessage = info.TcpOptions.OutputMessage;
                            value.LastTestMessage = info.TcpOptions.LastTestMessage;
                        }
                    }
                    if (info.ServiceType == ServiceType.Ftp && info.FtpOptions != null)
                    {
                        try
                        {
                            var opt = App.AppHost?.Services.GetService<IOptions<FtpServerOptions>>();
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
                    if (info.ServiceType == ServiceType.Heartbeat && info.HeartbeatOptions != null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<HeartbeatServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            value.BaseMessage = info.HeartbeatOptions.BaseMessage;
                            value.IncludePing = info.HeartbeatOptions.IncludePing;
                            value.IncludeStatus = info.HeartbeatOptions.IncludeStatus;
                        }
                    }

                    if (info.ServiceType == ServiceType.FileObserver && info.FileObserverOptions != null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<FileObserverServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            value.FilePath = info.FileObserverOptions.FilePath;
                            value.ImageNames = info.FileObserverOptions.ImageNames;
                            value.SendAllImages = info.FileObserverOptions.SendAllImages;
                            value.SendFirstX = info.FileObserverOptions.SendFirstX;
                            value.XCount = info.FileObserverOptions.XCount;
                            value.SendTcpCommand = info.FileObserverOptions.SendTcpCommand;
                            value.TcpCommand = info.FileObserverOptions.TcpCommand;
                        }
                    }

                    if (info.ServiceType == ServiceType.Hid && info.HidOptions != null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<HidServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            value.MessageTemplate = info.HidOptions.MessageTemplate;
                            value.UsbProtocol = info.HidOptions.UsbProtocol;
                            value.AttachedService = info.HidOptions.AttachedService;
                            value.DebounceTimeMs = info.HidOptions.DebounceTimeMs;
                            value.KeyDownTimeMs = info.HidOptions.KeyDownTimeMs;
                        }
                    }

                    if (info.ServiceType == ServiceType.Scp && info.ScpOptions != null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<ScpServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            value.Host = info.ScpOptions.Host;
                            value.Port = info.ScpOptions.Port;
                            value.Username = info.ScpOptions.Username;
                            value.Password = info.ScpOptions.Password;
                            value.LocalPath = info.ScpOptions.LocalPath;
                            value.RemotePath = info.ScpOptions.RemotePath;
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
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string> AssociatedServices { get; set; } = new();
        public TcpServiceOptions? TcpOptions { get; set; }
        public FtpServerOptions? FtpOptions { get; set; }
        public HttpServiceOptions? HttpOptions { get; set; }
        public CsvServiceOptions? CsvOptions { get; set; }
        public HeartbeatServiceOptions? HeartbeatOptions { get; set; }
        public FileObserverServiceOptions? FileObserverOptions { get; set; }
        public HidServiceOptions? HidOptions { get; set; }
        public ScpServiceOptions? ScpOptions { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
    }

    internal class LegacyServiceInfo
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
        public CsvServiceOptions? CsvOptions { get; set; }
        public HeartbeatServiceOptions? HeartbeatOptions { get; set; }
        public FileObserverServiceOptions? FileObserverOptions { get; set; }
        public HidServiceOptions? HidOptions { get; set; }
        public ScpServiceOptions? ScpOptions { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
    }
}
