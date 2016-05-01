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
    internal abstract class WebsiteArchitecture
    {
        public ListOfPersonReader ListOfPersonReader { get; set; } = null;
        public ListOfAlbumReader ListOfAlbumReader { get; set; } = null;
        public ListOfImagesReader ListOfImagesReader { get; set; } = null;

        public WebsiteArchitecture(ListOfPersonReader ListOfPersonReader, ListOfAlbumReader ListOfAlbumReader, ListOfImagesReader ListOfImagesReader)
        {
            this.ListOfPersonReader = ListOfPersonReader;
            this.ListOfAlbumReader = ListOfAlbumReader;
            this.ListOfImagesReader = ListOfImagesReader;
        }
    }
    internal interface IWebPageMetaData
    {
        string Url { get; set; }
        PreferedViewing Viewing { get; set; }
        bool IsPrefereOffline { get; set; }
        bool IsDownloadRequired { get; set; }
        bool IsSimulation { get; set; }
        bool IsOnClickBigImage { get; set; }
        bool IsMultiPaged { get; set; }
    }
}