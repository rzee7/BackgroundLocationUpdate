using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using LocationUpdate;
using System.Threading.Tasks;
using Android.Content;
using Plugin.Geolocator.Abstractions;

namespace Location.Droid
{
    [Activity(Label = "Location", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        #region Fields

        protected static ServiceConnection _serviceConnection;

        //Controls
        TextView _lblServiceStatus, _lblServiceConnectedTime, _lblBroadcastTimeInterval;
        TextView _lblLatitude, _lblLongitude, _lblSpeedAccuracy;
        public const int Location_BroadCastTime = 20; //Seconds; If you wants Minutes then, lets say 1 Min: 60 x 60 = 3600; 

        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            #region Service Connections

            _serviceConnection = _serviceConnection ?? new ServiceConnection(null);
            _serviceConnection.ServiceConnected += _serviceConnection_ServiceConnected;
            _serviceConnection.ServiceDisconnected += _serviceConnection_ServiceDisconnected;
            StartLocationService();

            #endregion

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

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
    }
}

