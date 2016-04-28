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
    

    //TODO: ImageDefinition -> Attach and fill with data from IdlebrainAlbumPageReader
    class ImageDefinition
    {
        public string thumbnail { get; } = string.Empty;
        public string original { get; } = string.Empty;
    }
}