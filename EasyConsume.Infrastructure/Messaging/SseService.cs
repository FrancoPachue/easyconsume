using LaunchDarkly.EventSource;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EasyConsume.Infrastructure.Messaging
{
    public class SseServiceBase : IDisposable
    {
        private EventSource _essClient;
        private HttpClientHandler _httpClientHandler;
        private bool _disposed = false;

        public event Action<Exception> ErrorOccurred;

        public async Task StartSseAsync(Uri uri, Action<MessageReceivedEventArgs> messageHandler, Action<MessageReceivedEventArgs> heartbeatHandler)
        {
            _httpClientHandler = new HttpClientHandler();
            var requestHeaders = BuildRequestHeaders("your_user", "your_password", DecompressionMethods.All);

            var customMessageHandler = new MessageHandler(_httpClientHandler);

            var configBuilder = Configuration.Builder(uri);

            var essConfig = configBuilder.HttpMessageHandler(customMessageHandler)
                            .ResponseStartTimeout(TimeSpan.FromSeconds(10))
                            .ReadTimeout(TimeSpan.FromSeconds(60))
                            .RequestHeaders(requestHeaders).Build();

            _essClient = new EventSource(essConfig);

            customMessageHandler.Connected += ConnectedHandler;
            _essClient.Closed += (sender, e) => _essClient.Close();
            _essClient.Error += ErrorHandler;

            _essClient.MessageReceived += (sender, e) =>
            {
                switch (e.EventName)
                {
                    case "error":
                        _essClient.Close();
                        ErrorHandler(sender, new ExceptionEventArgs(new Exception(e.Message.Data)));
                        break;
                    case "heartbeat":
                        heartbeatHandler(e);
                        break;
                    default:
                        messageHandler(e);
                        break;
                }
            };

            await _essClient.StartAsync();
        }

        private void ConnectedHandler(object sender, ConnectedEventArgs e)
        {
            HttpResponseMessage response = e.HttpResponse;
            string queueId = response.Headers.Contains("Queue-ID") ?
                response.Headers.GetValues("Queue-ID").FirstOrDefault() : string.Empty;
            Console.WriteLine($"Connected: {Convert.ToInt32(response.StatusCode)} {response.ReasonPhrase}: {"Queue-ID"}={queueId}");
        }

        private void ErrorHandler(object sender, ExceptionEventArgs e)
        {
            ErrorOccurred?.Invoke(e.Exception);
        }

        public static Dictionary<string, string> BuildRequestHeaders(string username, string password,
    DecompressionMethods compressionMethods = DecompressionMethods.All)
        {
            List<string> supportedEncodings = new List<string>();
            if ((DecompressionMethods.GZip & compressionMethods) != 0)
                supportedEncodings.Add("gzip");
            if ((DecompressionMethods.Deflate & compressionMethods) != 0)
                supportedEncodings.Add("deflate");
            if ((DecompressionMethods.Brotli & compressionMethods) != 0)
                supportedEncodings.Add("br");
            var acceptEncodingValue = string.Join(",", supportedEncodings);

            var connectionString =
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            var requestHeaders = new Dictionary<string, string>
            {
                {"Authorization", $"Basic {connectionString}"},
            };
            if (!string.IsNullOrWhiteSpace(acceptEncodingValue))
                requestHeaders.Add("Accept-Encoding", acceptEncodingValue);

            return requestHeaders;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _essClient?.Dispose();
                _httpClientHandler?.Dispose();
            }
            _disposed = true;
        }
    }

}
