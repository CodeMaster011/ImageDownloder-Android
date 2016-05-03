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

namespace ImageDownloder.Core.Architecture
{
    internal sealed class WebsiteHandler : IWebsiteReader ,IWebPageReader, IBigImageCollectionHolder
    {
        private WebsiteArchitecture architecture = null;
        private object informationForLevel = null;
        private CurrentStateInfo currentState = CurrentStateInfo.ListOfPerson;
        private string url;
        private PreferedViewing viewing;
        private bool isPrefereOffline;
        private bool isDownloadRequired;
        private bool isSimulation;
        private bool isOnClickBigImage;
        private bool isMultiPaged;

        public string Url { get { refreshConfig();  return url; } set { url = value; } }
        public PreferedViewing Viewing { get { refreshConfig(); return viewing; } set { viewing = value; } }
        public bool IsPrefereOffline { get { refreshConfig(); return isPrefereOffline; }set { isPrefereOffline = value; } }
        public bool IsDownloadRequired { get { refreshConfig(); return isDownloadRequired; } set { isDownloadRequired = value; } }
        public bool IsSimulation { get { refreshConfig(); return isSimulation; } set { isSimulation = value; } }
        public bool IsOnClickBigImage { get { refreshConfig(); return isOnClickBigImage; } set { isOnClickBigImage = value; }  }
        public bool IsMultiPaged { get { refreshConfig(); return isMultiPaged; } set { isMultiPaged = value; } }
        public List<ImageDefinition> AlbumImages { get; set; } = new List<ImageDefinition>();

        public IWebPageReader HomePageReader { get { return null; } }

        public IWebPageReader IndexPageReader { get { return Start(); } }

        public string Name { get { return architecture.Name; } }
        public string ComicText { get { return architecture.ComicText; } }

        public WebsiteHandler(WebsiteArchitecture architecture)
        {
            this.architecture = architecture;
        }

        private void arrangeCurrentState()
        {
            if (architecture.ListOfAlbumReader == null && MyGlobal.currentState == 2) currentState = CurrentStateInfo.ListOfImage;
            else currentState = (CurrentStateInfo)MyGlobal.currentState;
        }

        private void refreshConfig()
        {
            arrangeCurrentState();

            switch (currentState)
            {
                case CurrentStateInfo.Unknow:
                    break;
                case CurrentStateInfo.ListOfPerson:
                    LoadConfig(architecture.ListOfPersonReader);
                    break;
                case CurrentStateInfo.ListOfAlbum:
                    LoadConfig(architecture.ListOfAlbumReader);
                    break;
                case CurrentStateInfo.ListOfImage:
                    LoadConfig(architecture.ListOfImagesReader);
                    break;
                case CurrentStateInfo.ShowImage:
                    break;
                default:
                    break;
            }
        }
        private void LoadConfig(IWebPageMetaData reader)
        {
            url = reader.Url;
            viewing = reader.Viewing;
            isPrefereOffline = reader.IsPrefereOffline;
            isDownloadRequired = reader.IsDownloadRequired;
            isSimulation = reader.IsSimulation;
            isOnClickBigImage = reader.IsOnClickBigImage;
            isMultiPaged = reader.IsMultiPaged;
        }

        public IWebPageReader Start()
        {
            LoadConfig(architecture.ListOfPersonReader);
            return this;
        }

        public IWebPageReader GetNextPage()
        {
            arrangeCurrentState();

            switch (currentState)
            {
                case CurrentStateInfo.Unknow:
                    break;
                case CurrentStateInfo.ListOfPerson:
                    if (architecture.ListOfPersonReader.IsMultiPaged)
                    {
                        var u = architecture.ListOfPersonReader.GetNextPageUrl();
                        architecture.ListOfPersonReader.Url = u == string.Empty ? Url : u;
                        return u != string.Empty ? this : null;
                    }
                    break;
                case CurrentStateInfo.ListOfAlbum:
                    if (architecture.ListOfAlbumReader.IsMultiPaged)
                    {
                        var u = architecture.ListOfAlbumReader.GetNextPageUrl();
                        architecture.ListOfAlbumReader.Url = u == string.Empty ? Url : u;
                        return u != string.Empty ? this : null;
                    }
                    break;
                case CurrentStateInfo.ListOfImage:
                    if (architecture.ListOfImagesReader.IsMultiPaged)
                    {
                        var u = architecture.ListOfImagesReader.GetNextPageUrl();
                        architecture.ListOfImagesReader.Url = u == string.Empty ? Url : u;
                        return u != string.Empty ? this : null;
                    }
                    break;
                case CurrentStateInfo.ShowImage:
                    break;
                default:
                    break;
            }
            return null;
        }

        public IWebPageReader OnClickCallback(WebPageData item)
        {
            switch (currentState)
            {
                case CurrentStateInfo.Unknow:
                    break;
                case CurrentStateInfo.ListOfPerson:
                    AlbumImages.Clear();
                    if (architecture.ListOfAlbumReader == null) //one level skipped
                    {
                        currentState = CurrentStateInfo.ListOfAlbum;
                        return OnClickCallback(item);
                    }
                    architecture.ListOfAlbumReader.Url = architecture.ListOfAlbumReader.GetUrl((Person)item.Tag);
                    LoadConfig(architecture.ListOfAlbumReader);
                    informationForLevel = item.Tag;
                    return this;
                case CurrentStateInfo.ListOfAlbum:
                    AlbumImages.Clear();
                    if (architecture.ListOfAlbumReader == null)
                    {
                        try
                        {
                            item.Tag = Changer.ToAlbum((Person)item.Tag);
                        }
                        catch (Exception) { }                        
                    }

                    architecture.ListOfImagesReader.Url = architecture.ListOfImagesReader.GetUrl((Album)item.Tag);
                    LoadConfig(architecture.ListOfImagesReader);
                    informationForLevel = item.Tag;
                    return this;
                case CurrentStateInfo.ListOfImage:
                    break;
                case CurrentStateInfo.ShowImage:
                    break;
                default:
                    break;
            }
            return null;
        }

        public WebPageData[] ExtractData(HtmlDocument doc)
        {
            arrangeCurrentState();

            List<WebPageData> data = new List<WebPageData>();
            switch (currentState)
            {
                case CurrentStateInfo.Unknow:
                    break;
                case CurrentStateInfo.ListOfPerson:
                    AlbumImages.Clear();
                    var persons = architecture.ListOfPersonReader.GetListOfPerson(doc);
                    foreach (var person in persons)
                    {
                        WebPageData singleData = new WebPageData();
                        singleData.IsFinal = true;
                        singleData.ImageUrl = person.ImageUrl;
                        singleData.mainText = person.Name;
                        singleData.subText = (person.Description == string.Empty ? "" : person.Description);
                        singleData.NoOfItemsIncluded = person.NoOfAlbums;
                        singleData.UID = MyGlobal.UidGenerator();
                        singleData.Tag = person;

                        data.Add(singleData);
                    }
                    break;
                case CurrentStateInfo.ListOfAlbum:

                    if (architecture.ListOfAlbumReader == null)
                    {
                        currentState = CurrentStateInfo.ListOfPerson;
                        return ExtractData(doc);
                    }

                    AlbumImages.Clear();
                    var albums = architecture.ListOfAlbumReader.GetAlbumList((Person)informationForLevel, doc);
                    foreach (var album in albums)
                    {
                        WebPageData singleData = new WebPageData();
                        singleData.IsFinal = true;
                        singleData.ImageUrl = album.ImageUrl;
                        singleData.mainText = album.Name;

                        singleData.subText = (album.Description == string.Empty ? "" : album.Description);
                        singleData.NoOfItemsIncluded = album.NoOfImages;
                        singleData.UID = MyGlobal.UidGenerator();
                        singleData.Tag = album;

                        data.Add(singleData);
                    }
                    break;
                case CurrentStateInfo.ListOfImage:
                    var images = architecture.ListOfImagesReader.GetImageList((Album)informationForLevel, doc);
                    foreach (var img in images)
                    {
                        WebPageData singleData = new WebPageData();
                        singleData.IsFinal = true;
                        singleData.ImageUrl = img.Image.thumbnil;
                        singleData.mainText = img.Name;
                        singleData.UID = MyGlobal.UidGenerator();

                        data.Add(singleData);
                        AlbumImages.Add(img.Image); //get the data for the next level and cached it
                    }
                    break;
                case CurrentStateInfo.ShowImage:
                    break;
                default:
                    break;
            }
            return data.Count == 0 ? null : data.ToArray();
        }

        private static class Changer
        {
            public static Album ToAlbum(Person person)
            {
                var a = new Album();
                a.Description = person.Description;
                a.ImageUrl = person.ImageUrl;
                a.InformationNeedForNextLevel = person.InformationNeedForNextLevel;
                a.Name = person.Name;
                a.NoOfImages = person.NoOfAlbums;

                return a;
            }
        }
    }
    internal enum CurrentStateInfo
    {
        Unknow,
        ListOfPerson,
        ListOfAlbum,
        ListOfImage,
        ShowImage
    }
}