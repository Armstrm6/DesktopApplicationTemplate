using System.Threading.Tasks;
using System.Windows.Controls;
namespace DesktopApplicationTemplate.UI.Navigation
{
    public interface INavigationHandler
    {
        string DescriptorId { get; }
        Page CreateView(string defaultName);
        Task AddServiceAsync(string name, object options);
    }
}
