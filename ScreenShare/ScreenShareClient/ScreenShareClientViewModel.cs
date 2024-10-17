 
// Contains view model for screenshare client.
 

using ScreenShare.Client;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ScreenShare.Client
{
     
    // ViewModel class for client
     
    public class ScreenshareClientViewModel :
    INotifyPropertyChanged
    {
        // Boolean to store whether the client is sharing screen or not.
        private bool _sharingScreen;

        // Underlying data model for ScreenshareClient.
        private readonly ScreenshareClient _model;

        // Property changed event raised when a property is changed on a component.
        public event PropertyChangedEventHandler? PropertyChanged;

        private DispatcherOperation? _sharingScreenOp;

         
        // Gets the dispatcher to the main thread. In case it is not available (such as during
        // unit testing) the dispatcher associated with the current thread is returned.
         
        private Dispatcher ApplicationMainThreadDispatcher =>
            (System.Windows.Application.Current?.Dispatcher != null) ?
                    System.Windows.Application.Current.Dispatcher :
                    Dispatcher.CurrentDispatcher;

         
        // Boolean to store whether the screen is currently being stored or not.
        // When the boolen is changed, we call OnPropertyChanged to refresh the view.
        // We also start/stop the screenshare accordingly when the property is changed.
         
        public bool SharingScreen
        {
            get => _sharingScreen;

            set
            {
                // Execute the call on the application's main thread.
                _sharingScreenOp = this.ApplicationMainThreadDispatcher.BeginInvoke(
                                    DispatcherPriority.Normal,
                                    new Action(() =>
                                    {
                                        lock (this)
                                        {
                                            this._sharingScreen = value;
                                            this.OnPropertyChanged("SharingScreen");
                                        }
                                    }));

                if (value)
                {
                    _model.StartScreensharing();
                }
                else
                {
                    _model.StopScreensharing();
                }
            }
        }


         
        // Constructor for the ScreenshareClientViewModel.
         
        public ScreenshareClientViewModel()
        {
            _model = ScreenshareClient.GetInstance(this);
            _sharingScreen = false;
        }

         
        // Handles the property changed event raised on a component.
         
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}