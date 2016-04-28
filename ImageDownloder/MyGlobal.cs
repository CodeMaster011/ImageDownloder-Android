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
        public static Context webBrowserContext = null;
        public static Action<int> NotifyDataUpdate = null;

        public static IWebPageReader currentWebPage = null;
        public static int currenItemPosition = -1;
        public static Stack<HistoryObject> history = new Stack<HistoryObject>();

        public static IOnlineModule onlineModule = new OnlineModule();
        public static IOfflineModule offlineModule = new OfflineModule();
        public static IAnalysisModule analysisModule = new AnalysisModule();
        public static ICache diskCache = new DiskCache(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/ImageDownloder/Cache", 0);

        public static bool IsRunning = true;

        public static string UidGenerator() => Guid.NewGuid().ToString();

        public static IWebPageReader MoveToWebpage(IWebPageReader webpage, int currenItemPosition = 0)
        {
            if(currentWebPage!=null) history.Push(new HistoryObject(currentWebPage,currenItemPosition));
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
                his.Dispose();
                return currentWebPage;
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public const int DefaultPic = Resource.Mipmap.Icon;

        public const string RequestPacketUid = "uid";
        public const string RequestPacketUrl = "url";
        public const string RequestPacketRequestType = "reqType";
        public const string RequestPacketWebpageReader = "reader";
        public const string RequestPacketAnalisisModuleResponse = "aResponse";
        public const string RequestPacketAnalisisModuleResponseInner = "aResponseInner";
        public const string RequestPacketAnalisisModuleResponseAction = "aResponseAction";
        public const string RequestPacketOfflineModuleResponse = "offResponse";
        public const string RequestPacketOnlineModuleResponse = "oResponse";
        public const string RequestPacketData = "data";
        public const string RequestPacketError = "error";
        public const string RequestPacketOwner = "owner";

        public enum RequestPacketRequestTypes
        {
            Unknown = 0,
            Str = 1,
            Img = 2
        }
        public enum RequestPacketOwners
        {
            UI,
            AnalysisModule,
            OfflineModule,
            OnlineModule
        }
        
    }
    public enum PreferedViewing
    {
        List,
        Grid
    }
}