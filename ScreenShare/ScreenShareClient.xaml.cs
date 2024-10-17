using ScreenShare.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenShare
{
    // Interaction logic for ScreenShareClient.xaml
    public partial class ScreenShareClient : Page
    {
        public ScreenShareClient()
        {
            InitializeComponent();
            ScreenshareClientViewModel viewModel = new();
            this.DataContext = viewModel;
        }

        // This function is triggered when the user clicks on the Stop Screen Share Button 
        // It sets the value of SharingScreen boolean to false as screen is not being shared 
      
        public void OnStopButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ScreenshareClientViewModel viewModel)
            {
                viewModel.SharingScreen = false;
            }

            Trace.WriteLine(Utils.GetDebugMessage("Stop Share Button Clicked", withTimeStamp: true));
        }

       
        // This function is triggered when the user clicks on the Start Screen Share Button 
        // It sets the value of SharingScreen boolean to true as screen is being shared 
        
        public void OnStartButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ScreenshareClientViewModel viewModel)
            {
                viewModel.SharingScreen = true;
            }

            Trace.WriteLine(Utils.GetDebugMessage("Start Share Button Clicked", withTimeStamp: true));
        }
    }
}
