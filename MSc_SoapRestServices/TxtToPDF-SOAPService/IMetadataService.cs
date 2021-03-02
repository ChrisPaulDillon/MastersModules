///-----------------------------------------------------------------
///   Class:          IMetadataService.cs
///   Description:    Interface Class for MetadataService. SOAP.
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using TxtToPDF_Implementation.Models;

namespace TxtToPDF_SOAPService
{
    [ServiceContract]
    public interface IMetadataService
    {  
        //Set both the operation contracts for grabbing all the filemetadata and for getting the PDF relevant to the ID
        [OperationContract]
        List<FileMetadataSummaryDTO> GetFileMetaData();

        [OperationContract]
        Stream GetRequestedPDF(string id);
    }
}
