using Location.ViewModel;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(MainViewModel))]
namespace Location.ViewModel
{
    public class MainViewModel : BaseViewModel, ILocationServiceBinding
    {
        public MainViewModel()
        {
            CurrentLocation = new Position();
        }
        #region Properties

        private Position _location;

        public Position CurrentLocation
        {
            get { return _location; }
            set { _location = value; OnPropertyChanged(); }
        }

        #endregion

        #region ILocation

        public void FetchLocation(Position currentPosition)
        {
            CurrentLocation = currentPosition;
            System.Diagnostics.Debug.WriteLine("Latitude: {0} | Longitude: {1}", currentPosition.Latitude, currentPosition.Longitude);
        }

        #endregion
    }
}
