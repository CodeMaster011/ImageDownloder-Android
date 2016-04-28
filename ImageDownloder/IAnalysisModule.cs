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
using static ImageDownloder.MyGlobal;
using System.Threading;

namespace ImageDownloder
{
    interface IAnalysisModule
    {
        void RequestStringData(string uid, IWebPageReader webpageReader, IUiResponseHandler responseHandler);
        void RequestImageData(string uid, string url, IResponseHandler responseHandler);
        void RequestImageData(string uid, string url, ImageResponseAction responseHandler);

        void CancleRequest(WebPageData[] webPagedata);
        void CancleRequest(List<string> UId);
    }
    class AnalysisModule : IAnalysisModule, IResponseHandler
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();
        private Queue<RequestPacket> pendingResponse = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void RequestProcessingError(RequestPacket requestPacket)
        {
            throw new NotImplementedException();
        }

        public void CancleRequest(WebPageData[] webPagedata)
        {
            try
            {
                if (webPagedata[0].UID == string.Empty) return;

                List<string> UId = new List<string>();
                foreach (var item in webPagedata)
                {
                    UId.Add(item.UID);
                }

                CancleRequest(UId);
            }
            catch (Exception) { }
        }
        public void CancleRequest(List<string> UId)
        {
            RequestPacket packet = new RequestPacket();
            packet.Add(RequestPacketData, UId);

            cancleRequest.Enqueue(packet);
        }

        public void RequestImageData(string uid, string url, ImageResponseAction responseHandler)  //no reader
        {
            RequestPacket packet = new RequestPacket();
            packet.Add(RequestPacketUid, uid);
            packet.Add(RequestPacketUrl, url);
            packet.Add(RequestPacketRequestType, RequestPacketRequestTypes.Img);    //image packet
            packet.Add(RequestPacketAnalisisModuleResponseAction, responseHandler);
            packet.Add(RequestPacketOwner, RequestPacketOwners.AnalysisModule);

            pendingRequest.Enqueue(packet);
        }

        public void RequestImageData(string uid, string url, IResponseHandler responseHandler)  //no reader
        {
            RequestPacket packet = new RequestPacket();
            packet.Add(RequestPacketUid, uid);
            packet.Add(RequestPacketUrl, url);
            packet.Add(RequestPacketRequestType, RequestPacketRequestTypes.Img);    //image packet
            packet.Add(RequestPacketAnalisisModuleResponseInner, responseHandler);
            packet.Add(RequestPacketOwner, RequestPacketOwners.AnalysisModule);

            pendingRequest.Enqueue(packet);
        }

        public void RequestStringData(string uid, IWebPageReader webpageReader, IUiResponseHandler responseHandler)
        {
            RequestPacket packet = new RequestPacket();
            packet.Add(RequestPacketUid, uid);
            packet.Add(RequestPacketUrl, webpageReader.Url);
            packet.Add(RequestPacketRequestType, RequestPacketRequestTypes.Str);//string packet
            packet.Add(RequestPacketWebpageReader, webpageReader);
            packet.Add(RequestPacketAnalisisModuleResponse, responseHandler);
            packet.Add(RequestPacketOwner, RequestPacketOwners.AnalysisModule);

            pendingRequest.Enqueue(packet);
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }

        private void initialResponse(RequestPacket packet, bool isPicture = false)
        {
            var responseHandler = packet.Get<IUiResponseHandler>(RequestPacketAnalisisModuleResponse);
            responseHandler.RequestProcessedCallback(
                packet.Get<string>(RequestPacketUid),
                packet.Get<string>(RequestPacketUrl),
                new WebPageData[] { WebPageData.GetFakeData() });
        }
        private void simulatedResponse(RequestPacket packet, bool isPicture = false)
        {
            var extractedData = packet.Get<IWebPageReader>(RequestPacketWebpageReader).ExtractData(null);

            var responseHandler = packet.Get<IUiResponseHandler>(RequestPacketAnalisisModuleResponse);
            responseHandler.RequestProcessedCallback(
                packet.Get<string>(RequestPacketUid),
                packet.Get<string>(RequestPacketUrl),
                extractedData);
        }
        private void originalResponse(RequestPacket packet, bool isPicture = false)
        {
            if (isPicture)
            {
                if (packet.requestObjs.ContainsKey(RequestPacketAnalisisModuleResponseAction))
                {
                    var responseHandlerAction = packet.Get<ImageResponseAction>(RequestPacketAnalisisModuleResponseAction);
                    responseHandlerAction(
                        packet.Get<string>(RequestPacketUid),
                        packet.Get<string>(RequestPacketUrl),
                        packet.Get<Android.Graphics.Bitmap>(RequestPacketData));
                }

                if (packet.requestObjs.ContainsKey(RequestPacketAnalisisModuleResponseInner))
                {
                    var responseHandlerInner = packet.Get<IResponseHandler>(RequestPacketAnalisisModuleResponseInner);

                }
                return;
            }

            var data = packet.Get<string>(RequestPacketData);
            var reader = packet.Get<IWebPageReader>(RequestPacketWebpageReader);
            var responseHandler = packet.Get<IUiResponseHandler>(RequestPacketAnalisisModuleResponse);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);
            
            if (reader.IsFragmentSubmissiable)
            {
                reader.FragmentSubmissionCallback = new FragmentSubmission((
                    (WebPageData[] fData) => {
                        responseHandler.RequestProcessedCallback(
                        packet.Get<string>(RequestPacketUid),
                        packet.Get<string>(RequestPacketUrl),
                        fData);
                        Thread.Sleep(10);
                    }));
            }

            var extractedData = reader.ExtractData(doc);
            
            responseHandler.RequestProcessedCallback(
                packet.Get<string>(RequestPacketUid),
                packet.Get<string>(RequestPacketUrl),
                extractedData);
        }

        private void processData()
        {
            while (IsRunning)
            {
                if (cancleRequest.Count > 0)
                {
                    offlineModule.CancleRequest(cancleRequest.Dequeue());
                }

                if (pendingRequest.Count > 0)
                {
                    var reqObj = pendingRequest.Dequeue();

                    var objType = reqObj.Get<RequestPacketRequestTypes>(RequestPacketRequestType);
                    switch (objType)
                    {
                        case RequestPacketRequestTypes.Unknown:
                            break;
                        case RequestPacketRequestTypes.Str:
                            if (!reqObj.Get<IWebPageReader>(RequestPacketWebpageReader).IsSimulation)
                            {
                                initialResponse(reqObj);
                                offlineModule.RequestData(reqObj, this);
                            }
                            else
                            {
                                simulatedResponse(reqObj);
                                reqObj.Dispose();
                            }
                            break;
                        case RequestPacketRequestTypes.Img:
                            offlineModule.RequestData(reqObj, this);    //request for img
                            break;
                        default:
                            break;
                    }
                }
                if (pendingResponse.Count > 0)
                {
                    var responsePacket = pendingResponse.Dequeue();
                    originalResponse(responsePacket, responsePacket.Get<RequestPacketRequestTypes>(RequestPacketRequestType) == RequestPacketRequestTypes.Img ? true : false);
                    responsePacket.Dispose();
                }
                Thread.Sleep(1);
            }
        }

        public AnalysisModule()
        {
            Thread th = new Thread(processData);
            th.Name = "Analysis Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Highest;
            th.Start();
        }
    }
}