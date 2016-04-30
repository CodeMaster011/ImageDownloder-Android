using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Squareup.Picasso;
using Android.Util;

namespace ImageDownloder
{
    static class MyPicasso
    {        
        public static string GetFormatedKey(string url, int width = 0, int height = 0, bool centerInside = false)
        {
            string cenInside = centerInside ? "centerInside\n" : string.Empty;
            string resize = width == 0 || height == 0 ? string.Empty : $"resize:{width}x{height}\n";
            return $"{url}\n{resize}{cenInside}";
        }
        public static void CreateNewPicasso(Context context)
        {
            var mPicassoBuilder = new Picasso.Builder(context);
            mPicassoBuilder.MemoryCache(MyGlobal.memoryCache);
            var mPicasso = mPicassoBuilder.Build();
            Picasso.SetSingletonInstance(mPicasso);
        }
        public class MyCache : Java.Lang.Object, Squareup.Picasso.ICache
        {
            private Dictionary<string, Bitmap> memory = new Dictionary<string, Bitmap>();
            private Queue<string> bigImages = new Queue<string>();

            public int ThumbnailSize { get; set; } = 60 * 1024;

            public void Clear()
            {
                lock (memory)
                {
                    foreach (var item in memory)
                    {
                        item.Value.Dispose();
                    }
                    memory.Clear();

                    //Log.Debug("MY_PICASSO", "============CLEARED============");

                    //Size();
                }
            }

            public void ClearKeyUri(string p0)
            {
                lock (memory)
                {
                    string key = p0;
                    if (memory.ContainsKey(key))
                    {
                        memory[key].Dispose();
                        memory.Remove(key);

                        Log.Debug("MY_PICASSO", "LINK REMOVED " + key);

                        //Size();
                    }
                }                
            }

            public void ClearBigImages()
            {
                lock (memory)
                {
                    while (bigImages.Count > 0)
                    {
                        ClearKeyUri(bigImages.Dequeue());
                    }
                    Log.Debug("MY_PICASSO", $"BIG IMAGE CLEARED");
                    Size();
                }
            }

            public Bitmap Get(string p0)
            {
                //Log.Debug("MY_PICASSO", "GET KEY = " + p0);

                if (memory.ContainsKey(p0)) return memory[p0];
                else return null;
            }

            public int MaxSize()
            {
                return 30 * 1024 * 1024;
            }

            public void Set(string p0, Bitmap p1)
            {
                string key = p0;
                if (!memory.ContainsKey(key))
                {
                    memory.Add(key, p1);

                    if (p1.AllocationByteCount > ThumbnailSize)
                        bigImages.Enqueue(key);

                    //TODO: Simplify the process of size counting
                    var difference = MaxSize() - Size();

                    while (difference <= 0 && bigImages.Count > 0)
                    {
                        //cache is full
                        //delete some big images
                        string tempKey = bigImages.Dequeue();
                        if (memory.ContainsKey(tempKey))
                        {
                            var size = memory[tempKey].AllocationByteCount;

                            Log.Debug("MY_PICASSO", $"BIG IMAGE DELETED = {tempKey}");

                            ClearKeyUri(tempKey);

                            difference += size;
                        }
                    }

                    //Log.Debug("MY_PICASSO", "ADD KEY = " + p0);

                    //Size();
                }
            }

            public int Size()
            {
                int size = 0;
                lock (memory)
                {
                    try
                    {
                        foreach (var item in memory)
                        {
                            size += item.Value.AllocationByteCount;
                        }
                    }
                    catch (Exception) { }
                    Log.Debug("MY_PICASSO", $"CACHE SIZE = {size}");
                }
                return size;
            }
            
            public Bitmap decodeSampledBitmapFromResource(Android.Content.Res.Resources res, int resId, int reqWidth, int reqHeight)
            {

                // First decode with inJustDecodeBounds=true to check dimensions
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = true;
                BitmapFactory.DecodeResource(res, resId, options);
                
                // Calculate inSampleSize
                options.InSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

                // Decode bitmap with inSampleSize set
                options.InJustDecodeBounds = false;
                return BitmapFactory.DecodeResource(res, resId, options);
            }
            public int calculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
            {
                // Raw height and width of image
                int height = options.OutHeight;
                int width = options.OutWidth;
                int inSampleSize = 1;

                if (height > reqHeight || width > reqWidth)
                {

                    int halfHeight = height / 2;
                    int halfWidth = width / 2;

                    // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                    // height and width larger than the requested height and width.
                    while ((halfHeight / inSampleSize) > reqHeight
                            && (halfWidth / inSampleSize) > reqWidth)
                    {
                        inSampleSize *= 2;
                    }
                }

                return inSampleSize;
            }
        }
    }
}