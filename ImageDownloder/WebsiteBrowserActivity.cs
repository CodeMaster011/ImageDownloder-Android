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
using Java.Lang;
using static ImageDownloder.MyGlobal;
using Squareup.Picasso;

namespace ImageDownloder
{
    [Activity(Label = "WebsiteBrowserActivity")]
    public class WebsiteBrowserActivity : Activity, IUiResponseHandler
    {
        private ListView contentListView = null;
        private GridView contentGridView = null;
        private BrowserListAdapter adapter = null;

        public void RequestProcessedCallback(string uid, string requestedUrl, WebPageData[] data)
        {            
            RunOnUiThread(new Action(() => {
                adapter.data = data;
                adapter.NotifyDataSetChanged();

                switch (currentWebPage.Viewing)
                {
                    case PreferedViewing.List:
                        if (contentListView.Visibility != ViewStates.Visible)
                        {
                            contentListView.Visibility = ViewStates.Visible;
                            contentGridView.Visibility = ViewStates.Gone;
                            contentListView.Adapter = adapter;
                            contentGridView.Adapter = null;

                            adapter.NotifyDataSetChanged();
                        }
                        break;
                    case PreferedViewing.Grid:
                        if (contentGridView.Visibility != ViewStates.Visible)
                        {
                            contentGridView.Visibility = ViewStates.Visible;
                            contentListView.Visibility = ViewStates.Gone;
                            contentGridView.Adapter = adapter;
                            contentListView.Adapter = null;

                            adapter.NotifyDataSetChanged();
                        }
                        break;
                    default:
                        break;
                }

                if (currenItemPosition != -1 && data.Length >= currenItemPosition)
                {
                    switch (currentWebPage.Viewing)
                    {
                        case PreferedViewing.List:
                            contentListView.SetSelection(currenItemPosition);
                            break;
                        case PreferedViewing.Grid:
                            contentGridView.SetSelection(currenItemPosition);
                            break;
                        default:
                            break;
                    }
                     //contentView.SmoothScrollToPosition(currenItemPosition);

                    currenItemPosition = -1;
                }
                //contentView.Invalidate();
            }));
        }

        public void RequestProcessingError(string uid, string requestedUrl, string error)
        {
            throw new NotImplementedException();
        }

        public override void OnBackPressed()
        {
            var hPage = BackToPreviousWebpage();
            if (hPage == null) return;

            analysisModule.CancleRequest(adapter.data); //cancel all previous pending requests

            analysisModule.RequestStringData(UidGenerator(), hPage, this);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_browser);

            webBrowserContext = this;            

            contentListView = FindViewById<ListView>(Resource.Id.contentListView);
            contentGridView = FindViewById<GridView>(Resource.Id.contentGridView);


            adapter = new BrowserListAdapter(null, this) { liv = contentListView };

            

            switch (currentWebPage.Viewing)
            {
                case PreferedViewing.List:
                    if (contentListView.Visibility != ViewStates.Visible)
                    {
                        contentListView.Visibility = ViewStates.Visible;
                        contentGridView.Visibility = ViewStates.Gone;
                        contentListView.Adapter = adapter;
                        contentGridView.Adapter = null;
                    }
                    break;
                case PreferedViewing.Grid:
                    if (contentGridView.Visibility != ViewStates.Visible)
                    {
                        contentGridView.Visibility = ViewStates.Visible;
                        contentListView.Visibility = ViewStates.Gone;
                        contentGridView.Adapter = adapter;
                        contentListView.Adapter = null;
                    }
                    break;
                default:
                    break;
            }

            analysisModule.RequestStringData(UidGenerator(), currentWebPage, this);    //make the request to analysisModule for first time

            contentListView.ItemClick += ContentView_ItemClick; //on click
            contentGridView.ItemClick += ContentView_ItemClick; //on click

            NotifyDataUpdate = new Action<int>((int position) => {
                RunOnUiThread(new Action(() => { adapter.NotifyDataSetChanged(); }));                
            });
        }

        private void ContentView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var webpageData = adapter.data[e.Position];
            if (webpageData.IsFinal && webpageData.underlayingLinkReader!=null)
            {
                analysisModule.RequestStringData(UidGenerator(), MoveToWebpage(webpageData.underlayingLinkReader, e.Position), this);
                currenItemPosition = 0;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        

        class BrowserListAdapter : BaseAdapter
        {
            public WebPageData[] data { get; set; } = null;

            public ListView liv { get; set; } = null;
            private Context context = null;

            public override int Count
            {
                get
                {
                    if (data == null) return 0;
                    return data.Length;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    var layoutInflator = (LayoutInflater)webBrowserContext.GetSystemService(Service.LayoutInflaterService);
                    switch (currentWebPage.Viewing)
                    {
                        case PreferedViewing.List:
                            convertView = layoutInflator.Inflate(Resource.Layout.website_browser_listview_single_row, parent, false);
                            break;
                        case PreferedViewing.Grid:
                            convertView = layoutInflator.Inflate(Resource.Layout.website_browser_gridview_single_item, parent, false);
                            break;
                        default:
                            break;
                    }
                    
                    VAdapterViewHolder vholder = new VAdapterViewHolder(convertView);
                    convertView.Tag = vholder;
                }
                VAdapterViewHolder vHolder = (VAdapterViewHolder)convertView.Tag;
                WebPageData data = this.data[position];
                vHolder.mainTextView.Text = data.mainText;
                vHolder.subTextView.Text = data.subText;

                //if (data.drawable != null) vHolder.imageView.SetImageBitmap(data.drawable);
                //else vHolder.imageView.SetImageResource(DefaultPic);
                //vHolder.imageView.SetImageBitmap(imageProvider.GetBitmapThumbnail(data.ImageUrl));//Grab image from ImageProvider using URL for better user experience

                if (data.ImageUrl!=string.Empty)
                    Picasso.With(context).Load(data.ImageUrl).Into(vHolder.imageView);
                else
                    vHolder.imageView.SetImageResource(DefaultPic);

                if (!data.IsFinal) vHolder.mainTextView.SetTextColor(Android.Graphics.Color.Red);
                else vHolder.mainTextView.SetTextColor(Android.Graphics.Color.White);

                //Android.Util.Log.Debug("UI", parent.Id == liv.Id ? "ListView" : "Grid Called ===================================");

                return convertView;
            }
            public BrowserListAdapter(WebPageData[] data, Context context)
            {
                this.data = data;
                this.context = context;
            }
        }

        class VAdapterViewHolder : Java.Lang.Object
        {
            public TextView mainTextView, subTextView;
            public ImageView imageView;

            public VAdapterViewHolder(View view)
            {
                switch (currentWebPage.Viewing)
                {
                    case PreferedViewing.List:
                        mainTextView = view.FindViewById<TextView>(Resource.Id.mainTextView);
                        subTextView = view.FindViewById<TextView>(Resource.Id.subTextView);
                        imageView = view.FindViewById<ImageView>(Resource.Id.imageView1);
                        break;
                    case PreferedViewing.Grid:
                        mainTextView = view.FindViewById<TextView>(Resource.Id.mainTextViewG);
                        subTextView = view.FindViewById<TextView>(Resource.Id.subTextViewG);
                        imageView = view.FindViewById<ImageView>(Resource.Id.imageViewG);
                        break;
                    default:
                        break;
                }
                
            }
        }
    }
}