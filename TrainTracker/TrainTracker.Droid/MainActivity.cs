using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Java.Net;

namespace TrainTracker.Droid
{
    [Activity(Label = "Train Tracker", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener
    {
        private GoogleMap map;
        private Marker marker;
        private GroundOverlay groundOverlay;
        private CameraPosition lastCameraPosition;
        private Polyline polyline;
        private LocationManager locationManager;
        private string locationProvider;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            if (map == null)
            {
                var fragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.googlemap);
                fragment.GetMapAsync(this);
            }
            locationManager = (LocationManager) GetSystemService(LocationService);
            var providers = locationManager.GetProviders(new Criteria { Accuracy = Accuracy.Fine }, true);
            if (providers.Any())
            {
                locationProvider = providers.First();
            }
            


        }

        public void OnMapReady(GoogleMap googleMap)
        {
            map = googleMap;
            map.UiSettings.RotateGesturesEnabled = false;
            
            var groundOverlayOpt = new GroundOverlayOptions();

            var points = Decode(Resources.GetString(Resource.String.frankstonLine)).ToArray();
            var random = new Random();
            var pointIndex = random.Next(0, points.Length - 2);
            var point1 = points[pointIndex];
            var point2 = points[pointIndex + 1];
            var angle = Math.Tan((point2.Longitude - point1.Longitude)/(point2.Latitude - point1.Latitude))*180/Math.PI;
            groundOverlayOpt.Position(point1, 20, 20);
            groundOverlayOpt.Anchor(0.5f, 0.5f);
            groundOverlayOpt.InvokeImage(BitmapDescriptorFactory.FromResource(Resource.Drawable.Train));
            groundOverlayOpt.InvokeBearing((float) angle);
            groundOverlay = map.AddGroundOverlay(groundOverlayOpt);

            var markerOpt = new MarkerOptions();
            markerOpt.SetPosition(point1);
            marker = map.AddMarker(markerOpt);

            polyline = map.AddPolyline(new PolylineOptions().Add(points));
            map.CameraChange += Map_CameraChange;
        }

        private void Map_CameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            if(lastCameraPosition != null && Math.Abs(lastCameraPosition.Zoom - e.Position.Zoom) > float.Epsilon)
            {
                //zoom changed
                if (e.Position.Zoom > 16)
                {
                    marker.Visible = false;
                    polyline.Visible = false;
                }
                else
                {
                    marker.Visible = true;
                    polyline.Visible = true;
                }
                
            }
            lastCameraPosition = e.Position;
        }

        /// <summary>
        /// Decode google style polyline coordinates.
        /// </summary>
        /// <param name="encodedPoints"></param>
        /// <returns></returns>
        public static IEnumerable<LatLng> Decode(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
                throw new ArgumentNullException("encodedPoints");

            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new LatLng(Convert.ToDouble(currentLat)/1E5, Convert.ToDouble(currentLng)/1E5);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        /// <summary>
        /// Encode it
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static string Encode(IEnumerable<LatLng> points)
        {
            var str = new StringBuilder();

            var encodeDiff = (Action<int>)(diff =>
            {
                int shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;

                int rem = shifted;

                while (rem >= 0x20)
                {
                    str.Append((char)((0x20 | (rem & 0x1f)) + 63));

                    rem >>= 5;
                }

                str.Append((char)(rem + 63));
            });

            int lastLat = 0;
            int lastLng = 0;

            foreach (var point in points)
            {
                int lat = (int)Math.Round(point.Latitude * 1E5);
                int lng = (int)Math.Round(point.Longitude * 1E5);

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }

            return str.ToString();
        }

        public void OnLocationChanged(Location location)
        {
            marker.Position = new LatLng(location.Latitude, location.Longitude);
        }

        public void OnProviderDisabled(string provider)
        {
            
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            
        }
    }

}


