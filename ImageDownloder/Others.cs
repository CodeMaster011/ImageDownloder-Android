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
    public class ImageDefinition
    {
        public string thumbnil { get; set; }
        public string original { get; set; }
    }
}