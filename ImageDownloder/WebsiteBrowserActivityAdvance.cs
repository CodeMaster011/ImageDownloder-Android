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
using Android.Support.V7.Widget;
using static ImageDownloder.MyGlobal;
using Android.Graphics;
using Squareup.Picasso;

namespace ImageDownloder
{
    [Activity(Label = "Website Browser Activity Advance")]
    class WebsiteBrowserActivityAdvance : Activity, IUiResponseHandler
    {
        private RecyclerView recyView = null;
        private RecyAdapter recyAdapter = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_browser_advance);

            //==============================UI ITEM=====================================
            recyView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            //==========================================================================

            //==============================UI ATTACHMENTS==============================
            recyAdapter = new RecyAdapter() { context = this};
            recyAdapter.ItemClick += RecyAdapter_ItemClick;
            recyView.SetAdapter(recyAdapter);

            //recyLayoutManager = new GridLayoutManager(this, 2, (int)Orientation.Vertical, false);
            recyLayoutManager = new StaggeredGridLayoutManager(2, (int)Orientation.Vertical);
            recyView.SetLayoutManager(recyLayoutManager);

            recyView.AddItemDecoration(new RecyItemDecorator());
            //==========================================================================

            //==============================GLOBAL ITEM=================================
            webBrowserContext = this;

            NotifyDataUpdate = new Action<int>((int position) =>
            {
                RunOnUiThread(new Action(() => { recyAdapter.NotifyDataSetChanged(); }));
            });

            analysisModule.RequestStringData(UidGenerator(), currentWebPage, this);    //make the request to analysisModule for first time
            //==========================================================================

            //Title
            
        }

        public override void OnBackPressed()
        {
            var hPage = BackToPreviousWebpage();
            if (hPage == null) return;

            analysisModule.CancleRequest(recyAdapter.data); //cancel all previous pending requests

            analysisModule.RequestStringData(UidGenerator(), hPage, this);
        }

        private void RecyAdapter_ItemClick(object sender, int position)
        {
            var webpageData = recyAdapter.data[position];
            if (webpageData.IsFinal && webpageData.underlayingLinkReader != null)
            {
                analysisModule.RequestStringData(UidGenerator(), MoveToWebpage(webpageData.underlayingLinkReader, position), this);
                currenItemPosition = 0;
            }
        }

        public void RequestProcessedCallback(string uid, string requestedUrl, WebPageData[] data)
        {
            RunOnUiThread(new Action(() => {
                recyAdapter.data = data;
                recyAdapter.NotifyDataSetChanged();

                //switch (currentWebPage.Viewing)
                //{
                //    case PreferedViewing.List:
                        
                //        break;
                //    case PreferedViewing.Grid:
                        
                //        break;
                //    default:
                //        break;
                //}

                if (currenItemPosition != -1 && data.Length >= currenItemPosition)
                {
                    recyView.ScrollToPosition(currenItemPosition);
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

        class RecyAdapter : RecyclerView.Adapter
        {
            public event EventHandler<int> ItemClick = null;

            public WebPageData[] data { get; set; } = null;
            public Context context { get; set; }

            public override int ItemCount
            {
                get
                {
                    return data == null ? 0 : data.Length;
                }
            }
            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vHolder = holder as RecyViewHolder;
                WebPageData data = this.data[position];

                vHolder.mainTextView.Text = data.mainText;
                vHolder.subTextView.Text = data.subText;

                //if (data.drawable != null) vHolder.imageView.SetImageBitmap(data.drawable);
                //else vHolder.imageView.SetImageResource(DefaultPic);
                //vHolder.imageView.SetImageBitmap(imageProvider.GetBitmapThumbnail(data.ImageUrl));//Grab image from ImageProvider using URL for better user experience

                if (data.ImageUrl != string.Empty)
                    Picasso.With(context).Load(data.ImageUrl).Into(vHolder.imageView);
                else
                    vHolder.imageView.SetImageResource(DefaultPic);

                if (!data.IsFinal) vHolder.mainTextView.SetTextColor(Android.Graphics.Color.Red);
                else vHolder.mainTextView.SetTextColor(Android.Graphics.Color.Black);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.website_browser_gridview_single_item, parent, false);
                return new RecyViewHolder(itemView, itemClickedCallback);
            }

            private void itemClickedCallback(int position)
            {
                ItemClick?.Invoke(this, position);
            }
        }

        class RecyViewHolder : RecyclerView.ViewHolder
        {
            public TextView mainTextView, subTextView;
            public ImageView imageView;
            public RelativeLayout container;
            public RecyViewHolder(View view, Action<int> itemClick) : base(view)
            {
                view.Click += (sender, e) => itemClick(base.Position);  //add click handler

                container = view.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
                mainTextView = view.FindViewById<TextView>(Resource.Id.mainTextViewG);
                subTextView = view.FindViewById<TextView>(Resource.Id.subTextViewG);
                imageView = view.FindViewById<ImageView>(Resource.Id.imageViewG);
            }
        }

        class RecyItemDecorator : RecyclerView.ItemDecoration
        {

            public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
            {
                //base.GetItemOffsets(outRect, view, parent, state);
                outRect.Bottom = 0;
                outRect.Left = 0;
                outRect.Right = 0;
                outRect.Top = 0;

                //var cPosition = parent.GetChildAdapterPosition(view);

                //if (cPosition % 2 != 0) return;

                //var nPosition = cPosition + 1;
                //var nView = parent.GetChildAt(nPosition);

                //var cParm = view.LayoutParameters;
                //var nParm = nView.LayoutParameters;


                //double cRatio = view.Height / view.Width;
                //double nRatio = nView.Height / nView.Width;

                //var totalWidth = parent.Width;

                //int fwidth = totalWidth / 2;

                

                //cParm.Width = fwidth;
                //cParm.Height = (int)(cRatio * cParm.Width);

                //nParm.Width = fwidth;
                //nParm.Height = (int)(nRatio * nParm.Width);
            }
        }
    }
}