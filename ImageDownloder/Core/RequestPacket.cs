using System;
using System.Collections.Generic;

namespace ImageDownloder
{    
    internal class RequestPacket : IDisposable, ICloneable
    {
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

        public Dictionary<string, object> requestObjs = null;

        public RequestPacket()
        {
            requestObjs = new Dictionary<string, object>();
        }

        public RequestPacket Add(string key, object value)
        {
            requestObjs.Add(key, value);
            return this;
        }

        public string Uid
        {
            get
            {
                return Get<string>(RequestPacketUid);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketUid))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketUid, value);
            }
        }

        public string Url
        {
            get
            {
                return Get<string>(RequestPacketUrl);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketUrl))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketUrl, value);
            }
        }

        public RequestPacketRequestTypes RequestType
        {
            get
            {
                return Get<RequestPacketRequestTypes>(RequestPacketRequestType);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketRequestType))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketRequestType, value);
            }
        }

        public IWebPageReader WebpageReader
        {
            get
            {
                return Get<IWebPageReader>(RequestPacketWebpageReader);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketWebpageReader))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketWebpageReader, value);
            }
        }

        public IResponseHandler AnalisisModuleResponse
        {
            get
            {
                return Get<IResponseHandler>(RequestPacketAnalisisModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketAnalisisModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketAnalisisModuleResponse, value);
            }
        }

        public IUiResponseHandler AnalisisModuleResponseUI
        {
            get
            {
                return Get<IUiResponseHandler>(RequestPacketAnalisisModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketAnalisisModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketAnalisisModuleResponse, value);
            }
        }

        public IResponseHandler OfflineModuleResponse
        {
            get
            {
                return Get<IResponseHandler>(RequestPacketOfflineModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOfflineModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOfflineModuleResponse, value);
            }
        }

        public IResponseHandler OnlineModuleResponse
        {
            get
            {
                return Get<IResponseHandler>(RequestPacketOnlineModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOnlineModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOnlineModuleResponse, value);
            }
        }

        public string DataInString
        {
            get
            {
                return Get<string>(RequestPacketData);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketData))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketData, value);
            }
        }

        public Android.Graphics.Bitmap DataInBitmap
        {
            get
            {
                return Get<Android.Graphics.Bitmap>(RequestPacketData);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketData))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketData, value);
            }
        }

        public List<string> DataInStringList
        {
            get
            {
                return Get<List<string>>(RequestPacketData);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketData))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketData, value);
            }
        }

        public string Error
        {
            get
            {
                return Get<string>(RequestPacketError);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketError))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketError, value);
            }
        }

        public RequestPacketOwners Owner
        {
            get
            {
                return Get<RequestPacketOwners>(RequestPacketOwner);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOwner))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOwner, value);
            }
        }

        public T Get<T>(string key) => Get<T>(key, false);

        private T Get<T>(string key, bool isConfirm)
        {
            if (!isConfirm)
                if (!requestObjs.ContainsKey(key))
                    return default(T);
            return (T)requestObjs[key];
        }

        public void Dispose()
        {
            try
            {
                if(RequestType == RequestPacketRequestTypes.Img)
                    DataInBitmap?.Dispose();
            }
            catch (Exception) { }

            if (requestObjs != null)
            {
                requestObjs.Clear();
            }
            requestObjs = null;
        }

        public object Clone()
        {
            RequestPacket n = null;
            lock (requestObjs)
            {
                n = new RequestPacket();
                foreach (var item in requestObjs)
                {
                    n.Add(item.Key, item.Value);
                }
                n.requestObjs.Remove(RequestPacketUid);
                n.Uid = MyGlobal.UidGenerator();
            }            
            return n;
        }

        ~RequestPacket()
        {
            Dispose();
            
            Android.Util.Log.Debug("RequestPacket", $"GC Collected {--MyGlobal.requestPacketCount}");
        }

        public static RequestPacket CreateStringPacket(string uid, IWebPageReader reader, RequestPacketOwners owner,
            IResponseHandler analisisModuleResponse = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
         => CreateStringPacket(uid, reader.Url, reader, owner, analisisModuleResponse, offlineModuleResponse, onlineModuleResponse);

        public static RequestPacket CreateStringPacket(string uid, IWebPageReader reader, RequestPacketOwners owner,
            IUiResponseHandler analisisModuleResponseUI = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
         => CreateStringPacket(uid, reader.Url, reader, owner, analisisModuleResponseUI, offlineModuleResponse, onlineModuleResponse);

        public static RequestPacket CreateStringPacket(string uid, string url, IWebPageReader reader, RequestPacketOwners owner,
            IUiResponseHandler analisisModuleResponseUI = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        {
            var r = new RequestPacket() { RequestType = RequestPacketRequestTypes.Str, Uid = uid, Url = url, WebpageReader = reader, Owner = owner };
            if (analisisModuleResponseUI != null) r.AnalisisModuleResponseUI = analisisModuleResponseUI;
            if (offlineModuleResponse != null) r.OfflineModuleResponse = offlineModuleResponse;
            if (onlineModuleResponse != null) r.OnlineModuleResponse = onlineModuleResponse;

            Android.Util.Log.Debug("RequestPacket", $"Created {++MyGlobal.requestPacketCount}");

            return r;
        }

        public static RequestPacket CreateStringPacket(string uid, string url, IWebPageReader reader, RequestPacketOwners owner,
            IResponseHandler analisisModuleResponse = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        {
            var r = new RequestPacket() { RequestType = RequestPacketRequestTypes.Str, Uid = uid, Url = url, WebpageReader = reader, Owner = owner };
            if (analisisModuleResponse != null) r.AnalisisModuleResponse = analisisModuleResponse;
            if (offlineModuleResponse != null) r.OfflineModuleResponse = offlineModuleResponse;
            if (onlineModuleResponse != null) r.OnlineModuleResponse = onlineModuleResponse;

            Android.Util.Log.Debug("RequestPacket", $"Created {++MyGlobal.requestPacketCount}");

            return r;
        }

        public static RequestPacket CreateImagePacket(string uid,string url, RequestPacketOwners owner,
            IResponseHandler analisisModuleResponse = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        {
            var r = new RequestPacket() { RequestType = RequestPacketRequestTypes.Img, Uid = uid, Url = url, Owner = owner };
            if (analisisModuleResponse != null) r.AnalisisModuleResponse = analisisModuleResponse;
            if (offlineModuleResponse != null) r.OfflineModuleResponse = offlineModuleResponse;
            if (onlineModuleResponse != null) r.OnlineModuleResponse = onlineModuleResponse;

            Android.Util.Log.Debug("RequestPacket", $"Created {++MyGlobal.requestPacketCount}");

            return r;
        }

        public static RequestPacket CreateImagePacket(string url, RequestPacketOwners owner,
            IResponseHandler analisisModuleResponse = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        => CreateImagePacket(Guid.NewGuid().ToString(), url, owner, analisisModuleResponse, offlineModuleResponse, onlineModuleResponse);
    }

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
        OnlineModule,
        ImageProvider
    }
}