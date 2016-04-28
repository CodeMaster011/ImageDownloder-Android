using System;
using System.Collections.Generic;
using static ImageDownloder.MyGlobal;
using System.Threading;

namespace ImageDownloder
{
    interface IOfflineModule
    {
        void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler);
        void CancleRequest(RequestPacket requestPacket);
    }

    class OfflineModule : IOfflineModule, IResponseHandler
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();   //TODO: Replace request queue with stack for better user experience
        private Queue<RequestPacket> pendingResponse = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void RequestProcessingError(RequestPacket requestPacket)
        {
            throw new NotImplementedException();
        }

        public void CancleRequest(RequestPacket requestPacket)
        {
            cancleRequest.Enqueue(requestPacket);
        }

        public void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler)
        {
            requestPacket.OfflineModuleResponse = responseHandler;
            pendingRequest.Enqueue(requestPacket);
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }

        private void processData()
        {
            while (IsRunning)
            {
                if (cancleRequest.Count > 0)
                {
                    onlineModule.CancleRequest(cancleRequest.Dequeue());
                }

                if (pendingRequest.Count > 0)
                {
                    var reqPacket = pendingRequest.Dequeue();
                    var reqURL = reqPacket.Url;

                    switch (reqPacket.RequestType)
                    {
                        case RequestPacketRequestTypes.Unknown:
                            break;
                        case RequestPacketRequestTypes.Str:
                            var reader = reqPacket.WebpageReader;
                            if (reader.IsPrefereOffline)
                            {
                                var resultStr = diskCache.GetString(reqURL);
                                if (resultStr == string.Empty)
                                {
                                    //no data in disk download now
                                    onlineModule.RequestData(reqPacket, this);
                                }
                                else
                                {
                                    //data exist in cache
                                    reqPacket.DataInString = resultStr;   //add result
                                    pendingResponse.Enqueue(reqPacket); //add to response queue
                                }
                            }
                            else
                            {
                                onlineModule.RequestData(reqPacket, this);
                            }
                            break;
                        case RequestPacketRequestTypes.Img:
                            var resultBmp = diskCache.GetBitmap(reqURL);
                            if (resultBmp == null) { onlineModule.RequestData(reqPacket, this); }   //no data in disk download now
                            else
                            {
                                //data exist in cache
                                reqPacket.DataInBitmap = resultBmp;   //add result
                                pendingResponse.Enqueue(reqPacket); //add to response queue
                            }
                            break;
                        default:
                            break;
                    }
                    
                }

                if (pendingResponse.Count > 0)
                {
                    var responsePacket = pendingResponse.Dequeue();

                    var responseHandler = responsePacket.OfflineModuleResponse;

                    if (responsePacket.requestObjs.ContainsKey(RequestPacket.RequestPacketOnlineModuleResponse))  
                    {
                        //the response package is coming from IOnlineModule

                        var packUrl = responsePacket.Url;

                        switch (responsePacket.RequestType)
                        {
                            case RequestPacketRequestTypes.Unknown:
                                break;
                            case RequestPacketRequestTypes.Str:
                                var reader = responsePacket.WebpageReader;
                                if (reader.IsPrefereOffline)
                                {
                                    if (diskCache.IsKeyExist(packUrl))
                                    {
                                        diskCache.Put(packUrl, responsePacket.DataInString, true);
                                    }
                                    else
                                    {
                                        diskCache.Put(packUrl, responsePacket.DataInString, true);
                                        responseHandler.RequestProcessedCallback(responsePacket); //make callback
                                    }
                                }
                                else
                                {
                                    responseHandler.RequestProcessedCallback(responsePacket); //make callback
                                }
                                break;
                            case RequestPacketRequestTypes.Img:
                                if (diskCache.IsKeyExist(packUrl))
                                    diskCache.Put(packUrl, responsePacket.DataInBitmap, true);
                                else
                                {
                                    diskCache.Put(packUrl, responsePacket.DataInBitmap, true);
                                    responseHandler.RequestProcessedCallback(responsePacket); //make callback
                                }
                                    
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        //data has been retrieved from cache
                        responseHandler.RequestProcessedCallback(responsePacket); //make callback
                    }
                }
                Thread.Sleep(1);
            }
        }
        public OfflineModule()
        {
            Thread th = new Thread(processData);
            th.Name = "Offline Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Normal;
            th.Start();
        }
    }
}