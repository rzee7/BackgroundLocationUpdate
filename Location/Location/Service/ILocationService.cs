using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location
{
    public interface ILocationServiceNormal
    {
        void FetchLocation(Position currentPosition);
    }
    public interface ILocationServiceBinding
    {
        void FetchLocation(Position currentPosition);
    }
}
