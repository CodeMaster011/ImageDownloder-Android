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
using System.Threading;
using static ImageDownloder.MyGlobal;
using Android.Util;

namespace ImageDownloder
{
    interface IOnlineModule
    {
        void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler);
        void CancleRequest(RequestPacket requestPacket);
    }
    class OnlineModule : IOnlineModule
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void CancleRequest(RequestPacket requestPacket)
        {
            cancleRequest.Enqueue(requestPacket);
        }

        public void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler)
        {
            string uid = requestPacket.Uid;

            requestPacket.OnlineModuleResponse = responseHandler;

            pendingRequest.Enqueue(requestPacket);         
        }        

        private void processRequest()
        {
            while (IsRunning)
            {
                try
                {
                    //===================REQUEST CANCELING==================================
                    if (cancleRequest.Count > 0)
                    {
                        var cancleReqPacket = cancleRequest.Dequeue();

                        var cUids = cancleReqPacket.DataInStringList;
                        if (cUids != null)
                        {
                            Queue<RequestPacket> tempRequest = new Queue<RequestPacket>();
                            for (int i = 0; i < pendingRequest.Count; i++)
                            {
                                var tPacket = pendingRequest.Dequeue();
                                var tUid = tPacket.Uid;

                                if (cUids.Contains(tUid)) cUids.Remove(tUid);
                                else tempRequest.Enqueue(tPacket);
                            }
                            pendingRequest = tempRequest;
                        }
                    }
                    //===================REQUEST PROCESSING==================================
                    var packet = pendingRequest.Dequeue();

                    var requestedUrl = packet.Url;
                    var responseHandler = packet.OnlineModuleResponse;
                    var packType = packet.RequestType;
                    try
                    {
                        switch (packType)
                        {
                            case RequestPacketRequestTypes.Unknown:
                                break;
                            case RequestPacketRequestTypes.Str:
                                Log.Debug("Online Module:", $"Downloading (string) url {requestedUrl}");

                                string result = Helper.DownloadFile(requestedUrl);
                                packet.DataInString = result;

                                Log.Debug("Online Module:", $"Making processed callback for url {requestedUrl}");
                                responseHandler.RequestProcessedCallback(packet);
                                break;
                            case RequestPacketRequestTypes.Img:
                                //Log.Debug("Online Module:", $"Downloading (image) url {requestedUrl}");

                                //var stream = Helper.DownloadFileInMemory(requestedUrl);
                                //stream.Seek(0, System.IO.SeekOrigin.Begin);
                                //var bitmap =  Android.Graphics.BitmapFactory.DecodeStream(stream);
                                //stream.Close();

                                //packet.DataInBitmap =  bitmap;

                                //Log.Debug("Online Module:", $"Making processed callback for url {requestedUrl}");
                                //responseHandler.RequestProcessedCallback(packet);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        packet.Error = ex.Message;
                        responseHandler.RequestProcessingError(packet);
                    }
                }
                catch (Exception) { }
                Thread.Sleep(1);
            }
        }

        public OnlineModule()
        {
            Thread th = new Thread(processRequest);
            th.Name = "Online Request Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Highest;
            th.Start();
        }
        
    }
}