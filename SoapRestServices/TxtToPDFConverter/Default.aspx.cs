///-----------------------------------------------------------------
///   Class:          Default.aspx.cs
///   Description:    Handles Web Application, allowing for uploading and displaying Blobs
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Linq;
using TxtToPDF_Implementation.Handlers;

namespace TxtToPDFConverter
{
    public partial class Default : System.Web.UI.Page
    {
        //Accessor for variables and methods for blob containers and queues
        private WebjobHandler handler = new WebjobHandler();

        private CloudBlobContainer GetPDFGalleryContainer()
        {
            return handler.GetCloudBlobContainer(); //Create a local blob referenced container
        }

        private CloudQueue GetPDFCreatorQueue()
        {
            return handler.GetCloudQueue(); //Grab the queue relevent to the task
        }

        // User clicked the "Submit" button
        protected void BtnSubmit_Click(object sender, EventArgs e)
        {         
            var fileName = Path.GetFileName(upload.FileName);//Get the file name specified by the user. 
            String path = "textdocs/" + fileName; //Go to container, create a new blob
            handler.GetBlob(path, upload); //Create blob and upload using specified path and upload content
            GetPDFCreatorQueue().AddMessage(new CloudQueueMessage(System.Text.Encoding.UTF8.GetBytes(fileName)));
            System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", fileName)); //Details blob info on webjob
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {

                gvPDFs.DataSource = from o in GetPDFGalleryContainer().GetDirectoryReference("pdfdocs").ListBlobs()
                                                 select new
                                                 {
                                                     Url = o.Uri,
                                                     Title = handler.GetBlobMetaData(o.Uri) //Calls GetBlobMetaData to get metadata information for each entry
                                                 };
                gvPDFs.DataBind();
            }
            catch (Exception)
            {
            }
        }

        //Reloads the blobs
        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            try
            {

                gvPDFs.DataSource = from o in GetPDFGalleryContainer().GetDirectoryReference("pdfdocs").ListBlobs()
                                                 select new
                                                 {
                                                     Url = o.Uri,
                                                     Title = handler.GetBlobMetaData(o.Uri) //Calls GetBlobMetaData to get metadata information for each entry
                                                 };
                gvPDFs.DataBind();
            }
            catch (Exception)
            {
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
