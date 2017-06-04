import org.apache.avro.Schema;
import org.apache.avro.generic.GenericData;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.rapidoid.buffer.Buf;
import org.rapidoid.http.AbstractHttpServer;
import org.rapidoid.http.HttpStatus;
import org.rapidoid.http.MediaType;
import org.rapidoid.net.abstracts.Channel;
import org.rapidoid.net.impl.RapidoidHelper;

import java.nio.ByteBuffer;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ThreadLocalRandom;

public class KHttpServer extends AbstractHttpServer {

    private static final byte[] URI_HELLO = "/hello".getBytes();
    private static final byte[] URI_NOOP = "/noop".getBytes();
    private static final byte[] URI_GEN = "/gen".getBytes();
    private static final byte[] URI_KLOAD_10 = "/kload10".getBytes();
    private static final byte[] URI_KLOAD_100 = "/kload100".getBytes();
    private static final byte[] URI_KLOAD_1000 = "/kload1000".getBytes();
    private static final byte[] HELLO_WORLD = "Hello, World!".getBytes();
    private final Schema schema;
    private final Producer<String, GenericRecord> producer;
    private byte[] randomBytesSource;

    public KHttpServer(Schema schema, Producer<String, GenericRecord> producer) {
        this.schema = schema;
        this.producer = producer;
        Random random = new Random(UUID.randomUUID().hashCode());
        this.randomBytesSource = new byte[Integer.MAX_VALUE - 5];
        random.nextBytes(randomBytesSource);
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
            } else if (matches(buf, req.path, URI_HELLO)) {
                return ok(ctx, isKeepAlive, HELLO_WORLD, MediaType.TEXT_PLAIN);
            }
        }
        return HttpStatus.NOT_FOUND;
    }

    private HttpStatus GenerateKafkaLoad(Channel ctx, int eventSize, boolean isKeepAlive, boolean publishToKafka) {
        produceEvents(eventSize, publishToKafka);
        return ok(ctx, isKeepAlive, new byte[0], MediaType.APPLICATION_OCTET_STREAM);
    }

    private void produceEvents(int eventSize, boolean publishToKafka) {
        long timestamp = System.currentTimeMillis();
        for (long nEvents = 0; nEvents < 100; nEvents++) {
            GenericRecord kevent = new GenericData.Record(schema);
            kevent.put("timestamp", timestamp);
            kevent.put("payload", generatePayload(eventSize));
            ProducerRecord<String, GenericRecord> data = new ProducerRecord<>("ktopic", kevent);
            if (publishToKafka)
                producer.send(data);
        }
    }

    public ByteBuffer generatePayload(int eventSize) {
        int offset = ThreadLocalRandom.current().nextInt(0, randomBytesSource.length - eventSize);
        return ByteBuffer.wrap(randomBytesSource, offset, eventSize).slice();
    }
}