using System.Collections.Generic;
using System;

namespace ImageDownloder
{
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
    //TODO: Fix the memory leak with HistoryObject
    class HistoryObject : IDisposable
    {
        public IWebPageReader webpageReader { get; private set; } = null;
        public WebPageData[] cachedData { get; private set; } = null;
        public int clickedPosition { get; private set; } = 0;
        public string title { get; private set; }

        public HistoryObject(IWebPageReader webpageReader, WebPageData[] cachedData,string title, int clickedPosition)
        {
            this.webpageReader = webpageReader;
            this.clickedPosition = clickedPosition;
            this.cachedData = cachedData;
            this.title = title;

            Android.Util.Log.Debug("HistoryObject", $"Created {++MyGlobal.historyObjCount}");
        }

        public void Dispose()
        {
            webpageReader = null;
            clickedPosition = 0;
            cachedData = null;
        }
        ~HistoryObject()
        {

            Dispose();
            Android.Util.Log.Debug("HistoryObject", $"GC Collected {--MyGlobal.historyObjCount}");
        }
    }
    public class ImageDefinition
    {
        public string thumbnil { get; set; }
        public string original { get; set; }
    }
    public class ImageDownloadInformation
    {
        public ImageDefinition src { get; set; } = null;
        public string des { get; set; } = null;
        public string Name { get; set; } = null;
        public string websiteName { get; set; } = null;
        public int currentSize { get; set; } = -1;
        public int totalSize { get; set; } = -1;
        public bool isDownloading { get; set; } = false;
        public bool isFailed { get; set; } = false;
        public bool isFinished { get; set; } = false;
    }
}