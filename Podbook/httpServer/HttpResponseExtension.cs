
using System.IO;

namespace Hiale.Podbook.httpServer
{
	public static class HttpMethods
	{
		// ReSharper disable InconsistentNaming
		public const string GET = "GET";
		// ReSharper restore InconsistentNaming
	}

	public class HttpResponseExtension : SimpleHttpServer.Models.HttpResponse
	{
		public static HttpResponseExtension Ok => Create(200, "OK");

		public static HttpResponseExtension BadRequest => Create(400, "Bad Request");

		public static HttpResponseExtension NotFound => Create(404, "Not Found");

		public static HttpResponseExtension InternalServerError => Create(500, "Internal Server Error");

        public Stream ContentStream { get; set; }

		private static HttpResponseExtension Create(int statusCode, string reason)
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			return new HttpResponseExtension {StatusCode = statusCode.ToString(), ReasonPhrase = reason};
		}
	}
}
