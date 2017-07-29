using Android.App;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Android.Content.PM;
using System.Threading.Tasks;
using Plugin.Geolocator;
using System;
using Android.Content;
using Plugin.Geolocator.Abstractions;

namespace LocationUpdate
{
    [Activity(Label = "LocationUpdate", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        #region Fields

        protected static ServiceConnection _serviceConnection;

        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _serviceConnection = _serviceConnection ?? new ServiceConnection(null);
            _serviceConnection.ServiceConnected += _serviceConnection_ServiceConnected;
            _serviceConnection.ServiceDisconnected += _serviceConnection_ServiceDisconnected; ;
            StartLocationService();
        }

        private void _serviceConnection_ServiceDisconnected(object sender, ServiceConnectedEventArgs e)
        {
            if (Location != null)
            {
                Location.LocationChanged -= Location_LocationChanged;
            }
        }

        private void _serviceConnection_ServiceConnected(object sender, ServiceConnectedEventArgs e)
        {
            //Service is connected and ready use.
            if(Location!=null)
            {
                Location.LocationChanged += Location_LocationChanged;
            }
        }

        private void Location_LocationChanged(object sender, PositionEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Location Service Stuff

        public void StartLocationService()
        {
            new Task(() =>
            {
                this.ApplicationContext.StartService(new Intent(this.ApplicationContext, typeof(LocationService)));

                // bind our service (Android goes and finds the running service by type, and puts a reference
                // on the binder to that service)
                // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                // we're looking for (LocationService)
                Intent locServiceIntent = new Intent(Android.App.Application.Context, typeof(LocationService));

                // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                // created in a previous step.
                ApplicationContext.BindService(locServiceIntent, _serviceConnection, Bind.AutoCreate);
            }).Start();
        }

        public void StopLocationService()
        {
            // Unbind from the LocationService; otherwise, StopSelf (below) will not work:
            if (_serviceConnection != null)
            {
                ApplicationContext.UnbindService(_serviceConnection);
            }

            // Stop the LocationService:
            if (Location != null)
            {
                Location.StopSelf();
            }
        }

        #endregion


        #region Location Stuff

        public LocationService Location
        {
            get
            {
                if (_serviceConnection.Binder == null)
                    throw new Exception("Service not bound yet");
                
                //Note that we using the ServiceConnection to get the Binder, and the Binder to get the Service here
                return _serviceConnection.Binder.Service;
            }
        }

        #endregion

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        async Task StartListening()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false
            });

            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
        }

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            this.RunOnUiThread(() =>
            {
                var test = e.Position;
                Console.WriteLine("Value: {0} {1}", test.Latitude, test.Longitude);
                //listenLabel.Text = "Full: Lat: " + test.Latitude.ToString() + " Long: " + test.Longitude.ToString();
                //listenLabel.Text += "\n" + $"Time: {test.Timestamp.ToString()}";
                //listenLabel.Text += "\n" + $"Heading: {test.Heading.ToString()}";
                //listenLabel.Text += "\n" + $"Speed: {test.Speed.ToString()}";
                //listenLabel.Text += "\n" + $"Accuracy: {test.Accuracy.ToString()}";
                //listenLabel.Text += "\n" + $"Altitude: {test.Altitude.ToString()}";
                //listenLabel.Text += "\n" + $"AltitudeAccuracy: {test.AltitudeAccuracy.ToString()}";
            });
        }
    }
}

