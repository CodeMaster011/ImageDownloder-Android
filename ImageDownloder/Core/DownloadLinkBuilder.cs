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
using System.Threading;

namespace ImageDownloder.Core
{
    class DownloadLinkBuilder : IResponseHandler
    {
        private IWebPageReader pageReader = null;
        private List<ImageDefinition> knownImages = null;
        private List<ImageDownloadInformation> list = null;
        private string desFolder = null, websiteName = null;
        private Action loadingCompleted = null;

        public DownloadLinkBuilder(string websiteName, IWebPageReader pageReader, List<ImageDefinition> knownImages)
        {
            this.pageReader = pageReader;
            this.knownImages = knownImages;
            this.websiteName = websiteName;
        }
        public DownloadLinkBuilder DownloadTo(string desFolder)
        {
            this.desFolder = desFolder;
            if (!System.IO.Directory.Exists(desFolder))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(desFolder);
                }
                catch (Exception) { }
            }
            return this;
        }
        public DownloadLinkBuilder OnLoadingCompleted(Action loadingCompleted)
        {
            this.loadingCompleted = loadingCompleted;
            return this;
        }

        public void BuildInto(ref List<ImageDownloadInformation> list)
        {
            this.list = list;
            new Thread(build).Start();
        }
        private void build()
        {
            loadImageDefinition(knownImages);
            knownImages = null; //remove reference

            var nextPageUrl = pageReader.GetNextPage().Url;
            readAll(nextPageUrl);
            loadingCompleted?.Invoke();
        }

        private string responseData = null;

        private void readAll(string start)
        {
            responseData = null;
            MyGlobal.offlineModule.RequestData(RequestPacket.CreateStringPacket(MyGlobal.UidGenerator(), start, pageReader, RequestPacketOwners.UI), this);
            wait(ref responseData);

            string nextPageUrl = null;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(responseData);
            var data = ((IBigImageCollectionHolder)pageReader).GetImages(start, doc, out nextPageUrl);
            loadImageDefinition(data);
            if (nextPageUrl != null && nextPageUrl != string.Empty)
                readAll(nextPageUrl);
        }

        private void loadImageDefinition(List<ImageDefinition> data)
        {
            lock (data)
            {
                foreach (var item in data)
                {
                    list.Add(new ImageDownloadInformation()
                    {
                        src = item,
                        Name = getName(item.original),
                        websiteName = websiteName,
                        des = createDestinationPath(item.original, desFolder)
                    });
                }
            }            
        }

        private void wait(ref string data)
        {
            while (data == null)
            {
                Thread.Sleep(10);
            }
        }
        private string getName(string src)
        {
            var ss = src.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var fName = ss[ss.Length - 1];
            return fName;
        }
        private string createDestinationPath(string src, string desFolder)
        {
            var ss = src.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var fName = ss[ss.Length - 1];
            if (!desFolder.EndsWith("/")) desFolder += "/";
            return desFolder + fName;
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            responseData = requestPacket.DataInString;
            requestPacket.Dispose();
        }

        public void RequestProcessingError(RequestPacket requestPacket)
        {
            
        }
    }
}