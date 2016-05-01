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
        private const int WebsiteImageViewRequestCode = 1;

        public bool IsNextPageRequestSent = false;

        private void restoreCurrentPosition()
        {
            if (currenItemPosition != -1 && adapter?.data?.Length >= currenItemPosition)
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
        }

        private void restoreContainer()
        {
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
        }

        public void RequestProcessedCallback(string uid, string requestedUrl, WebPageData[] data)
        {
            WebPageData[] newData = null;
            if (IsNextPageRequestSent)
            {
                newData = new WebPageData[adapter.data.Length + data.Length];
                adapter.data.CopyTo(newData, 0);
                data.CopyTo(newData, adapter.data.Length);

                IsNextPageRequestSent = false;
            }

            RunOnUiThread(new Action(() => {

                adapter.data = newData != null ? newData : data;

                adapter.NotifyDataSetChanged();

                restoreContainer();
                restoreCurrentPosition();

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

            currentState = currentState - 1;

            IsNextPageRequestSent = false;

            adapter.data = cachedData;
            adapter.NotifyDataSetChanged();
            restoreContainer();
            restoreCurrentPosition();

            memoryCache.Clear();

            Android.Util.Log.Debug("WebsiteImageViewActivity",
                $"Request Packet ={MyGlobal.requestPacketCount}, History Obj ={MyGlobal.historyObjCount}");
        }
        protected override void OnResume()
        {
            base.OnResume();

            if (currenItemPosition != -1 && adapter.data.Length >= currenItemPosition)
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
        }
        Intent websiteImageViewer = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_browser);

            webBrowserContext = this;            

            contentListView = FindViewById<ListView>(Resource.Id.contentListView);
            contentGridView = FindViewById<GridView>(Resource.Id.contentGridView);


            adapter = new BrowserListAdapter(null, this) { liv = contentListView };


            websiteImageViewer = new Intent(this, typeof(WebsiteImageViewActivity));

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
        }

        private void ContentView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var webpageData = adapter.data[e.Position];
            
            IsNextPageRequestSent = false;
            var nextPageReader = currentWebPage.OnClickCallback(webpageData);

            currentState = currentState + 1;

            if (webpageData.IsFinal && nextPageReader != null)
            {
                analysisModule.RequestStringData(UidGenerator(), MoveToWebpage(nextPageReader, adapter.data, e.Position), this);

                adapter.data = new WebPageData[] { WebPageData.GetFakeData() };
                adapter.NotifyDataSetChanged();

                currenItemPosition = 0;
            }
            else if (currentWebPage.IsOnClickBigImage)
            {
                try
                {
                    currenItemPosition = e.Position;

                    cachedData = adapter.data;

                    //StartActivity(websiteImageViewer);
                    StartActivityForResult(websiteImageViewer, WebsiteImageViewRequestCode);
                }
                catch (System.Exception) { }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == WebsiteImageViewRequestCode && resultCode == Result.Ok)
            {
                var isDataChanged = data.GetBooleanExtra("IsDataChange", true);
                if (isDataChanged)
                {
                    adapter.data = cachedData;
                    adapter.NotifyDataSetChanged();
                }
            }
        }


        class BrowserListAdapter : BaseAdapter
        {
            public WebPageData[] data { get; set; } = null;

            public ListView liv { get; set; } = null;
            private WebsiteBrowserActivity parent = null;

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

                if (data.ImageUrl!=string.Empty)
                    Picasso.With(parent.Context).Load(data.ImageUrl).Resize(128,128).CenterInside().Into(vHolder.imageView);
                else
                    vHolder.imageView.SetImageResource(DefaultPic);

                if (!data.IsFinal) vHolder.mainTextView.SetTextColor(Android.Graphics.Color.Red);
                else vHolder.mainTextView.SetTextColor(Android.Graphics.Color.White);

                if (data.subText == string.Empty) vHolder.subTextView.Visibility = ViewStates.Gone;
                else vHolder.subTextView.Visibility = ViewStates.Visible;

                if (!this.parent.IsNextPageRequestSent && currentWebPage.IsMultiPaged)
                {
                    float indexReched = (float) position / this.data.Length;
                    if (indexReched >= NextPageLoadingIndex)
                    {
                        var nextPage = currentWebPage.GetNextPage();
                        if (nextPage != null)
                        {
                            analysisModule.RequestStringData(UidGenerator(), nextPage, this.parent);
                            this.parent.IsNextPageRequestSent = true;
                        }                        
                    }
                }


                //Android.Util.Log.Debug("UI", parent.Id == liv.Id ? "ListView" : "Grid Called ===================================");
                return convertView;
            }
            public BrowserListAdapter(WebPageData[] data, WebsiteBrowserActivity parent)
            {
                this.data = data;
                this.parent = parent;
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