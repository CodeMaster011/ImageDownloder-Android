using Android.Graphics;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Android.OS;
using Android.Runtime;
using static ImageDownloder.MyGlobal;
using Android.Graphics.Drawables;

namespace ImageDownloder
{
    //TODO: Design the 4 layer architecture of website processing
    public interface IWebsiteReader
    {
        IWebPageReader HomePageReader { get; }
        IWebPageReader IndexPageReader { get; }        
    }
    public interface IWebPageReader
    {
        string Url { get; }
        PreferedViewing Viewing { get; }
        bool IsPrefereOffline { get; }
        bool IsDownloadRequired { get; set; }
        bool IsSimulation { get; set; }
        bool IsFragmentSubmissiable { get; }
        bool IsOnClickBigImage { get; }
        //FragmentSubmission FragmentSubmissionCallback { get; set; }

        IWebPageReader OnClickCallback(WebPageData item);
        WebPageData[] ExtractData(HtmlDocument doc);
    }
    public interface IBigImageCollectionHolder
    {
        List<ImageDefinition> AlbumImages { get; set; }
    }

    public delegate void FragmentSubmission(WebPageData[] data);

    public class WebPageData : IDisposable
    {
        //public Bitmap drawable { get; set; } = null;    
        public string ImageUrl { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public string UID { get; set; } = string.Empty;
        public string mainText { get; set; } = string.Empty;
        public string subText { get; set; } = string.Empty;
        public IWebPageReader underlayingLinkReader { get; set; } = null;
        public bool IsFinal { get; set; } = false;
        public object Tag { get; set; } = null;

        public static WebPageData GetFakeData()
        {
            WebPageData data = new WebPageData() { IsFinal = false };
            data.mainText = "Loading...";
            data.subText = "Loading...";
            data.underlayingLinkReader = null;

            return data;
        }
        public static WebPageData GetTextOnly(string mainText, string subText)
        {
            WebPageData data = new WebPageData() { IsFinal = false };
            data.mainText = mainText;
            data.subText = subText;
            data.underlayingLinkReader = null;
            
            return data;
        }

        public void Dispose()
        {            
            underlayingLinkReader = null;
        }
        ~WebPageData()
        {
            Dispose();
        }
    }
}