using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoadApp
{
    class Program
    {
        private const int StepMilliseconds = 500;
        static int requestCount = 0;
        static int successCount = 0;

        static void Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8888");
            httpClient.Timeout = TimeSpan.FromSeconds(11);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var printTask = new Task(() =>
            {
                int counter = 0;
                while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
                {
                    var prevSuccess = successCount;
                    Thread.Sleep(StepMilliseconds);
                    counter++;
                    var newSuccess = successCount;
                    Console.WriteLine($"success = {successCount}, all = {requestCount}, perSecond={(double)(newSuccess - prevSuccess) / StepMilliseconds * 1000}," +
                                      $" avg={(double)successCount / counter / StepMilliseconds * 1000}");
                }
            });
            printTask.Start();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
            {
                Enumerable.Range(0, 1000)
                    .Select(x => LoadAsync(httpClient))
                    .ToArray();
                Thread.Sleep(StepMilliseconds);
            }
            Console.WriteLine($"success = {successCount}, all = {requestCount}");
        }

        private static async Task LoadAsync(HttpClient httpClient)
        {
            Interlocked.Increment(ref requestCount);
            var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "kload10")).ConfigureAwait(false);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                Interlocked.Increment(ref successCount);
        }
    }
}