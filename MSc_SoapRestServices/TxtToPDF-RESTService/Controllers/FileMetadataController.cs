///-----------------------------------------------------------------
///   Class:          FileMetadataController.cs
///   Description:    Controller for handling grabbing all blob metadata via REST
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Http;
using TxtToPDF_Implementation.Handlers;
using TxtToPDF_Implementation.Models;

namespace TxtToPDF_RESTService.Controllers
{
    public class FileMetadataController : ApiController
    {
        APIHandler fileImplementation = new APIHandler();

        //GET: api/FileMetadata 
        //Grabs all filemetadata from all available table entries
        public IEnumerable<FileMetadataSummaryDTO> Get()
        {
            return fileImplementation.GetAllFileMetadata();
        }
    }
}

