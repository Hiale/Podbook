using System.Collections.Generic;
using System.IO;
using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;

namespace Hiale.Podbook.httpServer
{
    public class CustomFileSystemRouteHandler
    {
        public Dictionary<string, string> Files { get; }

        public CustomFileSystemRouteHandler(Dictionary<string, string> files)
        {
            Files = files;
        }

        public HttpResponseExtension Handle(HttpRequest request)
        {
            if (!Files.TryGetValue(request.Path, out var path))
                return HttpResponseExtension.NotFound;
            var response = HttpResponseExtension.Ok;
            response.Headers["Content-Type"] = QuickMimeTypeMapper.GetMimeType(Path.GetExtension(path));
            response.ContentStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return response;
        }
    }
}
