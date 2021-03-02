///-----------------------------------------------------------------
///   Class:          FileMetadataSummaryDTO
///   Description:    Object class used for grabbing summarised metadata to be used with both SOAP and REST projects
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;

namespace TxtToPDF_Implementation.Models
{
    public class FileMetadataSummaryDTO
    {

        [Key]
        public string FileMetadataID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }


    }
}