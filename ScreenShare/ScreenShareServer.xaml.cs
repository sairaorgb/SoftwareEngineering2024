using ScreenShare.Server;
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
    // Interaction logic for ScreenShareServer.xaml
    
    public partial class ScreenShareServer : Page
    {
        public ScreenShareServer()
        {
            InitializeComponent();
            ScreenshareServerViewModel viewModel = ScreenshareServerViewModel.GetInstance();
            this.DataContext = viewModel;

            Trace.WriteLine(Utils.GetDebugMessage("Created the ScreenshareServerView Component", withTimeStamp: true));

            Debug.WriteLine(viewModel.CurrentWindowClients.Count);
        }


        
        // This function increases the current page number by 1
        // If on the last page, next page button is not accessible and so is this function 
        
        private void OnNextPageButtonClicked(object sender, RoutedEventArgs e)
        {
            ScreenshareServerViewModel? viewModel = this.DataContext as ScreenshareServerViewModel;
            Debug.Assert(viewModel != null, Utils.GetDebugMessage("View Model could not be created"));
            viewModel.RecomputeCurrentWindowClients(viewModel.CurrentPage + 1);

            Trace.WriteLine(Utils.GetDebugMessage("Next Page Button Clicked", withTimeStamp: true));
        }

        
        // This function decreases the current page number by 1
        // If on the first page, previous button is not accessible and so is this function 
       
        private void OnPreviousPageButtonClicked(object sender, RoutedEventArgs e)
        {
            ScreenshareServerViewModel? viewModel = this.DataContext as ScreenshareServerViewModel;
            Debug.Assert(viewModel != null, Utils.GetDebugMessage("View Model could not be created"));
            viewModel.RecomputeCurrentWindowClients(viewModel.CurrentPage - 1);

            Trace.WriteLine(Utils.GetDebugMessage("Previous Page Button Clicked", withTimeStamp: true));
        }

      
        // This function calls the OnPin function of the viewModel which pins the tile on which the user has clicked 
        // The argument given to OnPin is the ClientID of user which has to be pinned, stored in Command Parameter 
        
        private void OnPinButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button pinButton)
            {
                ScreenshareServerViewModel? viewModel = this.DataContext as ScreenshareServerViewModel;

                Debug.Assert(pinButton != null, Utils.GetDebugMessage("Pin Button is not created properly"));
                Debug.Assert(pinButton.CommandParameter != null, "ClientId received to pin does not exist");
                Debug.Assert(viewModel != null, Utils.GetDebugMessage("View Model could not be created"));

                viewModel.OnPin(pinButton.CommandParameter.ToString()!);
            }

            Trace.WriteLine(Utils.GetDebugMessage("Pin Button Clicked", withTimeStamp: true));
        }

        
        // This function calls the OnUnpin function of the ViewModel which will unpin the tile the user clicked on
        // The argument given to Unpin function is the Client ID which has to be unpinned, stored in the Command Parameter 
        
        public void OnUnpinButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button someButton)
            {
                ScreenshareServerViewModel? viewModel = this.DataContext as ScreenshareServerViewModel;

                Debug.Assert(someButton != null, Utils.GetDebugMessage("Unpin Button is not created properly"));
                Debug.Assert(someButton.CommandParameter != null, "ClientId received to unpin does not exist");
                Debug.Assert(viewModel != null, Utils.GetDebugMessage("View Model could not be created"));

                viewModel.OnUnpin(someButton.CommandParameter.ToString()!);
            }

            Trace.WriteLine(Utils.GetDebugMessage("Unpin Button Clicked", withTimeStamp: true));
        }
    }
}


