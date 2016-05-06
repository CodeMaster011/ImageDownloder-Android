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
    class BharatStudentWebsiteArchitecture : WebsiteArchitecture
    {
        public BharatStudentWebsiteArchitecture()
            : base("Bbharat Student", "BS", new BharatStudentListOfPerson(), null, new BharatStudentListOfImage())
        { }

        class BharatStudentListOfPerson : ListOfPersonReader
        {
            public override string Url { get; set; } = "http://www.bharatstudent.com/cafebharat/photo_gallery_4-Hindi-Actress-Photo-Galleries-1,2.php";

            private const string webDirectory = "http://www.bharatstudent.com/cafebharat/";


            public override List<Person> GetListOfPerson(HtmlDocument doc)
            {
                var allLists = Helper.AllChild(doc.DocumentNode, "ul", "bullettext1");
                if (allLists == null) return null;

                var list = new List<Person>();

                foreach (var section in allLists)
                {
                    var allItems = Helper.AllChild(section, "li");
                    if (allItems != null)
                    {
                        foreach (var item in allItems)
                        {
                            var aLinkNode = Helper.AnyChild(item, "a");
                            list.Add(
                                new Person()
                                {
                                    Name = HtmlEntity.DeEntitize(aLinkNode.InnerText),
                                    InformationNeedForNextLevel = webDirectory + aLinkNode.GetAttributeValue("href", "")
                                }
                                );
                        }
                    }                    
                }
                return list.Count == 0 ? null : list;
            }
        }



        class BharatStudentListOfImage : ListOfImagesReader
        {
            //http://www.bharatstudent.com/ng7uvideo/bs/gallery/normal/actress/bw/2007/march/ayeshatakia/ayeshatakia_070.jpg
            //http://www.bharatstudent.com/ng7uvideo/bs/gallery/thumb/actress/bw/2007/march/ayeshatakia/ayeshatakia_070.jpg

            private const string webDirectory = "http://www.bharatstudent.com/cafebharat/";
            private string nextPageUrl = string.Empty;

            public override bool IsMultiPaged { get; set; } = true;

            public override string GetNextPageUrl() => nextPageUrl;

            public override List<ImageInfo> GetImageList(Album album, HtmlDocument doc)
            {
                var aLinksNodes = Helper.AllChild(doc.DocumentNode, "a", 
                    new SearchCritriaBuilder()
                    .AddHasChild(new ChildNode() { Name = "img" })
                    .AddHasAttribute("class", "greylink")
                    .Build());
                if (aLinksNodes == null) return null;

                var list = new List<ImageInfo>();
                foreach (var aNode in aLinksNodes)
                {
                    var imgNode = Helper.AnyChild(aNode, "img");
                    list.Add(
                        new ImageInfo()
                        {
                            Image = new ImageDefinition()
                            {
                                thumbnil = imgNode.GetAttributeValue("src", ""),
                                original = imgNode.GetAttributeValue("src", "").Replace("/thumb/", "/normal/")
                            }
                        }
                        );
                }
                //td width="28%" height="30" align="right" valign="middle"
                var pageContainer = Helper.AnyChild(doc.DocumentNode, "td", new Dictionary<string, string>()
                {
                    ["width"] = "28%",
                    ["height"] = "30",
                    ["align"] = "right",
                    ["valign"] = "middle"
                });
                if (pageContainer != null)
                {
                    //<span class="bluepadlink">1</span>
                    var currentPageIndex = int.Parse(Helper.AnyChild(pageContainer, "span", "bluepadlink").InnerText);
                    //<a href="photo_gallery_2-Hindi-Actress-Ayesha_Takia-photo-galleries-1,2,6,2.php" class='greypadlink'>2</a>
                    var allPageLinks = Helper.AllChild(pageContainer, "a");
                    foreach (var page in allPageLinks)
                    {
                        if(page.InnerText == (currentPageIndex + 1).ToString())
                        {
                            nextPageUrl = webDirectory + page.GetAttributeValue("href", "");
                            break;
                        }
                    }
                }

                return list.Count == 0 ? null : list;
            }

            public override string GetUrl(Album album) => album.InformationNeedForNextLevel as string;
        }
    }
}