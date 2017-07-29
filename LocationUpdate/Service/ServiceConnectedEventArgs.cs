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
    public class ServiceConnectedEventArgs : EventArgs
    {
        #region Properties

        public IBinder Binder { get; set; }

        #endregion
    }
}