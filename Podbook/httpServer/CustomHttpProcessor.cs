using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SimpleHttpServer;
using SimpleHttpServer.Models;

namespace Hiale.Podbook.httpServer
{
    public class CustomHttpProcessor : HttpProcessor
    {
        private readonly List<CustomRoute> _routes;

        private readonly MethodInfo _getRequestMethod;
        private readonly MethodInfo _writeMethod;

        public CustomHttpProcessor()
        {
            _routes = new List<CustomRoute>();
            _getRequestMethod = typeof(HttpProcessor).GetMethod("GetRequest", BindingFlags.NonPublic | BindingFlags.Instance); //I didn't want to change the orignal code, so I must use reflection to call private methods
            _writeMethod = typeof(HttpProcessor).GetMethod("Write", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public void AddRoute(CustomRoute route)
        {
            _routes.Add(route);
        }

        public new void HandleClient(TcpClient tcpClient)
        {
            var inputStream = GetInputStream(tcpClient);
            var outputStream = GetOutputStream(tcpClient);

            var request = (HttpRequest) _getRequestMethod.Invoke(this, new object[] {inputStream, outputStream});

            var response = RouteRequest(inputStream, outputStream, request);

            if (response.Content == null)
            {
                if (response.StatusCode != "200")
                {
                    response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                }
            }

            WriteResponse(outputStream, response);

            outputStream.Flush();
            outputStream.Close();
            inputStream.Close();
        }

        protected new HttpResponseExtension RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {

            var routes = _routes.Where(x => Regex.Match(request.Url, x.UrlRegex).Success).ToList();

            if (!routes.Any())
                return HttpResponseExtension.NotFound;

            var route = routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
                return HttpResponseExtension.BadRequest;

            var match = Regex.Match(request.Url, route.UrlRegex);
            request.Path = match.Groups.Count > 1 ? match.Groups[1].Value : request.Url;

            request.Route = route;
            try
            {
                return route.Callable(request);
            }
            catch (Exception)
            {
                return HttpResponseExtension.InternalServerError;
            }
        }

        protected virtual void WriteResponse(Stream stream, HttpResponseExtension response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            var isContentStream = response.ContentStream != null && response.ContentStream.CanRead && response.ContentStream.CanSeek;

            response.Headers["Content-Length"] = isContentStream ? response.ContentStream.Length.ToString() : response.Content.Length.ToString();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            stringBuilder.Append(string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            stringBuilder.Append("\r\n\r\n");
            _writeMethod.Invoke(null, new object[] {stream, stringBuilder.ToString()});

            if (isContentStream)
            {
                response.ContentStream.CopyTo(stream);
                response.ContentStream.Dispose();
            }
            else
            {
                stream.Write(response.Content, 0, response.Content.Length);
            }
        }
    }
}
