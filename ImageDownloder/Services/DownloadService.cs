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
using Android.Util;
using System.IO;
using Java.Net;

namespace ImageDownloder.Services
{
    [Service]
    class DownloadService : Service
    {
        private IOnDownloadProcess listener = null;
        private bool isForcedStopDownload = false;
        public bool IsPauseDownload = false;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Log.Debug("DownloadService", "Starting...");

            MyGlobal.downloadService = this;
            MyGlobal.isDownloadServiceRunning = true;
            RegisterListener(MyGlobal.downloadListener);

            new Thread(doWork).Start();

            return StartCommandResult.Sticky;
        }

        public void StopDownloadNow() => isForcedStopDownload = true;
        public void RegisterListener(IOnDownloadProcess listener) => this.listener = listener;
        public void UnRegisterListener(IOnDownloadProcess listener) => this.listener = null;
        public void PauseDownload() => IsPauseDownload = true;
        public void ResumeDownload() => IsPauseDownload = false;
        public void ForcedStop() => isForcedStopDownload = true;

        private void doWork()
        {
            int index = 0;
            try
            {
                while (!isForcedStopDownload && MyGlobal.downloadServiceData?.Count > index)
                {
                    RegisterListener(MyGlobal.downloadListener);

                    Log.Debug("DownloadService", "Working...");

                    ImageDownloadInformation data = null;

                    lock (MyGlobal.downloadServiceData)
                    {
                        data = MyGlobal.downloadServiceData[index++];
                    }

                    if (data != null) downloadFile(data);

                    do
                    {
                        Thread.Sleep(10);
                    } while (IsPauseDownload);

                    if (isForcedStopDownload)
                    {
                        try
                        {
                            while (MyGlobal.downloadServiceData.Count > index)
                            {
                                data = MyGlobal.downloadServiceData[index++];
                                data.isFailed = true;
                            }
                        }
                        catch (Exception) { }
                        break;
                    }
                }
            }
            catch (Exception) { }
            
            workFinished();
        }         

        private bool downloadFile(ImageDownloadInformation file)
        {
            listener?.OnNewFileDownloadStarted(file);   //new file download started
            file.isDownloading = true;

            bool downloaded = true;
            Stream input = null;
            FileStream output = null;
            HttpURLConnection connection = null;

            int skeep = 10;
            int callCount = 0;
            try
            {
                URL url = new URL(file.src.original);
                connection = (HttpURLConnection)url.OpenConnection();
                connection.Connect();

                // expect HTTP 200 OK, so we don't mistakenly save error report
                // instead of the file
                if (connection.ResponseCode != HttpStatus.Ok)                
                    throw new Exception("Server returned HTTP " + connection.ResponseCode + " " + connection.ResponseMessage);
                
                int fileTotalLength = connection.ContentLength;
                file.totalSize = fileTotalLength;

                input = connection.InputStream;
                output = new FileStream(file.des, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

                byte[] data = new byte[4096];
                int current = 0;
                int oSize = 1;
                while (oSize > 0)
                {
                    // allow canceling
                    if (isForcedStopDownload)
                    {
                        output.Close();
                        File.Delete(file.des);  //delete the current not completed file
                        downloaded = false;
                        break;
                    }

                    oSize = input.Read(data, 0, data.Length - 1);   //read data from web

                    current += oSize;

                    file.currentSize = current;

                    
                    // publishing the progress....
                    if (fileTotalLength > 0 && callCount % skeep == 0) // only if total length is known
                        listener?.OnFileDownloadProgress(file);

                    callCount++;

                    output.Write(data, 0, oSize);

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (output != null) output.Close();
                }
                catch (Exception) { }
                file.isDownloading = false;
                file.isFailed = true;
                listener.OnFileDownloadError(file, ex.Message);
                downloaded = false;
            }
            finally
            {
                try
                {
                    if (input != null) input.Close();
                    if (output != null) output.Close();
                }
                catch (Exception) { }
                if (connection != null) connection.Disconnect();
            }

            if (downloaded)
            {
                file.isFinished = true;
                file.isDownloading = false;
                listener?.OnFileDownloadSuccessful(file);
            }

            return downloaded;
        }

        private void workFinished()
        {
            MyGlobal.isDownloadServiceRunning = false;
            isForcedStopDownload = false;
            IsPauseDownload = false;
            MyGlobal.downloadService = null;
            Log.Debug("DownloadService", "Finishing...");
            StopSelf();
        }

        public interface IOnDownloadProcess
        {
            void OnNewFileDownloadStarted(ImageDownloadInformation file);
            void OnFileDownloadProgress(ImageDownloadInformation file);
            void OnFileDownloadSuccessful(ImageDownloadInformation file);
            void OnFileDownloadError(ImageDownloadInformation file, string error);
        }
    }
}