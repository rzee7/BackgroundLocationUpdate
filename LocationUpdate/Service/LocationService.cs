using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace LocationUpdate
{
    [Service]
    public class LocationService : Service
    {
        #region Fields

        IBinder binder;

        #endregion

        #region Events

        public event EventHandler<PositionEventArgs> LocationChanged = delegate { };

        #endregion

        #region Binder Implemented

        public override IBinder OnBind(Intent intent)
        {
            binder = new LocationBinder(this);
            return binder;
        }

        #endregion

        #region Overridable Methods

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        #endregion

        #region Service Lifecycle

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;
        }
        #endregion

        #region Methods

        public async void BroadCastLocation()
        {
            if (!CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, new ListenerSettings
                {
                    ActivityType = ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = true,
                    DeferralDistanceMeters = 1,
                    DeferralTime = TimeSpan.FromSeconds(1),
                    ListenForSignificantChanges = true,
                    PauseLocationUpdatesAutomatically = false
                });

                //Execute when there is a location change.
                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            }
        }

        private void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            this.LocationChanged(this, e);   
        }

        #endregion

    }
}