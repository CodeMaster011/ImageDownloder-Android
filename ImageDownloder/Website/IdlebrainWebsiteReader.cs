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
        public string Name { get; } = "Idlebrain";
        public string ComicText { get; } = "Ib";

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
            private IdlebrainSimulatedPage simulatedPage = new IdlebrainSimulatedPage("");
            private WebPageData[] cached = null;
            private const int fragmentCutOff = 20;

            public bool IsPrefereOffline { get; } = true;
            public bool IsOnClickBigImage { get; } = false;
            public PreferedViewing Viewing { get; } = PreferedViewing.List;
            public string Url { get { return "http://www.idlebrain.com/movie/photogallery/heroines.html"; }}
            public bool IsDownloadRequired { get; set; } = true;
            public bool IsSimulation { get; set; } = false;
            public bool IsFragmentSubmissiable { get; } = true;

            public bool IsMultiPaged { get; set; } = false;
            public IWebPageReader GetNextPage() => null;

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

                            //singleData.underlayingLinkReader = new IdlebrainSimulatedPage(sss);//TODO: MEMORY LEAK -> don't create new instance for every item
                            singleData.Tag = sss;   //Add the data as tag

                            singleData.IsFinal = true;

                            data.Add(singleData);
                        }
                    }
                    this.IsSimulation = true;
                    return cached = data.ToArray();
                }
                catch (Exception) { }
                return null;
            }

            public IWebPageReader OnClickCallback(WebPageData item)
            {
                string content = (string)item.Tag;
                simulatedPage.content = content;
                return simulatedPage;
            }
        }
        class IdlebrainSimulatedPage : IWebPageReader
        {
            private IdlebrainAlbumPageReader albumPageReader = new IdlebrainAlbumPageReader();
            private const string webFolder = "http://www.idlebrain.com/movie/photogallery/";

            public bool IsPrefereOffline { get; } = true;
            public PreferedViewing Viewing { get; } = PreferedViewing.List;
            public string Url { get { return ""; } }
            public bool IsDownloadRequired { get; set; } = false;
            public bool IsOnClickBigImage { get; } = false;
            public bool IsSimulation { get; set; } = true;
            public bool IsFragmentSubmissiable { get; } = false;
            public bool IsMultiPaged { get; set; } = false;

            public IWebPageReader GetNextPage() => null;

            public string content { get; set; } = string.Empty;

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
                        //singleData.underlayingLinkReader = new IdlebrainAlbumPageReader(link);//TODO: MEMORY LEAK -> don't create new instance for every item
                        singleData.Tag = link;  //add the link in tag
                        data.Add(singleData);
                    }
                    return data.ToArray();
                }
                return null;
            }

            public IWebPageReader OnClickCallback(WebPageData item)
            {
                var link = (string)item.Tag;
                albumPageReader.ChangeUrl(link);
                return albumPageReader;
            }

            public IdlebrainSimulatedPage(string content)
            {
                this.content = content;
            }
        }
        class IdlebrainAlbumPageReader : IWebPageReader, IBigImageCollectionHolder
        {
            private string webDir = "";
            private const int fragmentCutOff = 20;

            public List<ImageDefinition> AlbumImages { get; set; } = new List<ImageDefinition>();
            public bool IsOnClickBigImage { get; } = true;
            public bool IsPrefereOffline { get; } = true;
            public PreferedViewing Viewing { get; } = PreferedViewing.Grid;
            public string Url { get; set; }
            public bool IsDownloadRequired { get; set; } = true;
            public bool IsSimulation { get; set; } = false;


            public bool IsMultiPaged { get; set; } = false;

            //http://www.idlebrain.com/movie/photogallery/aksha21/index.html
            //private int pageIndex = 1;
            public IWebPageReader GetNextPage()
            {
                //ChangeUrl($"http://www.idlebrain.com/movie/photogallery/aksha{pageIndex++}/index.html");
                return this;
            }

            public List<ImageDefinition> GetImages(string url, HtmlDocument doc, out string nextPageUrl)
            {
                var webDir = getWebFolderPath(url);
                nextPageUrl = null;

                Dictionary<string, string> att = new Dictionary<string, string>();

                att.Add("width", "100%");
                att.Add("style", "background-color: white;");

                var container = Helper.AnyChild(doc.DocumentNode, "table", att, true);
                if (container == null) return null;

                var imgNodes = Helper.AllChild(container, "img");
                if (imgNodes == null) return null;

                List<ImageDefinition> data = new List<ImageDefinition>();
                foreach (var imgNode in imgNodes)
                {
                    string thSrc = (webDir.EndsWith("/") ? webDir : webDir + "/") + imgNode.GetAttributeValue("src", "");                  

                    var imgDefi = new ImageDefinition()
                    {
                        thumbnil = thSrc,
                        original = (webDir.EndsWith("/") ? webDir : webDir + "/") + imgNode.GetAttributeValue("src", "").Replace("th_", "")
                    };

                    data.Add(imgDefi);
                }
                return data;
            }

            public WebPageData[] ExtractData(HtmlDocument doc)
            {
                AlbumImages.Clear();
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
                    singleData.UID = UidGenerator();

                    singleData.ImageUrl = thSrc;    //image link

                    var imgDefi = new ImageDefinition()
                    {
                        thumbnil = thSrc,
                        original = (webDir.EndsWith("/") ? webDir : webDir + "/") + imgNode.GetAttributeValue("src", "").Replace("th_","")
                    };

                    AlbumImages.Add(imgDefi);

                    data.Add(singleData);
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

            public IWebPageReader OnClickCallback(WebPageData item)
            {
                return null;
            }

            public IdlebrainAlbumPageReader(string url)
            {
                ChangeUrl(url);
            }

            public IdlebrainAlbumPageReader()
            {

            }
            public void ChangeUrl(string url)
            {
                Url = url;
                webDir = getWebFolderPath(url);
            }
        }
    }
}