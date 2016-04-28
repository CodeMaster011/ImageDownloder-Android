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

//http://developer.android.com/reference/android/support/v4/view/ViewPager.html
//http://developer.android.com/training/displaying-bitmaps/cache-bitmap.html
//http://developer.android.com/training/displaying-bitmaps/load-bitmap.html
namespace ImageDownloder
{
    class WebsiteImageViewActivity: support.FragmentActivity
    {
        private ViewPager vPager = null;
        private PageAdapter pAdapter = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.website_image_view);

            vPager = FindViewById<ViewPager>(Resource.Id.viewPager);

            pAdapter = new PageAdapter(SupportFragmentManager);
            
        }

        class PageAdapter : support.FragmentPagerAdapter, ViewPager.IOnPageChangeListener
        {
            
            public override int Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override support.Fragment GetItem(int position)
            {
                throw new NotImplementedException();
            }

            public void OnPageScrollStateChanged(int state)
            {
                
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                
            }

            public void OnPageSelected(int position)
            {
                
            }

            public PageAdapter(support.FragmentManager fm):base(fm)
            {

            }
        }
    }
}