using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading;
using Android.Content;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Java.Lang;
using System;

namespace ImageDownloder
{
	[Activity (Label = "Image Viewer", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
        private GridView gridView = null;
        private GridViewAdapter adapter = null;

        public List<IWebsiteReader> websiteReader = new List<IWebsiteReader>();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            Android.Views.Display display = WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);
            MyGlobal.screenSize = new System.Drawing.Size(size.X, size.Y);

            MyPicasso.CreateNewPicasso(ApplicationContext);


            websiteReader.Add(new Core.Architecture.WebsiteHandler(new Website.IndiancinemagalleryWebsiteArchitecture()));
            websiteReader.Add(new Website.IdlebrainWebsiteReader());
            websiteReader.Add(new Core.Architecture.WebsiteHandler(new Website.BharatStudentWebsiteArchitecture()));

            gridView = FindViewById<GridView>(Resource.Id.mainGridView);
            adapter = new GridViewAdapter() { parent = this };
            gridView.Adapter = adapter;
            gridView.ItemClick += GridView_ItemClick;
            //MyGlobal.MoveToWebpage(
            //        new Core.Architecture.WebsiteHandler(new Website.IndiancinemagalleryWebsiteArchitecture()).Start(),
            //        null, "Indiancinemagallery", 0);

            //MyGlobal.MoveToWebpage(new Website.IdlebrainWebsiteReader().IndexPageReader, null, 0);

            //var websiteBrowser = new Intent(this, typeof(WebsiteBrowserActivity));
            //StartActivity(websiteBrowser);

        }

        private void GridView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var website = websiteReader[e.Position];
            //MyGlobal.MoveToWebpage(website.IndexPageReader, null, website.Name, 0);

            MyGlobal.currentWebPage = website.IndexPageReader;
            MyGlobal.title = website.Name;

            var websiteBrowser = new Intent(this, typeof(WebsiteBrowserActivity));
            StartActivity(websiteBrowser);
        }

        class GridViewAdapter : BaseAdapter
        {
            public MainActivity parent { get; set; } = null;

            public override int Count
            {
                get
                {
                    return parent.websiteReader.Count;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    var layoutInflator = (LayoutInflater)parent.Context.GetSystemService(Service.LayoutInflaterService);
                    convertView = layoutInflator.Inflate(Resource.Layout.main_listview_single_item, parent, false);
                    convertView.Tag = new ViewHolder(convertView);
                }
                var website = this.parent.websiteReader[position];

                var vHolder = convertView.Tag as ViewHolder;

                vHolder.websiteNameTextView.Text = website.Name;
                vHolder.websiteComicLinearLayout.SetBackgroundColor(Color.ParseColor(MyGlobal.GetRandomComicColor()));
                vHolder.websiteComicTextView.Text = website.ComicText;

                return convertView;
            }

            class ViewHolder: Java.Lang.Object
            {
                public LinearLayout websiteComicLinearLayout = null;
                public TextView websiteNameTextView = null, websiteComicTextView = null;

                public ViewHolder(View v)
                {
                    websiteComicLinearLayout = v.FindViewById<LinearLayout>(Resource.Id.websiteComicLinearLayout);
                    websiteNameTextView = v.FindViewById<TextView>(Resource.Id.websiteNameTextView);
                    websiteComicTextView = v.FindViewById<TextView>(Resource.Id.websiteComicTextView);
                }
            }
        }
    }
}


