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
//http://developer.android.com/reference/android/support/v4/view/ViewPager.html
//http://developer.android.com/training/displaying-bitmaps/cache-bitmap.html
//http://developer.android.com/training/displaying-bitmaps/load-bitmap.html
namespace ImageDownloder
{
    [Android.App.Activity(Label = "WebsiteBrowserActivity")]
    class WebsiteImageViewActivity: FragmentActivity
    {
        private ViewPager vPager = null;
        private PageAdapter pAdapter = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_image_view);

            vPager = FindViewById<ViewPager>(Resource.Id.viewPager);

            pAdapter = new PageAdapter(SupportFragmentManager);
            vPager.Adapter = pAdapter;
        }

        class PageAdapter : FragmentPagerAdapter, ViewPager.IOnPageChangeListener
        {
            private const int MAX_FRAGMENT = 4;
            private Queue<PageAdapterFragment> fragments = new Queue<PageAdapterFragment>(MAX_FRAGMENT);

            public override int Count
            {
                get
                {
                    return albumImages.Count;
                }
            }
            public override Java.Lang.Object InstantiateItem(View container, int position)
            {
                return base.InstantiateItem(container, position);

            }
            public override Fragment GetItem(int position)
            {
                //if (fragments.Count >= MAX_FRAGMENT)
                //{
                //    var fr = fragments.Dequeue();
                //    fr.imageDefinition = albumImages[position];
                //    fragments.Enqueue(fr);
                //    return fr;
                //}
                //else
                //{
                //    var fr = new PageAdapterFragment() { imageDefinition = albumImages[position] };
                //    fragments.Enqueue(fr);
                //    return fr;
                //}
                return new PageAdapterFragment() { imageDefinition = albumImages[position] };
            }
            public override void DestroyItem(View container, int position, Java.Lang.Object objectValue)
            {
                ((ViewPager)container).RemoveView((View)objectValue);
            }

            public void OnPageScrollStateChanged(int state)
            {
                
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                currenItemPosition = position;
            }

            public void OnPageSelected(int position)
            {
                
            }

            public PageAdapter(support.FragmentManager fm):base(fm)
            {

            }
        }

        class PageAdapterFragment : Fragment
        {
            public ImageDefinition imageDefinition { get; set; } = null;
            public bool IsOriginalLoaded { get; set; } = false;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var v = inflater.Inflate(Resource.Layout.website_image_view_fragment, container, false);
                var imgView = v.FindViewById<ImageView>(Resource.Id.bigImageView);

                Picasso.With(Context).Load(imageDefinition.thumbnil).Into(imgView);

                if(IsOriginalLoaded)
                    Picasso.With(Context).Load(imageDefinition.original).Into(imgView);
                else
                    Picasso.With(Context).Load(imageDefinition.original).
                        Into(new ImageLoadTarget() { imageView = imgView, parent = this });

                return v;
            }
        }
        class ImageLoadTarget : Java.Lang.Object, ITarget
        {
            public ImageView imageView { get; set; } = null;
            public PageAdapterFragment parent { get; set; } = null;

            public void OnBitmapFailed(Drawable p0)
            {
                
            }

            public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
            {
                imageView.SetImageBitmap(p0);
                parent.IsOriginalLoaded = true;
            }

            public void OnPrepareLoad(Drawable p0)
            {
                
            }
        }
        class PageAdapterFragmentViewHolder
        {
            public ImageView imageView { get; set; } = null;

            public PageAdapterFragmentViewHolder(View v)
            {
                imageView = v.FindViewById<ImageView>(Resource.Id.bigImageView);
            }
            public PageAdapterFragmentViewHolder() { }
        }
    }
}