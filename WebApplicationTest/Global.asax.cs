using System;
using System.Web;

namespace WebApplicationTest
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }
    }
}