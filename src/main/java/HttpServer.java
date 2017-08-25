import org.rapidoid.buffer.Buf;
import org.rapidoid.http.AbstractHttpServer;
import org.rapidoid.http.HttpStatus;
import org.rapidoid.http.MediaType;
import org.rapidoid.net.abstracts.Channel;
import org.rapidoid.net.impl.RapidoidHelper;

public class HttpServer extends AbstractHttpServer {

    private static final byte[] URI_TH = "/th".getBytes();
    private static final byte[] URI_MTT = "/mtt".getBytes();
    private static final byte[] URI_NOOP = "/noop".getBytes();
    private static final byte[] URI_GEN = "/gen".getBytes();
    private static final byte[] URI_KLOAD_10 = "/kload10".getBytes();
    private static final byte[] URI_KLOAD_100 = "/kload100".getBytes();
    private static final byte[] URI_KLOAD_1000 = "/kload1000".getBytes();
    private final MetricsReporter metricsReporter;
    private final LoadGenerator loadGenerator;

    HttpServer(MetricsReporter metricsReporter, LoadGenerator loadGenerator) {
        this.metricsReporter = metricsReporter;
        this.loadGenerator = loadGenerator;
    }

    @Override
    protected HttpStatus handle(Channel ctx, Buf buf, RapidoidHelper req) {
        if (req.isGet.value) {
            boolean isKeepAlive = req.isKeepAlive.value;
            if (matches(buf, req.path, URI_KLOAD_10)) {
                return GenerateKafkaLoad(ctx, 10, isKeepAlive, true);
            } else if (matches(buf, req.path, URI_KLOAD_100)) {
                return GenerateKafkaLoad(ctx, 100, isKeepAlive, true);
            } else if (matches(buf, req.path, URI_KLOAD_1000)) {
                return GenerateKafkaLoad(ctx, 1000, isKeepAlive, true);
            } else if (matches(buf, req.path, URI_NOOP)) {
                return ok(ctx, isKeepAlive, new byte[0], MediaType.APPLICATION_OCTET_STREAM);
            } else if (matches(buf, req.path, URI_GEN)) {
                return GenerateKafkaLoad(ctx, 1000, isKeepAlive, false);
            } else if (matches(buf, req.path, URI_TH)) {
                return ok(ctx, isKeepAlive, metricsReporter.getLastThroughput().getBytes(), MediaType.TEXT_PLAIN);
            } else if (matches(buf, req.path, URI_MTT)) {
                return ok(ctx, isKeepAlive, metricsReporter.getLastMeanTravelTime().getBytes(), MediaType.TEXT_PLAIN);
            }
        }
        return HttpStatus.NOT_FOUND;
    }

    private HttpStatus GenerateKafkaLoad(Channel ctx, int eventSize, boolean isKeepAlive, boolean publishToKafka) {
        if (loadGenerator != null) {
            int eventsCount = loadGenerator.produceEvents(eventSize, publishToKafka);
            metricsReporter.produced(eventsCount);
        }
        return ok(ctx, isKeepAlive, new byte[0], MediaType.APPLICATION_OCTET_STREAM);
    }
}