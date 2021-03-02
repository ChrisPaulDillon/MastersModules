///-----------------------------------------------------------------
///   Class:          FileMetadataEntity.cs
///   Description:    Object class relating to the Azure table
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace TxtToPDF_Implementation.Models
{
    public class FileMetadataEntity : TableEntity
    {
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public string TextFileBlobURL { get; set; }
        public string PDFFileBlobURL { get; set; }
        public string FileMetadataID { get; set; }

        public FileMetadataEntity(string partitionKey, string fileMetadataID)
        {
            PartitionKey = partitionKey;
            RowKey = fileMetadataID;
        }

        public FileMetadataEntity() { }
    }
}