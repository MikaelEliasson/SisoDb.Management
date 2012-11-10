using System;

namespace SisoDb.Management.Test
{
    class FakeContextAdapter : IContextAdapter
    {

        public string AppRelativeCurrentExecutionFilePath;
        public Func<string, string> OnGetFormValue = str => null;
        public string ContentType;
        public string Response;
        public int StatusCode;


        public string GetFormValue(string key)
        {
            return OnGetFormValue(key);
        }

        public void SetContentType(string contentType)
        {
            ContentType = contentType;
        }

        public void Write(string response)
        {
            Response = response;
        }

        public void SetStatusCode(int statusCode)
        {
            StatusCode = statusCode;
        }

        public string GetAppRelativeCurrentExecutionFilePath()
        {
            return AppRelativeCurrentExecutionFilePath;
        }

        public FakeContextAdapter WithPath(string path)
        {
            AppRelativeCurrentExecutionFilePath = path;
            return this;
        }

        public FakeContextAdapter WithEntityType<T>()
        {
            MergeOnFormValueFunc("entityType", typeof(T).AssemblyQualifiedName);
            return this;
        }

        public FakeContextAdapter WithPredicate(string predicate)
        {
            MergeOnFormValueFunc("predicate", predicate);
            return this;
        }

        public FakeContextAdapter WithSetup(string setup)
        {
            MergeOnFormValueFunc("setup", setup);
            return this;
        }

        public FakeContextAdapter WithOrderBy(string value)
        {
            MergeOnFormValueFunc("orderby", value);
            return this;
        }

        /// <summary>
        /// 'asc' or 'desc'
        /// </summary>
        public FakeContextAdapter WithSortOrder(string value)
        {
            MergeOnFormValueFunc("sortorder", value);
            return this;
        }

        public FakeContextAdapter WithPage(object value)
        {
            MergeOnFormValueFunc("page", value.ToString());
            return this;
        }

        public FakeContextAdapter WithPageSize(object value)
        {
            MergeOnFormValueFunc("pagesize", value.ToString());
            return this;
        }

        public FakeContextAdapter WithJson(string value)
        {
            MergeOnFormValueFunc("json", value);
            return this;
        }

        private void MergeOnFormValueFunc(string key, string value)
        {
            var old = OnGetFormValue;
            OnGetFormValue = str => str == key ? value : old(str);
        }

        public FakeContextAdapter WithId(object value)
        {
            MergeOnFormValueFunc("entityId", value.ToString());
            return this;
        }
    }
}
