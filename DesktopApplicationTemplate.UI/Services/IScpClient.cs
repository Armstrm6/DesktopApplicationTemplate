namespace DesktopApplicationTemplate.UI.Services
{
    public interface IScpClient
    {
        Renci.SshNet.ConnectionInfo ConnectionInfo { get; }
        void Connect();
        void Upload(System.IO.Stream source, string path);
        void Disconnect();
    }
}
