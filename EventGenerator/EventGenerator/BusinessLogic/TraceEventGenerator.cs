using Vostok.Tracing;

namespace EventGenerator.BusinessLogic
{
    public class TraceEventGenerator : IEventGenerator
    {
        public void Generate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                using (var spanBuilder = Trace.BeginSpan())
                {
                    spanBuilder.SetAnnotation(TracingAnnotationNames.Operation, "Generate Trace");
                    spanBuilder.SetAnnotation(TracingAnnotationNames.Kind, "loadtest");
                    spanBuilder.SetAnnotation(TracingAnnotationNames.Service, "event-generator");
                    spanBuilder.SetAnnotation(TracingAnnotationNames.Host, "localhost");
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpUrl, "send");
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpRequestContentLength, 1024);
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpResponseContentLength, 2048);
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpCode, 200);
                }
            }
        }
    }
}