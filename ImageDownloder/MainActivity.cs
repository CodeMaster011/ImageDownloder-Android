using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading;
using Android.Content;
using System.Collections.Generic;
using Android.Graphics;
//https://developer.xamarin.com/recipes/android/fundamentals/activity/pass_data_between_activity/
namespace ImageDownloder
{
	[Activity (Label = "Image Downloder", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		int count = 1;

        //private UiRunner uiRunner = null;

        private Button submitButton = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //uiRunner = new UiRunner();

            // Get our button from the layout resource,
            // and attach an event to it
            submitButton = FindViewById<Button>(Resource.Id.myButton);

            submitButton.Click += delegate
            {
                //Thread th = new Thread(downloadFile);
                //th.Name = $"Download{count++}";
                //th.Start();
                //submitButton.Text = "Downloading...";

                Android.Views.Display display = WindowManager.DefaultDisplay;
                Point size = new Point();
                display.GetSize(size);
                MyGlobal.screenSize = new System.Drawing.Size(size.X, size.Y);

                MyPicasso.CreateNewPicasso(ApplicationContext);
                MyGlobal.MoveToWebpage(new Website.IdlebrainWebsiteReader().IndexPageReader);
                var websiteBrowser = new Intent(this, typeof(WebsiteBrowserActivity));
                StartActivity(websiteBrowser);
                
                //var websiteBrowserAdvance = new Intent(this, typeof(WebsiteBrowserActivityAdvance));
                //StartActivity(websiteBrowserAdvance);
            };
            //Environment.ExternalStorageDirectory.AbsolutePath
            
        }
        private void downloadFile()
        {
            string desPath = Environment.ExternalStorageDirectory.AbsolutePath + "/img.jpg";
            if (System.IO.File.Exists(desPath)) System.IO.File.Delete(desPath);

            //var result = Helper.DownloadFile("http://virtualedge.ca/wp-content/gallery/architecture/vec-architecture-01.jpg", desPath);
            var mResult = Helper.DownloadFile("http://www.idlebrain.com/movie/photogallery/kajalagarwal1/");
            //http://www.idlebrain.com/movie/photogallery/kajalagarwal1/

            var result = Helper.DumpDataToFile(mResult, Environment.ExternalStorageDirectory.AbsolutePath + "/page.html");

            //mHandler.NotifyDownloadCompleted(result == string.Empty ? "Download Completed" : result);
            //mHandler.NotifyTextChange(submitButton, "Download Again");

            //UiRunner.RunOnUi(new UiRunner.Action(() => {
            //    submitButton.Text = "Download Again";
            //    Toast.MakeText(BaseContext, result == string.Empty ? "Download Completed" : result, ToastLength.Short).Show();
            //}));
            UiRunner.RunOnUi(new UiRunner.Action(() => {
                submitButton.Text = "Download Again";
                Toast.MakeText(BaseContext, result == true ? "Download Completed and Saved." : "Error : Something went wrong.", ToastLength.Short).Show();
            }));
        }        
	}
}


