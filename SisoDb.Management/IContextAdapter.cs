namespace SisoDb.Management
{
    public interface IContextAdapter
    {
        string GetFormValue(string key);
        void SetContentType(string contentType);
        void Write(string response);
        void SetStatusCode(int statusCode);
        string GetAppRelativeCurrentExecutionFilePath();
    }
}
