///-----------------------------------------------------------------
///   Class:          Function.cs
///   Description:    Webjob handler for creating a PDF Blob using its text blob counterpart
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using TxtToPDF_Implementation.Handlers;

namespace TxtToPDFConvertor_Webjob
{
    public class Functions
    {
        /// <summary>Application specific webjob code that is event driven. This method will be called when messages appear in the queue</summary>
        /// <param name="blobInfo">Gives and the name of the blob being processed</param>
        /// <param name="inputBlob">The text file being uploaded</param>
        /// <param name="outputBlob">The converted PDF blob</param>
        /// <param name="logger"></param>
        public static void GeneratePDF(
        [QueueTrigger("fileconvertor")] String blobInfo,
        [Blob("filecontainer/textdocs/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("filecontainer/pdfdocs/{queueTrigger}.pdf")] CloudBlockBlob outputBlob, TextWriter logger)
        {
            logger.WriteLine("GeneratePDF() started...");
            logger.WriteLine("Input blob is: " + blobInfo);

            var txtBlobName = inputBlob.Name.ToString();  //Grab the txt Blobs name

            int start = inputBlob.Name.ToString().IndexOf("/") + 1;//Grab the text before the full stop to not include any the file extensions 
            int end = inputBlob.Name.ToString().IndexOf("."); //Grab the text before the extension type (to remove .txt)
            var pdfBlobName = inputBlob.Name.ToString().Substring(start, end - start) + ".pdf"; //recreate string and add additional ".pdf" extension for blob metadata
       

            // Open streams to blobs for reading and writing as appropriate.
            // Pass references to application specific methods
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                WebjobHandler handler = new WebjobHandler();
                handler.InsertIntoCloudTable(txtBlobName, pdfBlobName); //Calls the Implementation class to carry out the table insert operation
                handler.ConvertToPDF(input, output); //Function that converts text file to PDF
                outputBlob.Properties.ContentType = "application/pdf"; //Setting the content type allows the blob to be opened in PDF format
                outputBlob.Metadata["Title"] = pdfBlobName; //Set the metadata using the result from the substring
            }

            logger.WriteLine("GeneratePDF() completed...");
        }
    }
}
