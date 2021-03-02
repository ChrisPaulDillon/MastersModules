///-----------------------------------------------------------------
///   Class:          Program
///   Description:    Default code for firing webjob upon startup
///   Author:         Microsoft                 Date: 26/04/2019
///   Live Version:   http://cdapp.azurewebsites.net/     
///-----------------------------------------------------------------
using Microsoft.Azure.WebJobs;

namespace TxtToPDFConvertor_Webjob
{
    class Program
    {
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
