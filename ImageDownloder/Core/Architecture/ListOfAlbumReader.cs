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
    internal class Album
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NoOfImages { get; set; } = -1;
        public object InformationNeedForNextLevel { get; set; } = null;
    }

    internal abstract class ListOfAlbumReader : IWebPageMetaData
    {
        public string Url { get; set; } = string.Empty;
        public virtual PreferedViewing Viewing { get; set; } = PreferedViewing.List;
        public virtual bool IsPrefereOffline { get; set; } = true;
        public virtual bool IsDownloadRequired { get; set; } = true;
        public virtual bool IsSimulation { get; set; } = false;
        public virtual bool IsOnClickBigImage { get; set; } = false;
        public virtual bool IsMultiPaged { get; set; } = false;

        public abstract string GetUrl(Person person);
        public abstract List<Album> GetAlbumList(Person person, HtmlAgilityPack.HtmlDocument doc);
        public virtual string GetNextPageUrl() => string.Empty;
    }
}