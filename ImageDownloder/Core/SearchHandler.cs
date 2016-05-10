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

namespace ImageDownloder.Core
{
    class SearchHandler
    {
        private AutoResetEvent waitHandler = new AutoResetEvent(false);
        private string searchText = null;
        private bool dataSetModified = false;
        private WebPageData[] dataSet = null;
        private bool stop = false;

        public SearchHandler()
        {
            new Thread(doSearch).Start();
        }

        public void SetDataSet(WebPageData[] dataSet)
        {
            this.dataSet = dataSet;
            dataSetModified = true;
        }

        public void SetQuery(string searchText)
        {
            this.searchText = searchText;
            waitHandler.Set();
        }

        private void doSearch()
        {            
            while (!stop)
            {
                if (this.searchText == null) waitHandler.WaitOne();

                var searchText = this.searchText;   //make a local copy to avoid

                

            }
        }        
    }
}