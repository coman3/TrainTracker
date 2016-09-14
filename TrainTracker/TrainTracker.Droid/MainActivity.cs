using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Java.IO;
using Java.Net;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using TrainTracker.Models;
using LatLng = Android.Gms.Maps.Model.LatLng;

namespace TrainTracker.Droid
{
    [Activity(Label = "Train Tracker", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener
    {
        private GoogleMap _map;
        private LocationManager _locationManager;
        private Dictionary<string, TrainMarker> _trains;

        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;

        private string _locationProvider;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            Load();
        }

        private async void Load()
        {
            if (_map == null)
            {
                var fragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.googlemap);
                fragment.GetMapAsync(this);
            }
            await Task.Factory.StartNew(() =>
            {

                _hubConnection = new HubConnection("http://vdvstudio.noip.me/SignalR");
                _hubProxy = _hubConnection.CreateHubProxy("TrainHub");
                _hubProxy.On<List<TrainUpdate>>("UpdateTrains", OnTrainUpdatedHandle);
                _locationManager = (LocationManager) GetSystemService(LocationService);
                var providers = _locationManager.GetProviders(new Criteria {Accuracy = Accuracy.Fine}, true);
                if (providers.Any())
                {
                    _locationProvider = providers.First();
                }
            });
            if (_hubConnection != null)
            {
                await _hubConnection.Start();
                
            }
            var threadId = Thread.CurrentThread.ManagedThreadId;
        }


        private void OnTrainUpdatedHandle(List<TrainUpdate> trainUpdates)
        {
            RunOnUiThread(() =>
            {
                foreach (var trainUpdate in trainUpdates)
                {
                    if (_trains == null) return;

                    if (_trains.ContainsKey(trainUpdate.TripId))
                    {
                        _trains[trainUpdate.TripId].Update(trainUpdate);
                    }
                    else
                    {
                        _trains[trainUpdate.TripId] = new TrainMarker(_map, trainUpdate);
                    }
                }
            });
        }

        public void OnMapReady (GoogleMap googleMap)
        {
            
            _map = googleMap;
            _map.UiSettings.RotateGesturesEnabled = false;
            _trains = new Dictionary<string, TrainMarker>();
            _map.CameraChange += Map_CameraChange;

        }

        private void Map_CameraChange (object sender, GoogleMap.CameraChangeEventArgs e)
        {
            if (_hubConnection.State == ConnectionState.Connected)
            {
                _hubProxy.Invoke("SetCenter", new Models.LatLng( e.Position.Target.Latitude, e.Position.Target.Longitude));
            }
        }


        protected override void OnResume()
        {
            base.OnResume();
            _locationManager?.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        #region Location Events

        public void OnLocationChanged(Location location)
        {

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

        #endregion

    }

    public class TrainMarker
    {
        public Marker Marker { get; set; }
        public TrainUpdate TrainUpdate { get; set; }

        
        public TrainMarker(GoogleMap map, TrainUpdate update)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(update.Posistion.Latitude, update.Posistion.Longitude));
            Marker = map.AddMarker(marker);
            TrainUpdate = update;
        }

        public void Update(TrainUpdate update)
        {
            Marker.Position = new LatLng(update.Posistion.Latitude, update.Posistion.Longitude);
            TrainUpdate = update;
        }

    }
}



