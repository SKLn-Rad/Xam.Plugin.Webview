using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace Xam.Plugin.UWP
{
    public class LocalFileStreamResolver : IUriToStreamResolver
    {
        private readonly WeakReference<FormsWebViewRenderer> Reference;

        public LocalFileStreamResolver(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
                throw new Exception("Uri supplied is null.");

            var path = uri.AbsolutePath;
            return GetContent(path)?.AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string path)
        {
            if (!Reference.TryGetTarget(out FormsWebViewRenderer renderer))
                return default(IInputStream);

            try
            {
                if (renderer.GetBaseUrl() == null)
                    throw new Exception("Base URL was not set, could not load local content");

                var f = await StorageFile.GetFileFromApplicationUriAsync(new Uri(string.Concat(renderer.GetBaseUrl(), path)));
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
