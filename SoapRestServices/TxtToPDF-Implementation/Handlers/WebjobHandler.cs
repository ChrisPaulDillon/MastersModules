///-----------------------------------------------------------------
///   Class:          WebjobHandler.cs
///   Description:    Contains all relevent methods relating to the webjob
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using TxtToPDF_Implementation.Models;

namespace TxtToPDF_Implementation.Handlers
{
    public class WebjobHandler
    {
        private const String partitionName = "FileMetadata_Partition_1";
        private CloudTableClient tableClient;
        private CloudTable table;
        private CloudStorageAccount storageAccount;

        /// <summary>
        /// Automatically configures the storage account and grabs the table reference as soon as this object is created
        /// </summary>
        public WebjobHandler()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("FileMetadata");
            table.CreateIfNotExists();
        }

        /// <summary>Returns the location of the blob storage container using the given connection string</summary>
        /// <returns>The relevant blob container</returns>
        public CloudBlobContainer GetCloudBlobContainer()
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer blobContainer = blobClient.GetContainerReference("filecontainer");
            if (blobContainer.CreateIfNotExists())
            {
                blobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
            }
            return blobContainer;
        }

        /// <summary>
        /// Used to convert the text file into the equivalent 
        /// </summary>
        /// <param name="input">The text file in a stream format</param>
        /// <param name="output">The converted PDF file in a stream format</param>
        public void ConvertToPDF(Stream input, Stream output)
        {
            //Read file from stream
            StreamReader reader = new StreamReader(input);
            string text = reader.ReadToEnd();

            //Create a default PDF and format
            PdfDocument doc = new PdfDocument();
            PdfSection section = doc.Sections.Add();
            PdfPageBase page = section.Pages.Add();
            PdfFont font = new PdfFont(PdfFontFamily.Helvetica, 11);

            //Saves the newly made PDF file to the stream, passing it onto the webjob
            doc.SaveToStream(output, FileFormat.PDF);
            doc.Close();
        }

        /// <summary>
        /// Handles inserting into the azure table. Is only fired when a new blob is detected in queue 
        /// and is called through the GeneratePDF method within the webjob
        /// </summary>
        /// <param name="title">The name of the blob being added</param>
        public void InsertIntoCloudTable(string txtBlobName, string pdfBlobName)
        {
            //Name formatting
            int start = txtBlobName.IndexOf("/") + 1;
            int end = txtBlobName.IndexOf(".");
            string titleNoExtension = txtBlobName.Substring(start, end - start);

            const String partitionName = "FileMetadata_Partition_1";

            var txtBlob = GetCloudBlobContainer().GetDirectoryReference("txtdocs").GetBlobReference(txtBlobName);
            var pdfBlob = GetCloudBlobContainer().GetDirectoryReference("pdfdocs").GetBlobReference(pdfBlobName);
            //Grab both the pdf and text doc containers

            var txtBlobUrl = txtBlob.Uri.ToString();
            var pdfBlobUrl = pdfBlob.Uri.ToString();
            //Grab the relevant URI's for each blob

            TableBatchOperation batchOperation = new TableBatchOperation();

            string rowKey = GetRowKey().ToString(); //Convert the rowkey back to a string 

            FileMetadataEntity fileMetadata = new FileMetadataEntity(partitionName, rowKey)
            {
                Title = titleNoExtension,
                CreationDate = DateTime.Now,
                TextFileBlobURL = txtBlobUrl,
                PDFFileBlobURL = pdfBlobUrl,
            };

            batchOperation.Insert(fileMetadata);
            table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Finds the last row key in the given azure table utilising the 'LastOrDefault()' method. 
        /// If no key is found, automatically default to '1' and if the key is found, increment by 1.
        /// </summary>
        /// <returns></returns>
        public int GetRowKey()
        {
            TableQuery<FileMetadataEntity> query = new TableQuery<FileMetadataEntity>();
            FileMetadataEntity fileMetaDataEntity = table.ExecuteQuery(query).OrderBy(a => a.PartitionKey).LastOrDefault();
            //Grabs the rowKey with the highest number

            int rowKey = 1;

            if (fileMetaDataEntity != null)
            {
                rowKey = Convert.ToInt32(fileMetaDataEntity.RowKey); //Get the last rowkey
                rowKey++; //Increment by one
            }
            return rowKey;
        }

        /// <summary>Gets the relevant queue using the given connection string </summary>
        /// <returns>The 'fileconvertor' Azure queue</returns>
        public CloudQueue GetCloudQueue()
        {
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue sampleQueue = queueClient.GetQueueReference("fileconvertor");
            sampleQueue.CreateIfNotExists();
            Trace.TraceInformation("Queue initialized");

            return sampleQueue;
        }

        /// <summary>Grabs the blob metadata matching the relevant blob URI</summary>
        /// <param name="blobUri">The URI of the current blob</param>
        /// <returns>The title or the name of the PDF stored in the blob metadata</returns>
        public String GetBlobMetaData(Uri blobUri)
        {
            CloudBlockBlob blob = new CloudBlockBlob(blobUri);
            blob.FetchAttributes();
            return blob.Metadata["Title"];
        }

        /// <summary>Grabs the file path and file data and creates a new blob</summary>
        /// <param name="path">The path of the azure container</param>
        /// <param name="uploadedFile">The file content the user has uploaded</param>
        public void GetBlob(string path, FileUpload uploadedFile)
        {
            var blob = GetCloudBlobContainer().GetBlockBlobReference(path);
            blob.Properties.ContentType = GetMimeType(uploadedFile.FileName);
            blob.UploadFromStream(uploadedFile.FileContent);
        }

        /// <summary>Gets the file extension type of the document being uploaded and returns the relevant MIME type, text being the default</summary>
        /// <param name="Filename">The name of the document being uploaded</param>
        /// <returns>The MIME type of the file</returns>
        private string GetMimeType(string fileName)
        {
            try
            {
                string ext = Path.GetExtension(fileName).ToLowerInvariant(); //Grabs the file extension type
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (key != null)
                {
                    string contentType = key.GetValue("Content Type") as String; //Assigns the related MIME type
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
            }
            return "text/plain"; //Always return this as this was created to only handle text files
        }
    }
}