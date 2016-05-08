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
using ImageDownloder.Services;
using System.IO;
using Squareup.Picasso;
using Android.Util;

namespace ImageDownloder.Activities
{
    [Activity(Label = "Download")]
    public class DownloadActivity : Activity, DownloadService.IOnDownloadProcess
    {
        private DownloadListAdapter adapter = null;
        private ListView listview = null;
        private Button pauseButton = null;

        private void notify()
        {
            adapter.NotifyDataSetChanged();
        }

        public void OnFileDownloadError(ImageDownloadInformation file, string error)
        {
            File.Delete(file.des);
            //adapter.data = downloadServiceData;
            RunOnUiThread(notify);
        }

        public void OnFileDownloadProgress(ImageDownloadInformation file)
        {
            //adapter.data = downloadServiceData;
            //Log.Debug("DownloadActivity", "OnFileDownloadProgress " + file.src.original);
            RunOnUiThread(notify);
        }

        public void OnFileDownloadSuccessful(ImageDownloadInformation file)
        {
            //adapter.data = downloadServiceData;
            RunOnUiThread(notify);
        }

        public void OnNewFileDownloadStarted(ImageDownloadInformation file)
        {
            //adapter.data = downloadServiceData; RunOnUiThread(notify); 
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.download);

            if (downloadService == null && downloadServiceData.Count > 0) StartActivity(new Intent(this, typeof(DownloadService)));
            downloadListener = this;

            listview = FindViewById<ListView>(Resource.Id.downloadListView);
            adapter = new DownloadListAdapter(this);
            adapter.data = downloadServiceData;
            listview.Adapter = adapter;

            pauseButton = FindViewById<Button>(Resource.Id.downloadPauseButton);
            pauseButton.Click += delegate
            {
                if (downloadService!=null && !downloadService.IsPauseDownload)
                {
                    pauseButton.Text = "Resume";
                    downloadService?.PauseDownload();
                }
                else
                {
                    pauseButton.Text = "Pause";
                    downloadService?.ResumeDownload();
                }
                
            };
            FindViewById<Button>(Resource.Id.downloadStopButton).Click += delegate
            {
                downloadService?.ForcedStop();
            };
        }

        public override void OnBackPressed()
        {
            downloadListener = null;
            adapter.data = null;

            base.OnBackPressed();
        }

        class DownloadListAdapter : BaseAdapter
        {
            public List<ImageDownloadInformation> data { get; set; } = null;

            public ListView liv { get; set; } = null;
            private Activity parent = null;

            public override int Count { get { return data == null ? 0 : data.Count; } }

            public override Java.Lang.Object GetItem(int position) => null;

            public override long GetItemId(int position) => 0;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    var layoutInflator = (LayoutInflater)webBrowserContext.GetSystemService(Service.LayoutInflaterService);
                    convertView = layoutInflator.Inflate(Resource.Layout.download_listview_single_item, parent, false);
                    VAdapterViewHolder vholder = new VAdapterViewHolder(convertView);
                    convertView.Tag = vholder;
                }
                VAdapterViewHolder vHolder = (VAdapterViewHolder)convertView.Tag;
                var data = this.data[position];
                vHolder.nameTextView.Text = data.Name;
                vHolder.websiteNameTextView.Text = data.websiteName;



                if (data.src.thumbnil != string.Empty)
                    Picasso.With(parent.Context).Load(data.src.thumbnil).Resize(128, 128).CenterInside().Into(vHolder.imageView);
                else
                    vHolder.imageView.SetImageResource(DefaultPic);

                if (data.isFailed)
                {
                    vHolder.statueTextView.Text = "Failed";
                    vHolder.statueTextView.SetTextColor(Android.Graphics.Color.ParseColor("#C62828"));
                }
                else
                {
                    if (data.isDownloading)
                    {
                        vHolder.statueTextView.Text = "Downloading...";
                        vHolder.statueTextView.SetTextColor(Android.Graphics.Color.ParseColor("#2E7D32"));
                    }
                    else
                    {
                        if (data.isFinished)
                        {
                            vHolder.statueTextView.Text = "Finished";
                            vHolder.statueTextView.SetTextColor(Android.Graphics.Color.ParseColor("#2E7D32"));
                        }
                        else
                        {
                            vHolder.statueTextView.Text = "Waiting...";
                            vHolder.statueTextView.SetTextColor(Android.Graphics.Color.ParseColor("#0277BD"));
                        }                        
                    }
                }
                

                if (data.totalSize > 0)
                {
                    vHolder.progressBar.Indeterminate = false;
                    int progress = (int)((float)((float)data.currentSize * 100 / (float)data.totalSize));
                    vHolder.progressBar.Progress = progress;
                }
                else
                {
                    vHolder.progressBar.Indeterminate = true;
                }

                return convertView;
            }
            public DownloadListAdapter(Activity parent)
            {
                this.parent = parent;
            }
        }

        class VAdapterViewHolder : Java.Lang.Object
        {
            public TextView nameTextView = null, websiteNameTextView = null, statueTextView = null;
            public ImageView imageView = null;
            public ProgressBar progressBar = null;

            public VAdapterViewHolder(View view)
            {
                nameTextView = view.FindViewById<TextView>(Resource.Id.downloadSingleNameTextView);
                websiteNameTextView = view.FindViewById<TextView>(Resource.Id.downloadSingleWebsiteNameTextView);
                statueTextView = view.FindViewById<TextView>(Resource.Id.downloadSingleStatusTextView);
                imageView = view.FindViewById<ImageView>(Resource.Id.downloadSingleImageView);
                progressBar = view.FindViewById<ProgressBar>(Resource.Id.downloadSingleProgressBar);
            }
        }
    }
}