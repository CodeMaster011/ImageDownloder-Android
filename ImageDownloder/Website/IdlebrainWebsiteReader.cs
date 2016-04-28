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
using HtmlAgilityPack;
using static ImageDownloder.MyGlobal;

namespace ImageDownloder.Website
{
    class IdlebrainWebsiteReader : IWebsiteReader
    {
        public IWebPageReader HomePageReader
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IWebPageReader IndexPageReader
        {
            get
            {
                return new IdlebrainIndexPageReader();
            }
        }

        class IdlebrainIndexPageReader : IWebPageReader
        {
            private WebPageData[] cached = null;
            private const int fragmentCutOff = 20;

            public bool IsPrefereOffline { get; } = true;
            public PreferedViewing Viewing { get; } = PreferedViewing.List;
            public string Url { get { return "http://www.idlebrain.com/movie/photogallery/heroines.html"; } }
            public bool IsDownloadRequired { get; set; } = true;
            public bool IsSimulation { get; set; } = false;
            public bool IsFragmentSubmissiable { get; } = true;
            public FragmentSubmission FragmentSubmissionCallback { get; set; } = null;

            public WebPageData[] ExtractData(HtmlDocument doc)
            {
                if (doc == null && cached != null) return cached;   //return data from cache

                if (doc == null) return null;   //error protection

                Dictionary<string, string> att = new Dictionary<string, string>();
                att.Add("align", "left");
                try
                {
                    var container = Helper.AllChild(doc.DocumentNode, "div", att, true)[1];
                    if (container == null) return null;

                    //container = Helper.AnyChild(container, "p");

                    string innHtml = container.InnerHtml;

                    string[] stElements = innHtml.Split(new string[] { "<br>"}, StringSplitOptions.RemoveEmptyEntries);

                    List<WebPageData> data = new List<WebPageData>();
                    int index = 0;

                    foreach (var st in stElements)
                    {
                        var sss =$"<div>{Helper.TrimToEntry(st)}</div>";
                        HtmlNode pNode = HtmlNode.CreateNode(sss);
                        var mainNode = Helper.AnyChild(pNode, "b");
                        if (mainNode != null)
                        {
                            var aLinkNodes = Helper.AllChild(pNode, "a");
                            var singleData = WebPageData.GetTextOnly(
                                mainNode.InnerText.Replace(':','\0'), 
                                aLinkNodes != null ? $"Contain : {aLinkNodes.Count.ToString()}" : "");

                            singleData.underlayingLinkReader = new IdlebrainSimulatedPage(sss);
                            singleData.IsFinal = true;

                            data.Add(singleData);

                            index++;
                            if (index % fragmentCutOff == 0 && FragmentSubmissionCallback != null)
                                FragmentSubmissionCallback(data.ToArray());
                        }
                    }
                    this.IsSimulation = true;
                    return cached = data.ToArray();
                }
                catch (Exception) { }
                return null;
            }
        }
        class IdlebrainSimulatedPage : IWebPageReader
        {
            private const string webFolder = "http://www.idlebrain.com/movie/photogallery/";

            public bool IsPrefereOffline { get; } = true;
            public PreferedViewing Viewing { get; } = PreferedViewing.List;
            public string Url { get { return ""; } }
            public bool IsDownloadRequired { get; set; } = false;            
            public bool IsSimulation { get; set; } = true;
            public bool IsFragmentSubmissiable { get; } = false;
            public FragmentSubmission FragmentSubmissionCallback { get; set; } = null;

            public string content { get; } = string.Empty;

            public WebPageData[] ExtractData(HtmlDocument doc)
            {
                List<WebPageData> data = new List<WebPageData>();
                var container = HtmlNode.CreateNode(content);
                var mainNode = Helper.AnyChild(container, "b");
                if (mainNode != null)
                {
                    var aLinkNodes = Helper.AllChild(container, "a");
                    if (aLinkNodes == null) return null;
                    foreach (var alinkNode in aLinkNodes)
                    {
                        string link = "";
                        if (alinkNode.HasAttributes) link = alinkNode.GetAttributeValue("href", "");
                        link = link.Contains(webFolder) ? "" : webFolder + link;

                        WebPageData singleData = WebPageData.GetTextOnly($"{alinkNode.InnerText} - {mainNode.InnerText.Replace(':', '\0')}", "");
                        singleData.IsFinal = true;
                        singleData.underlayingLinkReader = new IdlebrainAlbumPageReader(link);

                        data.Add(singleData);
                    }
                    return data.ToArray();
                }
                return null;
            }
            public IdlebrainSimulatedPage(string content)
            {
                this.content = content;
            }
        }
        class IdlebrainAlbumPageReader : IWebPageReader
        {
            private readonly string webDir = "";
            private const int fragmentCutOff = 20;

            public List<ImageDefinition> AlbumImages { get; set; } = null;
            public bool IsPrefereOffline { get; } = true;
            public PreferedViewing Viewing { get; } = PreferedViewing.Grid;
            public string Url { get; }
            public bool IsDownloadRequired { get; set; } = true;
            public bool IsSimulation { get; set; } = false;
            public bool IsFragmentSubmissiable { get; } = true;
            public FragmentSubmission FragmentSubmissionCallback { get; set; } = null;

            public WebPageData[] ExtractData(HtmlDocument doc)
            {
                Dictionary<string, string> att = new Dictionary<string, string>();
                att.Add("width", "100%");
                att.Add("style", "background-color: white;");
                
                var container = Helper.AnyChild(doc.DocumentNode, "table", att, true);
                if (container == null) return null;

                //TODO : add support for http://www.idlebrain.com/movie/photogallery/madhusharma1.html

                var imgNodes = Helper.AllChild(container, "img");
                if (imgNodes == null) return null;

                int index = 0;
                List<WebPageData> data = new List<WebPageData>();

                foreach (var imgNode in imgNodes)
                {
                    string thSrc = (webDir.EndsWith("/") ? webDir : webDir + "/") + imgNode.GetAttributeValue("src", "");

                    var singleData = WebPageData.GetTextOnly($"{index++}", "");
                    singleData.IsFinal = true;
                    singleData.Index = index;
                    singleData.UID = UidGenerator();

                    singleData.ImageUrl = thSrc;    //image link

                    //analysisModule.RequestImageData(singleData.UID, thSrc, 
                    //    new ImageResponseAction((string uid, string requestedUrl, Android.Graphics.Bitmap bitmap) => 
                    //    {
                    //        singleData.drawable = bitmap;
                    //        NotifyDataUpdate?.Invoke(singleData.Index); //invoke UI update
                    //    }));

                    data.Add(singleData);

                    if (index % fragmentCutOff == 0 && FragmentSubmissionCallback != null)
                        FragmentSubmissionCallback(data.ToArray());
                }
                return data.ToArray();
            }
            
            private string getWebFolderPath(string url)
            {
                //for (int i = url.Length - 1; i >= 0; i--)
                //{
                //    if(url.)
                //}
                var index = url.LastIndexOf('/');
                return url.Substring(0, index);
            }
            public IdlebrainAlbumPageReader(string url)
            {
                Url = url;
                webDir = getWebFolderPath(url);
            }
        }
    }
}