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
using ImageDownloder.Core.Architecture;

namespace ImageDownloder.Website
{
    class IndiancinemagalleryWebsiteArchitecture : WebsiteArchitecture
    {
        public IndiancinemagalleryWebsiteArchitecture()
            : base(new IndiancinemagalleryListOfPersonReader(), null, new IndiancinemagalleryListOfImages())
        { }
        class IndiancinemagalleryListOfPersonReader : ListOfPersonReader
        {
            private const string webDirectory = "http://www.indiancinemagallery.com/";

            public override string Url { get; set; } = "http://www.indiancinemagallery.com/telugu/gallery/actress/alphabet.html";

            public override List<Person> GetListOfPerson(HtmlDocument doc)
            {
                Dictionary<string, string> attList = new Dictionary<string, string>();
                attList.Add("class", "wrap");
                attList.Add("style", "width:728px; margin-left:5px");
                var container = Helper.AnyChild(doc.DocumentNode, "div", attList);
                if (container == null) return null;

                var listOfLiTag = Helper.AllChild(container, "li");
                if (listOfLiTag == null) return null;

                List<Person> persons = new List<Person>();
                foreach (var li in listOfLiTag)
                {
                    var linkNode = Helper.AnyChild(li, "a");
                    if (linkNode != null)
                    {
                        Person p = new Person();
                        p.InformationNeedForNextLevel = webDirectory + linkNode.GetAttributeValue("href", "");

                        var text = li.InnerText;
                        var ss = text.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);

                        p.Name = ss[0];
                        //p.Description = $"{int.Parse(ss[ss.Length - 1])} photos";
                        p.NoOfAlbums = int.Parse(ss[ss.Length - 1]);
                        persons.Add(p);
                    }
                }

                return persons;
            }
        }

        class IndiancinemagalleryListOfImages : ListOfImagesReader
        {
            private const string webDirectory = "http://www.indiancinemagallery.com/";
            private string nextPageUrl = string.Empty;

            public override bool IsMultiPaged { get; set; } = true;

            public override string GetNextPageUrl()
            {
                return nextPageUrl;
            }

            public override List<ImageInfo> GetImageList(Album album, HtmlDocument doc)
            {
                var container = doc.GetElementbyId("columns4");
                if (container == null) return null;

                var imgList = Helper.AllChild(container, "img");
                if (imgList == null) return null;

                List<ImageInfo> data = new List<ImageInfo>();
                foreach (var img in imgList)
                {
                    ImageInfo info = new ImageInfo();
                    ImageDefinition def = new ImageDefinition();

                    def.thumbnil = webDirectory + img.GetAttributeValue("src", "");
                    def.original = def.thumbnil.Replace("thumb_", "");

                    info.Image = def;

                    data.Add(info);
                }

                var pagination = Helper.AnyChild(doc.DocumentNode, "div", "pagination");
                if (pagination != null)
                {
                    bool isNextPageFound = false;

                    var current = int.Parse(Helper.AnyChild(pagination, "span", "current").InnerText);
                    var pageLinks = Helper.AllChild(pagination, "a");
                    foreach (var page in pageLinks)
                    {
                        if (page.InnerText == (current + 1).ToString())
                        {
                            nextPageUrl = webDirectory + page.GetAttributeValue("href", "");
                            isNextPageFound = true;
                            break;
                        }
                    }
                    if (!isNextPageFound) nextPageUrl = string.Empty;
                }

                return data;
            }

            public override string GetUrl(Album album)
            {
                return (string)album.InformationNeedForNextLevel;
            }
        }
    }    
}