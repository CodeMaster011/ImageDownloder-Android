using System.Collections.Generic;
using System;

namespace ImageDownloder
{
    public delegate void ImageResponseAction(string uid, string requestedUrl, Android.Graphics.Bitmap bitmap);
    interface IResponseHandler
    {
        void RequestProcessedCallback(RequestPacket requestPacket);
        void RequestProcessingError(RequestPacket requestPacket);
    }
    interface IUiResponseHandler
    {
        void RequestProcessedCallback(string uid, string requestedUrl, WebPageData[] data);
        void RequestProcessingError(string uid, string requestedUrl, string error);
    }
    class HistoryObject: IDisposable
    {
        public IWebPageReader webpageReader { get; private set; } = null;
        public int clickedPosition { get; private set; } = 0;
        public HistoryObject(IWebPageReader webpageReader, int clickedPosition)
        {
            this.webpageReader = webpageReader;
            this.clickedPosition = clickedPosition;
        }

        public void Dispose()
        {
            webpageReader = null;
            clickedPosition = 0;
        }
        ~HistoryObject()
        {
            Dispose();
        }
    }

    //TODO: RequestPacket -> refine the packet and make some properties like StringData, BitmapData, Owner, Uid, URL, Callbacks etc.
    class RequestPacket : IDisposable, ICloneable
    {
        public Dictionary<string, object> requestObjs = null;
        public RequestPacket()
        {
            requestObjs = new Dictionary<string, object>();
        }
        public RequestPacket Add(string key,object value)
        {
            requestObjs.Add(key, value);
            return this;
        }

        public T Get<T>(string key)
        {
            if (!requestObjs.ContainsKey(key)) return default(T);
            return (T)requestObjs[key];
        }

        public void Dispose()
        {
            requestObjs = null;
        }

        public object Clone()
        {
            var n = new RequestPacket();
            foreach (var item in requestObjs)
            {
                n.Add(item.Key, item.Value);
            }
            return n;
        }

        ~RequestPacket()
        {
            Dispose();
        }
    }

    //TODO: ImageDefinition -> Attach and fill with data from IdlebrainAlbumPageReader
    class ImageDefinition
    {
        public string thumbnail { get; } = string.Empty;
        public string original { get; } = string.Empty;
    }
}