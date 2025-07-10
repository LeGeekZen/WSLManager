using System.Configuration;
using System.Data;
using System.Windows;
using System.Text;

namespace WSLManagerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Active les encodages legacy (CP850, etc.) pour .NET Core/5+
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
