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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var tasks = Enumerable.Range(0, 30).Select(x => new Task(() =>
            {
                while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
                { 
                    var httpResponseMessage = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "kload10")).Result;
                    Interlocked.Increment(ref requestCount);
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                        Interlocked.Increment(ref successCount);
                }
            }))
            .Concat(new[] {new Task(() =>
                {
                    int counter = 0;
                    while (stopwatch.Elapsed < TimeSpan.FromSeconds(60))
                    {
                        var prevSuccess = successCount;
                        Thread.Sleep(StepMilliseconds);
                        counter++;
                        var newSuccess = successCount;
                        Console.WriteLine($"success = {successCount}, all = {requestCount}, perSecond={(double)(newSuccess-prevSuccess)/StepMilliseconds*1000},"+
                            $" avg={(double)successCount/counter/StepMilliseconds*1000}");
                    }
                })})
            .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks);
            Console.WriteLine($"success = {successCount}, all = {requestCount}");
        }


    }
}