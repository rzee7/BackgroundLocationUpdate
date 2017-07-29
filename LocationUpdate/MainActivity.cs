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

        //Controls
        TextView _lblServiceStatus, _lblServiceConnectedTime, _lblBroadcastTimeInterval;
        TextView _lblLatitude, _lblLongitude, _lblSpeedAccuracy;
        public const int Location_BroadCastTime = 20; //Seconds; If you wants Minutes then, lets say 1 Min: 60 x 60 = 3600; 

        #endregion

        #region Activity LifeCycle

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            #region Controls Init

            //Service
            _lblServiceStatus = FindViewById<TextView>(Resource.Id.lblServiceStatus);
            _lblServiceConnectedTime = FindViewById<TextView>(Resource.Id.lblServiceConnectedTime);
            _lblBroadcastTimeInterval = FindViewById<TextView>(Resource.Id.lblServiceInterval);

            //Location
            _lblLatitude = FindViewById<TextView>(Resource.Id.lblLatitude);
            _lblLongitude = FindViewById<TextView>(Resource.Id.lblLongitude);
            _lblSpeedAccuracy = FindViewById<TextView>(Resource.Id.lblSpeedAccuracy);

            #endregion

            #region Service Connections

            _serviceConnection = _serviceConnection ?? new ServiceConnection(null);
            _serviceConnection.ServiceConnected += _serviceConnection_ServiceConnected;
            _serviceConnection.ServiceDisconnected += _serviceConnection_ServiceDisconnected;
            StartLocationService();

            #endregion
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
           // StopLocationService();

            //_serviceConnection.ServiceConnected -= _serviceConnection_ServiceConnected;
            //_serviceConnection.ServiceDisconnected -= _serviceConnection_ServiceDisconnected;
        }

        #endregion

        #region Location Service Stuff
         
        private void _serviceConnection_ServiceDisconnected(object sender, ServiceConnectedEventArgs e)
        {
            if (Location != null)
            {
                Location.LocationChanged -= Location_LocationChanged;
            }
            Console.WriteLine(nameof(_serviceConnection_ServiceConnected) + " MainActivity ServiceConnection Service Disconnected");
        }

        private void _serviceConnection_ServiceConnected(object sender, ServiceConnectedEventArgs e)
        {
            Console.WriteLine(nameof(_serviceConnection_ServiceConnected) + " MainActivity ServiceConnection Service Connected");
            _lblServiceStatus.Text = "Connected";
            _lblServiceConnectedTime.Text = string.Format("Connected @ {0}", DateTime.Now.ToString(@"hh\:mm tt"));

            string bTime = "Broadcast Time: {0} {1}";
            if (Location_BroadCastTime > 59) //In case dynamic values 
            {
                bTime = string.Format(bTime, Location_BroadCastTime, "Min");
            }
            else
                bTime = string.Format(bTime, Location_BroadCastTime, "Sec");

            _lblBroadcastTimeInterval.Text = bTime;
            
            //Service is connected and ready use.
            if (Location != null)
            {
                Location.LocationChanged += Location_LocationChanged;
            }
        }

        private void Location_LocationChanged(object sender, PositionEventArgs e)
        {
            Console.WriteLine(nameof(Location_LocationChanged) + " Location " + e.Position.Latitude + " " + e.Position.Longitude);
            RunOnUiThread(() =>
            {
                _lblLatitude.Text = "Latitude: " + e.Position.Latitude.ToString();
                _lblLongitude.Text = "Longitude: " + e.Position.Longitude.ToString();
                _lblSpeedAccuracy.Text = string.Format("Speed {0} | Accuracy {1:00.00} ", e.Position.Speed, e.Position.Accuracy);
            });
        }

        public void StartLocationService()
        {
            new Task(() =>
            {
                StartService(new Intent(this, typeof(LocationService)));

                // bind our service (Android goes and finds the running service by type, and puts a reference
                // on the binder to that service)
                // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                // we're looking for (LocationService)
                Intent locServiceIntent = new Intent(this, typeof(LocationService));

                // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                // created in a previous step.
                BindService(locServiceIntent, _serviceConnection, Bind.AutoCreate);

                Console.WriteLine(nameof(StartLocationService) + " Started Location Service");

            }).Start();
        }

        public void StopLocationService()
        {
            // Unbind from the LocationService; otherwise, StopSelf (below) will not work:
            if (_serviceConnection != null)
            {
                UnbindService(_serviceConnection);
            }

            // Stop the LocationService:
            if (Location != null)
            {
                Location.StopSelf();
            }

            Console.WriteLine(nameof(StopLocationService) + " Stopped Location Service");
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

