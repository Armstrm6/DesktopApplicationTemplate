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
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Models;
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
                        SubnetMask = s.TcpOptions.SubnetMask,
                        PrimaryDns = s.TcpOptions.PrimaryDns,
                        AlternateDns = s.TcpOptions.AlternateDns,
                        Mode = s.TcpOptions.Mode,
                        ConnectionRole = s.TcpOptions.ConnectionRole,
                        DestinationHost = s.TcpOptions.DestinationHost,
                        DestinationPort = s.TcpOptions.DestinationPort,
                        DestinationGateway = s.TcpOptions.DestinationGateway,
                        DestinationSubnetMask = s.TcpOptions.DestinationSubnetMask,
                        DestinationPrimaryDns = s.TcpOptions.DestinationPrimaryDns,
                        DestinationAlternateDns = s.TcpOptions.DestinationAlternateDns,
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

                MqttServiceOptions? mqtt = null;
                if (s.Type == ServiceType.Mqtt && s.MqttOptions != null)
                {
                    mqtt = new MqttServiceOptions
                    {
                        Host = s.MqttOptions.Host,
                        Port = s.MqttOptions.Port,
                        ClientId = s.MqttOptions.ClientId,
                        Username = s.MqttOptions.Username,
                        Password = s.MqttOptions.Password,
                        ConnectionType = s.MqttOptions.ConnectionType,
                        WebSocketPath = s.MqttOptions.WebSocketPath,
                        ClientCertificate = s.MqttOptions.ClientCertificate?.ToArray(),
                        WillTopic = s.MqttOptions.WillTopic,
                        WillPayload = s.MqttOptions.WillPayload,
                        WillQualityOfService = s.MqttOptions.WillQualityOfService,
                        WillRetain = s.MqttOptions.WillRetain,
                        KeepAliveSeconds = s.MqttOptions.KeepAliveSeconds,
                        CleanSession = s.MqttOptions.CleanSession,
                        ReconnectDelay = s.MqttOptions.ReconnectDelay
                    };
                }

                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    DescriptorId = s.DescriptorId,
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
                    MqttOptions = mqtt,
                    TotalExecutionTimeMs = s.TotalExecutionTimeMs,
                    ExecutionCount = s.ExecutionCount,
                    IncomingMessageCount = s.IncomingMessageCount,
                    OutgoingMessageCount = s.OutgoingMessageCount,
                    Logs = s.Logs
                        .Take(ServiceListModel.MaxLogEntries)
                        .Select(l => new LogEntry
                        {
                            Level = l.Level,
                            Message = l.Message,
                            Color = l.Color,
                            ServiceType = l.ServiceType ?? s.Type,
                            ServiceName = string.IsNullOrWhiteSpace(l.ServiceName) ? s.DisplayName : l.ServiceName
                        })
                        .ToList(),
                    MessageHistory = s.GetMessageHistorySnapshot().ToList()
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
                var services = JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();

                foreach (var info in services)
                {
                    info.AssociatedServices ??= new List<string>();
                    info.Logs ??= new List<LogEntry>();
                    info.MessageHistory ??= new List<ServiceMessageHistoryEntry>();
                }

                foreach (var info in services)
                {
                    if (info.ServiceType == ServiceType.Mqtt && info.MqttOptions is null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<MqttServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            info.MqttOptions = new MqttServiceOptions
                            {
                                Host = value.Host,
                                Port = value.Port,
                                ClientId = value.ClientId,
                                Username = value.Username,
                                Password = value.Password,
                                ConnectionType = value.ConnectionType,
                                WebSocketPath = value.WebSocketPath,
                                ClientCertificate = value.ClientCertificate?.ToArray(),
                                WillTopic = value.WillTopic,
                                WillPayload = value.WillPayload,
                                WillQualityOfService = value.WillQualityOfService,
                                WillRetain = value.WillRetain,
                                KeepAliveSeconds = value.KeepAliveSeconds,
                                CleanSession = value.CleanSession,
                                ReconnectDelay = value.ReconnectDelay
                            };
                        }
                        else
                        {
                            info.MqttOptions = new MqttServiceOptions();
                        }
                    }

                    if (info.ServiceType == ServiceType.Tcp && info.TcpOptions is null)
                    {
                        info.TcpOptions = new TcpServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Ftp && info.FtpOptions is null)
                    {
                        info.FtpOptions = new FtpServerOptions();
                    }

                    if (info.ServiceType == ServiceType.Http && info.HttpOptions is null)
                    {
                        info.HttpOptions = new HttpServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Heartbeat && info.HeartbeatOptions is null)
                    {
                        info.HeartbeatOptions = new HeartbeatServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.FileObserver && info.FileObserverOptions is null)
                    {
                        info.FileObserverOptions = new FileObserverServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Hid && info.HidOptions is null)
                    {
                        info.HidOptions = new HidServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Scp && info.ScpOptions is null)
                    {
                        info.ScpOptions = new ScpServiceOptions();
                    }
                }

                logger?.Log($"Loaded {services.Count} services", LogLevel.Debug);
                return services;
            }
            catch (JsonException)
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceInfo>();
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
        public string? DescriptorId { get; set; }
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
        public MqttServiceOptions? MqttOptions { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public int IncomingMessageCount { get; set; }
        public int OutgoingMessageCount { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
        public List<ServiceMessageHistoryEntry> MessageHistory { get; set; } = new();
    }

}
