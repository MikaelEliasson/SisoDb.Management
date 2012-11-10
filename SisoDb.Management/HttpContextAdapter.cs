using System.Web;
namespace SisoDb.Management
{
    public class HttpContextAdapter : IContextAdapter
    {
        public string GetFormValue(string key)
        {
            return HttpContext.Current.Request.Form[key];
        }

        public void SetContentType(string contentType)
        {
            HttpContext.Current.Response.ContentType = contentType;
        }

        public void Write(string response)
        {
            HttpContext.Current.Response.Write(response);
        }

        public void SetStatusCode(int statusCode)
        {
            HttpContext.Current.Response.StatusCode = statusCode;
        }


        public string GetAppRelativeCurrentExecutionFilePath()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
        }
    }
}
