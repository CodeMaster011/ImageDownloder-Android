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
using Android.Graphics;
namespace ImageDownloder
{
    class ImageProvider
    {
        private ICache diskCache = null;
        private IOnlineModule onlineModule = null;
        private ICache memoryImageCache = null;

        public Bitmap Default { get; set; } = null;

        public ImageProvider(ICache memoryImageCache, ICache diskCache,  IOnlineModule onlineModule)
        {
            this.diskCache = diskCache;
            this.onlineModule = onlineModule;
            this.memoryImageCache = memoryImageCache;          
        }

        public Bitmap GetBitmap(string url)
        {
            var result = memoryImageCache.GetBitmap(url);
            if (result != null) return result;

            //the image is not exist in memory
            
            //TODO: Make a request to download
            //TODO: Determine -> How make a notification as the file is downloaded to update ui

            return Default;
        }
    }

    class MemoryImageCache : ICache
    {
        private Dictionary<string, Bitmap> memory = new Dictionary<string, Bitmap>();

        public long CacheSize { get; }

        public Bitmap GetBitmap(string url)
        {
            if (IsKeyExist(url)) return memory[url];
            else return null;
        }
        
        public string GetString(string url)
        {
            return string.Empty;
        }

        public bool IsKeyExist(string url) => memory.ContainsKey(url);

        public bool IsKeyExist(string url, out string value)
        {
            value = string.Empty;
            return false;
        }

        public bool IsKeyExist(string url, out Bitmap value)
        {
            if (IsKeyExist(url))
            {
                value = GetBitmap(url);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool Put(string url, string value, bool update = false)
        {
            return false;
        }

        public bool Put(string url, Bitmap value, bool update = false)
        {
            if (update) Remove(url);
            else if (IsKeyExist(url)) return false;

            memory.Add(url, value);
            return true;
        }

        public bool Remove(string url)
        {
            try
            {
                if (IsKeyExist(url)) memory.Remove(url);
                else return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }           
        }
    }
}