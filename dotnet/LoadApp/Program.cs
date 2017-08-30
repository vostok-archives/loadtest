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

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var tasks = new List<Task>();
            var printTask = new Task(() =>
            {
                int counter = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var prevSuccess = successCount;
                    Thread.Sleep(StepMilliseconds);
                    counter++;
                    var newSuccess = successCount;
                    var rps = (double)(newSuccess - prevSuccess) / StepMilliseconds * 1000;
                    var avgRps = (double)successCount / counter / StepMilliseconds * 1000;
                    Console.WriteLine($"tasks= {tasks.Count}, success = {successCount}, error = {errorCount}, perSecond={rps}, avg={avgRps}, all={requestCount}");
                }
            });
            printTask.Start();

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 1; j++)
                {
                    var task = new Task(() =>
                    {
                        SendingLoop(httpClient, cancellationToken);
                    }, cancellationToken);
                    task.Start();
                    tasks.Add(task);
                }
                Thread.Sleep(3000);
            }
            Thread.Sleep(60000);
            cancellationTokenSource.Cancel();
            Console.WriteLine($"success = {successCount}, all = {requestCount}");
        }

        private static void SendingLoop(HttpClient httpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref requestCount);
                var httpResponseMessage = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "kload10"))
                    .Result;
                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    Interlocked.Increment(ref successCount);
                else
                    Interlocked.Increment(ref errorCount);
            }
        }
    }
}