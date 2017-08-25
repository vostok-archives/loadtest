import org.apache.avro.Schema;
import org.apache.avro.generic.GenericData;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.nio.ByteBuffer;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ThreadLocalRandom;

public class LoadGenerator {
    private final Schema schema;
    private final Producer<String, GenericRecord> producer;
    private final String topic;
    private final int eventBatchSize;
    private byte[] randomBytesSource;

    LoadGenerator(Schema schema, Producer<String, GenericRecord> producer, String topic, int eventBatchSize) {
        this.schema = schema;
        this.producer = producer;
        this.topic = topic;
        this.eventBatchSize = eventBatchSize;
        Random random = new Random(UUID.randomUUID().hashCode());
        this.randomBytesSource = new byte[Integer.MAX_VALUE - 5];
        random.nextBytes(randomBytesSource);
    }

    public int produceEvents(int eventSize, boolean publishToKafka) {
        long timestamp = System.currentTimeMillis();
        for (long i = 0; i < eventBatchSize; i++) {
            GenericRecord kevent = new GenericData.Record(schema);
            kevent.put("timestamp", timestamp);
            kevent.put("payload", generatePayload(eventSize));
            ProducerRecord<String, GenericRecord> data = new ProducerRecord<>(topic, kevent);
            if (publishToKafka)
                producer.send(data);
        }
        return eventBatchSize;
    }

    private ByteBuffer generatePayload(int eventSize) {
        int offset = ThreadLocalRandom.current().nextInt(0, randomBytesSource.length - eventSize);
        return ByteBuffer.wrap(randomBytesSource, offset, eventSize).slice();
    }
}