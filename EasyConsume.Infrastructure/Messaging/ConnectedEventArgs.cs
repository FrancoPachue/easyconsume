using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyConsume.Infrastructure.Messaging
{
    public class ConnectedEventArgs : EventArgs
    {
        public HttpResponseMessage HttpResponse { get; }
        public ConnectedEventArgs(HttpResponseMessage response)
        {
            HttpResponse = response;
        }
    }

    public class MessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Occurs when the EventSource successfully connects to the EventSource API. Returns a HttpResponseMessage allowing to peek the response headers or status code.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        protected string AssermblyVersion;

        public MessageHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
            AssermblyVersion =
                (Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0)).ToString(3);
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.Add("User-Agent", $"csharp-v{AssermblyVersion}");

            var response = await base.SendAsync(request, cancellationToken);
            Connected?.Invoke(this, new ConnectedEventArgs(response));

            response.Content = new DecompressingHttpContent(await response.Content.ReadAsStreamAsync(cancellationToken), response);
            return response;
        }
    }
}
