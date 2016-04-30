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
            packet.DataInStringList = UId;

            cancleRequest.Enqueue(packet);
        }
        public void RequestStringData(string uid, IWebPageReader webpageReader, IUiResponseHandler responseHandler)
        {
            pendingRequest.Enqueue(
                RequestPacket.CreateStringPacket(uid, webpageReader, RequestPacketOwners.AnalysisModule, responseHandler));
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }

        private void initialResponse(RequestPacket packet)
        {
            var responseHandler = packet.AnalisisModuleResponseUI;
            responseHandler.RequestProcessedCallback(
                packet.Uid,
                packet.Url,
                new WebPageData[] { WebPageData.GetFakeData() });
        }
        private void simulatedResponse(RequestPacket packet)
        {
            var extractedData = packet.WebpageReader.ExtractData(null);

            var responseHandler = packet.AnalisisModuleResponseUI;
            responseHandler.RequestProcessedCallback(
                packet.Uid,
                packet.Url,
                extractedData);
        }
        private void originalResponse(RequestPacket packet)
        {
            var data = packet.DataInString;
            var reader = packet.WebpageReader;
            var responseHandler = packet.AnalisisModuleResponseUI;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);
            
            //if (reader.IsFragmentSubmissiable)
            //{
            //    reader.FragmentSubmissionCallback = new FragmentSubmission((
            //        (WebPageData[] fData) => {
            //            responseHandler.RequestProcessedCallback(
            //            packet.Uid,
            //            packet.Url,
            //            fData);
            //            Thread.Sleep(10);
            //        }));
            //}

            var extractedData = reader.ExtractData(doc);
            
            responseHandler.RequestProcessedCallback(
                packet.Uid,
                packet.Url,
                extractedData);

            packet.Dispose();
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

                    var objType = reqObj.RequestType;
                    switch (objType)
                    {
                        case RequestPacketRequestTypes.Unknown:
                            break;
                        case RequestPacketRequestTypes.Str:
                            if (!reqObj.WebpageReader.IsSimulation)
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
                            //offlineModule.RequestData(reqObj, this);    //TODO: Remove and make a direct attachment with ImageProvider
                            break;
                        default:
                            break;
                    }
                }
                if (pendingResponse.Count > 0)
                {
                    var responsePacket = pendingResponse.Dequeue();
                    originalResponse(responsePacket);
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