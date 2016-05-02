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

namespace ImageDownloder
{
    static class MyGlobal
    {
        public static int historyObjCount = 0;
        public static int requestPacketCount = 0;

        public static int currentState = 1;

        public static System.Drawing.Size screenSize = new System.Drawing.Size();

        public static Context webBrowserContext = null;

        public static float NextPageLoadingIndex = 0.5F;
        public static float NextPageLoadingIndexBig = 0.9F;
        public static IWebPageReader currentWebPage = null;
        public static int currenItemPosition = -1;
        public static WebPageData[] cachedData = null;
        public static string title = string.Empty;
        public static Stack<HistoryObject> history = new Stack<HistoryObject>();

        public static IOnlineModule onlineModule = new OnlineModule();
        public static IOfflineModule offlineModule = new OfflineModule();
        public static IAnalysisModule analysisModule = new AnalysisModule();
        public static ICache diskCache = new DiskCache(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/ImageDownloder/Cache", 0);

        public static MyPicasso.MyCache memoryCache = new MyPicasso.MyCache();

        public static bool IsRunning = true;

        public static string UidGenerator() => Guid.NewGuid().ToString();

        public static IWebPageReader MoveToWebpage(IWebPageReader webpage, WebPageData[] cachedData, string title, int currenItemPosition = 0)
        {
            if(currentWebPage!=null) history.Push(new HistoryObject(currentWebPage, cachedData, title, currenItemPosition));
            currentWebPage = webpage;
            return webpage;
        }

        public static IWebPageReader BackToPreviousWebpage()
        {
            try
            {
                var his = history.Pop();
                currentWebPage = his.webpageReader;
                currenItemPosition = his.clickedPosition;
                cachedData = his.cachedData;
                title = his.title;
                his.Dispose();
                return currentWebPage;
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public const int DefaultPic = Resource.Mipmap.Icon;//TODO: Set a new default image for viewing
        public const int UnkownImage = Resource.Mipmap.unknownfemale;

        public static Random random = new Random(20);
        public static string[] comicColor = new string[] { "#005043","#363636", "#79464e","#303e57","#4d3939","#193713","#c56395","#1d41b7","#5f12a7","#9e12a7","#a71265","#102ab0","#107bb0","#109053","#419010","#889010","#903010" };
        public static string GetRandomComicColor()
        {            
            return comicColor[random.Next(comicColor.Length - 1)];
        }
    }
    public enum PreferedViewing
    {
        List,
        Grid
    }
}