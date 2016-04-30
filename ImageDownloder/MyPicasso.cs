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

            public int ThumbnailSize { get; set; } = 50 * 1024;

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
                string key = p0.Replace("\n", "");
                if (!memory.ContainsKey(key))
                {
                    memory.Add(key, p1);

                    if (p1.ByteCount > ThumbnailSize)   //TODO: Check some thumbnail is added to the queue
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
                            var size = memory[tempKey].ByteCount;

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
                            size += item.Value.ByteCount;
                        }
                    }
                    catch (Exception) { }
                    Log.Debug("MY_PICASSO", $"CACHE SIZE = {size}");
                }
                return size;
            }
        }
    }
}