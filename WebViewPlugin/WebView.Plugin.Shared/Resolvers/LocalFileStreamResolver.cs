using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;
using Xam.Plugin.Abstractions;

namespace Xam.Plugin.Shared.Resolvers
{
    public class LocalFileStreamResolver : IUriToStreamResolver
    {
        private readonly FormsWebViewRenderer Renderer;

        public LocalFileStreamResolver(FormsWebViewRenderer renderer)
        {
            Renderer = renderer;
        }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
                throw new Exception("Uri supplied is null.");

            var path = uri.AbsolutePath;
            return GetContent(path).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string path)
        {
            try
            {
                if (Renderer.GetCorrectBaseUrl() == null)
                    throw new Exception("Base URL was not set, could not load local content");
                
                var f = await StorageFile.GetFileFromApplicationUriAsync(new Uri(string.Concat(Renderer.GetCorrectBaseUrl(), path)));
                var stream = await f.OpenAsync(FileAccessMode.Read);

                return stream;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
