///-----------------------------------------------------------------
///   Class:          FileDataController.cs
///   Description:    Controller for handling PDF downloads via REST
///   Author:         Christopher Dillon                   Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Web.Http;
using TxtToPDF_Implementation.Handlers;

namespace TxtToPDF_RESTService.Controllers
{
    public class FileDataController : ApiController
    {
        APIHandler fileImplementation = new APIHandler();

        /// <summary>
        /// Grabs the HttpResponseMessage using the given rowKey ID
        /// If anything but a success message is returned, return "NOT FOUND" to the controller
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string id)
        {
            var message = fileImplementation.GetRequestedPDF(id);
            if (message.IsSuccessStatusCode)
                return message;
            return Request.CreateResponse(HttpStatusCode.NotFound, "NOT FOUND");

        }
    }
}
