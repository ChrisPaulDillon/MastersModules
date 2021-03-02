///-----------------------------------------------------------------
///   Class:          MetadataService.svc.cs
///   Description:    Service class for calling SOAP methods
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TxtToPDF_Implementation.Handlers;
using TxtToPDF_Implementation.Models;

namespace TxtToPDF_SOAPService
{
    public class MetadataService : IMetadataService
    {
        APIHandler fileImplementation = new APIHandler(); //From Implementation project

        public List<FileMetadataSummaryDTO> GetFileMetaData()
        {
            //Grabs all filemetadata using the Implementation project
            IEnumerable<FileMetadataSummaryDTO> allMetaData = fileImplementation.GetAllFileMetadata();
            return allMetaData.ToList();
        }

        public Stream GetRequestedPDF(string id)
        {
            return fileImplementation.GetPDF(id); //Uses method from Implementation project to grab relevent PDF
        }
    }
}