using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace WebView.Plugin.Shared.Resolvers
{
    public class LocalFileStreamResolver : IUriToStreamResolver
    {
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
                throw new Exception("Uri supplied is null.");

            string path = uri.AbsolutePath;
            return GetContent(path).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string path)
        {
            try
            {
                var uri = string.Concat("ms-appx:///", path);
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

                IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
                return stream;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
