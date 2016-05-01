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

namespace ImageDownloder.Core.Architecture
{
    internal class Person
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NoOfAlbums { get; set; } = -1;
        public object InformationNeedForNextLevel { get; set; } = null;
    }

    internal abstract class ListOfPersonReader: IWebPageMetaData
    {
        public abstract string Url { get; set; }
        public virtual PreferedViewing Viewing { get; set; } = PreferedViewing.List;
        public virtual bool IsPrefereOffline { get; set; } = true;
        public virtual bool IsDownloadRequired { get; set; } = true;
        public virtual bool IsSimulation { get; set; } = false;
        public virtual bool IsOnClickBigImage { get; set; } = false;
        public virtual bool IsMultiPaged { get; set; } = false;

        public abstract List<Person> GetListOfPerson(HtmlAgilityPack.HtmlDocument doc);
        public virtual string GetNextPageUrl() => string.Empty;
    }
}