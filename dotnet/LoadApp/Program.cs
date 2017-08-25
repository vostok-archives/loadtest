using System;
using System.Collections.Generic;
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
        static int errorCount = 0;

        static void Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8888");
            httpClient.Timeout = TimeSpan.FromSeconds(11);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var tasks = new List<Task>();

            var printTask = new Task(() =>
            {
                int counter = 0;
                while (true)
                {
                    var prevSuccess = successCount;
                    Thread.Sleep(StepMilliseconds);
                    counter++;
                    var newSuccess = successCount;
                    Console.WriteLine($"success = {successCount}, error={errorCount}, all = {requestCount},"
                                      + $" perSecond={(double) (newSuccess - prevSuccess) / StepMilliseconds * 1000},"
                                      + $" avg={(double) successCount / counter / StepMilliseconds * 1000:N1}"
                                      + $" tasks_count={tasks.Count}");
                }
            });
            printTask.Start();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(120))
            {
                if (tasks.Count < 600)
                {
                    tasks.AddRange(Enumerable.Range(0, 50).Select(i => LoadAsync(httpClient)).ToArray());
                }
                Thread.Sleep(StepMilliseconds);
            }
            Console.WriteLine($"success = {successCount}, all = {requestCount}");
        }

        private static async Task LoadAsync(HttpClient httpClient)
        {
            while (true)
            {
                try
                {
                    Interlocked.Increment(ref requestCount);
                    var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "kload10")).ConfigureAwait(false);
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref errorCount);
                }
                catch (Exception e)
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
        }
    }
}