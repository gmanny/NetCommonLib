using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorCommon.Tasks;

namespace MonitorCommon.Http
{
    public static class HttpRequestUtil
    {
        public static async Task<WebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken ct)
        {
            using (ct.Register(() => request.Abort(), useSynchronizationContext: false))
            {
                try
                {
                    return await request.GetResponseAsync();
                }
                catch (WebException ex)
                {
                    // WebException is thrown when request.Abort() is called,
                    // but there may be many other reasons,
                    // propagate the WebException to the caller correctly
                    if (ct.IsCancellationRequested)
                    {
                        // the WebException will be available as Exception.InnerException
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    // cancellation hasn't been requested, rethrow the original WebException
                    throw;
                }
            }
        }

        public static async Task<HttpWebResponse> SendRequestGen(string url, string method = "GET", IEnumerable<(string name, string value)> headers = null, bool allowAutoRedirect = true, Action<HttpWebRequest> setupRequest = null, CancellationToken ct = default)
        {
            HttpClient c = new HttpClient();
#pragma warning disable SYSLIB0014
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
#pragma warning restore SYSLIB0014

            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Method = method;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    req.Headers.Add(header.name, header.value);
                }
            }

            req.AllowAutoRedirect = allowAutoRedirect;

            setupRequest?.Invoke(req);

            HttpWebResponse result = (HttpWebResponse) await req.GetResponseAsync(ct);

            return result;
        }

        public static async Task<Stream> SendRequestChecked(string url, string method = "GET", IEnumerable<(string name, string value)> headers = null, bool allowAutoRedirect = true, Action<HttpWebRequest> setupRequest = null, CancellationToken ct = default)
        {
            HttpWebResponse responseObject = await SendRequestGen(url, method, headers, allowAutoRedirect, setupRequest, ct);

            if (responseObject.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Got {responseObject.StatusCode} {responseObject.StatusDescription} from {url} ({responseObject.ResponseUri})");
            }

            Stream responseStream = responseObject.GetResponseStream();
            if (responseStream == null)
            {
                throw new Exception($"No response body from {url} ({responseObject.ResponseUri})");
            }

            return responseStream;
        }

        public static async Task<string> SendRequest(string url, string method = "GET", IEnumerable<(string name, string value)> headers = null, bool allowAutoRedirect = true, Action<HttpWebRequest> setupRequest = null, CancellationToken ct = default)
        {
            Stream responseStream = await SendRequestChecked(url, method, headers, allowAutoRedirect, setupRequest, ct);
            
            StreamReader sr = new StreamReader(responseStream);
            return await sr.ReadToEndAsync();
        }

        public static Action<HttpWebRequest> AddRequestTimeoutDelegate(TimeSpan timeout) => req =>
        {
            req.Timeout = (int) timeout.TotalMilliseconds;
            req.ReadWriteTimeout = (int) timeout.TotalMilliseconds;
        };

        public static async Task<T> RetriedRequest<T>(Func<Task<T>> req, TimeSpan requestRetryDelay, int maxRequestRetry, ILogger logger, CancellationToken ct = default)
        {
            async Task<T> Iter(int retries = 0)
            {
                return await req().RecoverWith(async err =>
                {
                    if (retries >= maxRequestRetry)
                    {
                        return await Task.FromException<T>(new Exception($"Max number of retries reached, error: {err.Message}", err));
                    }

                    if (ct.IsCancellationRequested)
                    {
                        return await Task.FromException<T>(err);
                    }

                    logger.LogDebug($"Ignoring error {err.Message} on http request, retrying in {requestRetryDelay.TotalSeconds:0.00} sec, retries = {retries}");

                    await Task.Delay(requestRetryDelay);

                    return await Iter(retries + 1);
                });
            }

            return await Iter();
        }
    }
}