///-----------------------------------------------------------------
///   Class:          APIHandler.cs
///   Description:    Contains all methods relating to handling the REST and SOAP projects
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using TxtToPDF_Implementation.Models;

namespace TxtToPDF_Implementation.Handlers
{
    public class APIHandler
    {
        private const String partitionName = "FileMetadata_Partition_1";
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;

        /// <summary>
        /// Automatically configures the storage account and grabs the table reference as soon as this object is created
        /// </summary>
        public APIHandler()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("FileMetadata");
        }

        /// <summary>
        /// Grabs all the file metadata summaries using the given azure table
        /// </summary>
        /// <returns>A collection of IEnumberable 'FileMetadataSummaryDTO' objects</returns>
        public IEnumerable<FileMetadataSummaryDTO> GetAllFileMetadata()
        {
            TableQuery<FileMetadataEntity> query = new TableQuery<FileMetadataEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<FileMetadataEntity> entityList = new List<FileMetadataEntity>(table.ExecuteQuery(query));
            //Grab all FileMetadataEntity objects returned by the query

            //Create a list of FileMetadata from the list of FileMetadataEntity
            IEnumerable<FileMetadataSummaryDTO> FileMetadataList = from e in entityList
                                                                   select new FileMetadataSummaryDTO()
                                                                   {
                                                                       FileMetadataID = e.RowKey,
                                                                       Title = e.Title,
                                                                       CreationDate = e.CreationDate

                                                                   };
            return FileMetadataList;
        }

        /// <summary>
        /// Attempts to grab the requested FileMetadataEntity using the input ID as the row key. 
        /// If successful, grabs the blob and opens a HttpResponseMessage which reads the blob content and sets 
        /// the values 'MediaType' and 'ContentDispositionHeader' to allow for downloading of the PDF file
        /// If the table operation or grabbing the blob fails, automatically return a not found exception regardless
        /// </summary>
        /// <param name="id">The ID being inserted into the API</param>
        /// <returns>A HttpResponseMessage with the given blob & related PDF file</returns>
        public HttpResponseMessage GetRequestedPDF(string id)
        {
            try
            {

                TableOperation getOperation = TableOperation.Retrieve<FileMetadataEntity>(partitionName, id);
                TableResult getOperationResult = table.Execute(getOperation);
                //Create a retrieve operation that takes a FileMetadataEntity object
      
                var file = (FileMetadataEntity)getOperationResult.Result; //Cast the table result as a FileMetadataEntity object
                string blobName = file.Title + ".txt.pdf"; //Assign the extension in order to get the correct reference

                WebjobHandler _azureHandler = new WebjobHandler();

                var blob = _azureHandler.GetCloudBlobContainer().GetDirectoryReference("pdfdocs").GetBlockBlobReference(blobName);
                blob.FetchAttributes(); 
                //Grab the relevant blob using the blob container and given ID

                Stream blobStream = blob.OpenRead();
                var blobUrl = blob.Uri.ToString();

                var message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StreamContent(blobStream); //Set stream content to blob stream data
                message.Content.Headers.ContentLength = blob.Properties.Length;
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                message.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                        FileName = blob.Name,
                        Size = blob.Properties.Length
                };          
                return message; //Return the HttpResponseMessage as a PDF for the given 
            }
            catch
            {
         
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                return message;
            }   
        }


        /// <summary>
        /// Attempts to grab the requested FileMetadataEntity using the input ID as the row key. 
        /// If successful, grabs the object and references the ID to get the blob.
        /// Assigns the blob to a stream object.
        /// </summary>
        /// <param name="id">The RowKey of the given blob</param>
        /// <returns>Stream containing blob data</returns>
        public Stream GetPDF(string id)
        {
            try
            {
                TableOperation getOperation = TableOperation.Retrieve<FileMetadataEntity>(partitionName, id);
                TableResult getOperationResult = table.Execute(getOperation);
                //Create a retrieve operation that takes a FileMetadataEntity object

                var file = (FileMetadataEntity)getOperationResult.Result; //Cast the table result as a FileMetadataEntity object
                string blobName = file.Title + ".txt.pdf"; //Assign the extension in order to get the correct reference

                WebjobHandler _azureHandler = new WebjobHandler();

                var blob = _azureHandler.GetCloudBlobContainer().GetDirectoryReference("pdfdocs").GetBlockBlobReference(blobName);
                blob.FetchAttributes();
                //Grab the relevant blob using the blob container and given ID

                Stream blobStream = blob.OpenRead(); //Read the blob into a Stream object

                return blobStream; //Return stream data gathered from blob
            }
            catch
            {
                return null; //Return null if no blob is found
            }
        }
    }
}