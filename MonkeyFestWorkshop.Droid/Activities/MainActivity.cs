﻿using System.Collections.Generic;
using Android.App;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;
using MonkeyFestWorkshop.Droid.Adapters;
using MonkeyFestWorkshop.Domain.Models.Menu;
using MonkeyFestWorkshop.Domain.Models.Vehicle;
using MonkeyFestWorkshop.Domain.Enumerations;

namespace MonkeyFestWorkshop.Droid.Activities
{
    [Activity]
    public class MainActivity : AppCompatActivity, IValueEventListener
    {
        private RecyclerView recyclerView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            FirebaseApp.InitializeApp(Application.Context);

            LoadVehicles();
        }

        private void ConfigRecyclerView(List<SectionItem> menu)
        {
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView_vehicles);
            var adapter = new SectionMenuAdapter(menu);
            adapter.OnItemClick += Adapter_OnItemClick;
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.SetAdapter(adapter);
        }

        private void LoadVehicles()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            DatabaseReference reference = database.GetReference("vehicle");
            reference.AddValueEventListener(this);
        }

        private void Adapter_OnItemClick(object sender, BaseVehicle vehicle)
        {
            GoToDetailsActivity(vehicle);
        }

        private void GoToDetailsActivity(BaseVehicle vehicle)
        {
            var intent = new Intent(this, typeof(VehicleDetailActivity));
            var extras = new Bundle();
            extras.PutString("vehicle", JsonConvert.SerializeObject(vehicle));

            intent.PutExtras(extras);

            StartActivity(intent);
        }

        public void OnCancelled(DatabaseError error)
        {
            Log.Debug(ComponentName.PackageName, error.Code.ToString());
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            List<BaseVehicle> list = new List<BaseVehicle>();
            for (int i = 0; i < snapshot.ChildrenCount; i++)
            {
                DataSnapshot dataSnapshot = snapshot.Child(i.ToString());

                var car = new Car
                {
                    Id = dataSnapshot.Child("id").Value.ToString(),
                    Plate = dataSnapshot.Child("plate").Value.ToString(),
                    Model = dataSnapshot.Child("model").Value.ToString(),
                    Line = dataSnapshot.Child("line").Value.ToString(),
                    BrandName = dataSnapshot.Child("brand_name").Value.ToString(),
                    Price = dataSnapshot.Child("price").Value.ToString()
                };

                if (dataSnapshot.Child("featured").Value is Java.Lang.Boolean)
                {
                    var featured = dataSnapshot.Child("featured").Value as Java.Lang.Boolean;
                    car.Featured = featured.BooleanValue();
                }
                
                list.Add(car);
            }

            List<BaseVehicle> orderList = list.Where(x => !x.Featured).Select(x => x).OrderByDescending((x) =>  x.Price).ToList();
            List<BaseVehicle> featuredVehicles = list.Where(x => x.Featured).Select(x => x).ToList();

            List<SectionItem> sectionItems = new List<SectionItem>();

            var featuredItems = new SectionItem
            {
                Title = "Destacados",
                Vehicles = featuredVehicles,
                SectionType = SectionType.Featured
            };

            var classicItems = new SectionItem
            {
                Title = "Vehículos",
                Vehicles = orderList,
                SectionType = SectionType.Classic
            };

            sectionItems.Add(featuredItems);
            sectionItems.Add(classicItems);
             
            ConfigRecyclerView(sectionItems);
        }
    }
}
