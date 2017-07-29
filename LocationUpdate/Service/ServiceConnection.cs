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

namespace LocationUpdate
{
    public class ServiceConnection : Java.Lang.Object, IServiceConnection
    {
        #region Constructor

        public ServiceConnection(LocationBinder binder)
        {
            if (binder != null)
            {
                this._binder = binder;
            }
        }

        #endregion

        #region Properties

        public LocationBinder Binder
        {
            get { return this._binder; }
            set { this._binder = value; }
        }
        protected LocationBinder _binder;

        #endregion

        #region Event Arg

        public event EventHandler<ServiceConnectedEventArgs> ServiceConnected = delegate { };
        public event EventHandler<ServiceConnectedEventArgs> ServiceDisconnected = delegate { };

        #endregion

        #region IServiceConnection Implementations

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            LocationBinder serviceBinder = service as LocationBinder;
            if (serviceBinder != null)
            {
                this._binder = serviceBinder;
                this._binder.IsBound = true;

                // Notify, service connected event
                this.ServiceConnected(this, new ServiceConnectedEventArgs() { Binder = service });

                //Service is bound, we can start gathering location.
                serviceBinder.Service.BroadCastLocation();
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            //Service Disconnected
            this._binder.IsBound = false;
            this.ServiceDisconnected(this, null);
        }

        #endregion
    }
}