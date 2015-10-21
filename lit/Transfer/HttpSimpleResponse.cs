using System.Net;

namespace lit
{
    public class HttpSimpleResponse
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }

        public HttpSimpleResponse(HttpStatusCode statusCode)
            : this(statusCode, string.Empty)
        {

        }

        public HttpSimpleResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }

}
