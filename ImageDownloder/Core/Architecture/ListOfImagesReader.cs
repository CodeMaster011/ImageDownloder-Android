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
    class ImageInfo
    {
        public string Name { get; set; } = string.Empty;
        public ImageDefinition Image { get; set; } = null;
    }

    internal abstract class ListOfImagesReader : IWebPageMetaData
    {
        public virtual string Url { get; set; } = string.Empty;
        public virtual PreferedViewing Viewing { get; set; } = PreferedViewing.Grid;
        public virtual bool IsPrefereOffline { get; set; } = true;
        public virtual bool IsDownloadRequired { get; set; } = true;
        public virtual bool IsSimulation { get; set; } = false;
        public virtual bool IsOnClickBigImage { get; set; } = true;
        public virtual bool IsMultiPaged { get; set; } = false;

        public abstract string GetUrl(Album album);
        public abstract List<ImageInfo> GetImageList(Album album, HtmlAgilityPack.HtmlDocument doc);
        public virtual string GetNextPageUrl() => string.Empty;
    }
}