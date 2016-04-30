using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static ImageDownloder.MyGlobal;
using Android.Support.V4.View;
using support = Android.Support.V4.App;
using Android.Support.V4.App;
using Squareup.Picasso;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.Lang;
using Android.Util;
//http://developer.android.com/reference/android/support/v4/view/ViewPager.html
//http://developer.android.com/training/displaying-bitmaps/cache-bitmap.html
//http://developer.android.com/training/displaying-bitmaps/load-bitmap.html
namespace ImageDownloder
{
    [Android.App.Activity(Label = "WebsiteBrowserActivity")]
    class WebsiteImageViewActivity: FragmentActivity
    {
        private ViewPager vPager = null;
        private PagerAdapter pAdapter = null;

        public override void OnBackPressed()
        {
            //Picasso.With(this).CancelTag(pAdapter);
            vPager.RemoveAllViews();
            vPager = null;
            pAdapter = null;

            memoryCache.ClearBigImages();

            Android.Util.Log.Debug("WebsiteImageViewActivity",
                $"Request Packet ={MyGlobal.requestPacketCount}, History Obj ={MyGlobal.historyObjCount}");

            Finish();

            base.OnBackPressed();            
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_image_view);

            vPager = FindViewById<ViewPager>(Resource.Id.viewPager);

            pAdapter = new P_Ad() { context = this };//new PageAdapter(SupportFragmentManager);
            vPager.OffscreenPageLimit = 0;
            vPager.Adapter = pAdapter;
            vPager.CurrentItem = currenItemPosition;
            vPager.AddOnPageChangeListener((ViewPager.IOnPageChangeListener)pAdapter);
        }
        
        class P_Ad : PagerAdapter, ViewPager.IOnPageChangeListener
        {
            public Context context { get; set; } = null;
            private Queue<ImageView> freeItem = new Queue<ImageView>();

            public P_Ad()
            {

            }
            public override int Count
            {
                get
                {
                    return albumImages.Count;
                }
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return objectValue == view;
            }
            public override Java.Lang.Object InstantiateItem(View container, int position)
            {
                var layoutInflator = (LayoutInflater)context.GetSystemService(LayoutInflaterService);

                ImageView imageView = null;
                if (freeItem.Count > 0)
                {
                    imageView = freeItem.Dequeue();
                    Log.Debug("IMAGE_VIEW", $"***OLD OBJECT USED***({freeItem.Count})");
                }
                else
                {
                    imageView = new ImageView(context);
                    Log.Debug("IMAGE_VIEW", "=====NEW OBJECT CREATED========");
                }

                //Picasso.With(context).Load(albumImages[position].thumbnil).Priority(Picasso.Priority.High).Into(imageView, 
                //    new C_CC() { imageView = imageView, originalUrl = albumImages[position].original });
                Picasso.With(context).Load(albumImages[position].thumbnil).Resize(128, 128).CenterInside().Priority(Picasso.Priority.High).Into(imageView);
                Picasso.With(context).Load(albumImages[position].original).Resize(screenSize.Width,screenSize.Height).CenterInside().NoPlaceholder().Into(imageView);
                                
                try
                {
                    Picasso.With(context).Load(albumImages[position + 1].original).Resize(screenSize.Width, screenSize.Height).CenterInside().Fetch();
                    Picasso.With(context).Load(albumImages[position + 1].thumbnil).Resize(128, 128).CenterInside().Priority(Picasso.Priority.High).Fetch();
                    
                }
                catch (System.Exception) { }

                try
                {
                    Picasso.With(context).Load(albumImages[position - 1].original).Resize(screenSize.Width, screenSize.Height).CenterInside().Fetch();
                    Picasso.With(context).Load(albumImages[position - 1].thumbnil).Resize(128, 128).CenterInside().Priority(Picasso.Priority.High).Fetch();
                }
                catch (System.Exception) { }

                ((ViewPager)container).AddView(imageView);
                return imageView;
            }
            public override void DestroyItem(View container, int position, Java.Lang.Object objectValue)
            {
                ((ViewPager)container).RemoveView((View)objectValue);
                freeItem.Enqueue((ImageView)objectValue);
                //Log.Debug("IMAGE_VIEW", $"=*=*=*=OBJECT DELETED=*=*=*=({freeItem.Count})");
                //Picasso.With(context).CancelRequest((ImageView)objectValue);

                //memoryCache.Size();

                memoryCache.ClearKeyUri(MyPicasso.GetFormatedKey(albumImages[position].original, screenSize.Width, screenSize.Height, true));
            }

            public void OnPageScrollStateChanged(int state)
            {
                
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                
            }

            public void OnPageSelected(int position)
            {
                currenItemPosition = position;
            }
        }
    }
}