using System;
using SimpleHttpServer.Models;

namespace Hiale.Podbook.httpServer
{
    public class CustomRoute : Route
    {
        public new Func<HttpRequest, HttpResponseExtension> Callable { get; set; }
    }
}
