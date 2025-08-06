using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using System.Threading.Tasks;
using UWP_Demo.Services;
using System.Text;
using System.Linq;

namespace UWP_Demo.Views
{
    /// <summary>
    /// Network API functionality has been disabled
    /// </summary>
    public sealed partial class NetworkPage : Page
    {
        public NetworkPage()
        {
            this.InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Network API functionality is disabled");
        }
    }
}