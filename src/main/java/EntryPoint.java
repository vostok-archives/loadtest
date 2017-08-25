import ch.qos.logback.classic.Level;
import org.apache.avro.Schema;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.consumer.*;
import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.record.TimestampType;
import org.rapidoid.log.Log;
import org.rapidoid.net.Server;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Arrays;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

public class EntryPoint {

    public static void main(String[] args) throws Exception {
        ((ch.qos.logback.classic.Logger) org.slf4j.LoggerFactory.getLogger("org.apache.kafka")).setLevel(Level.INFO);

        Properties props = new Properties();
        props.put("bootstrap.servers", "icat-test01:9092");
        props.put("schema.registry.url", "http://icat-test01:8881");
        String schemaString = "{\"type\": \"record\", " +
                "\"name\": \"kevent\"," +
                "\"fields\": [" +
                "{\"name\": \"timestamp\", \"type\": \"long\"}," +
                "{\"name\": \"payload\", \"type\": \"bytes\"}" +
                "]}";
        Schema.Parser parser = new Schema.Parser();
        Schema schema = parser.parse(schemaString);
        String topic = "ktopic-with-ts";
        MetricsReporter metricsReporter = new MetricsReporter(3);
        if (args != null && args.length > 0) {
            String mode = args[0];
            if (mode.equals("gate"))
                RunHttpGate(props, schema, topic, metricsReporter);
            else if (mode.equals("consumer"))
                RunConsumerGroup(props, schema, topic, metricsReporter);
            else
                Log.error("KLoad mode is not recognized: " + mode);
        } else {
            Log.error("KLoad mode is not specified");
        }
    }

    private static void RunConsumerGroup(Properties props, Schema schema, String topic, MetricsReporter metricsReporter) {
        Log.info("Starting consumer");

        props.put("group.id", "kgroup");
        props.put("auto.offset.reset", "latest");
        props.put("enable.auto.commit", "true");
        props.put("auto.commit.interval.ms", 1000);
        props.put("session.timeout.ms", 60000);
        props.put("max.poll.records", 64 * 1000);
        props.put("max.partition.fetch.bytes", 1048576);
        props.put("fetch.min.bytes", 1);
        props.put("fetch.max.bytes", 52428800);
        props.put("fetch.max.wait.ms", 500);
        props.put("key.deserializer", "io.confluent.kafka.serializers.KafkaAvroDeserializer");
        props.put("value.deserializer", "io.confluent.kafka.serializers.KafkaAvroDeserializer");

        KafkaConsumer<String, GenericRecord> consumer = new KafkaConsumer<>(props);

        Object lock = new Object();
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            Log.info("Termination requested");
            consumer.wakeup();
            synchronized (lock) {
                Log.info("Lock acquired"); // it's essential to do something inside synchronized block!
            }
            Log.info("Now exiting");
        }));

        consumer.subscribe(Arrays.asList(topic), new GoBackOnRebalance(consumer, 30));
        synchronized (lock) {
            try {
                while (true) {
                    ConsumerRecords<String, GenericRecord> records = consumer.poll(Long.MAX_VALUE);
                    long currentTimestamp = System.currentTimeMillis();
                    ConsumerRecord<String, GenericRecord> lastRecord = null;
                    long lastTravelTime = 0;
                    for (ConsumerRecord<String, GenericRecord> record : records) {
                        lastRecord = record;
                        lastTravelTime = currentTimestamp - (long) lastRecord.value().get("timestamp");
                        metricsReporter.consumed(lastTravelTime);
                    }
                    Log.debug("[" + lastRecord.partition() + ":" + lastRecord.offset() + "]:"
                            + " ts=" + formatTimestamp(lastRecord)
                            + " key=" + lastRecord.key()
                            + " v.ts=" + lastRecord.value().get("timestamp")
                            + " v.size=" + lastRecord.serializedValueSize()
                            + " tt=" + formatDuration(lastTravelTime)
                            + " RC=" + records.count());

                }
            } catch (WakeupException e) {
                // ignore for shutdown via consumer.wakeup()
                Log.info("Consumer waked up");
            } finally {
                consumer.close();
                Log.info("Consumer closed");
            }
        }
    }

    private static String formatTimestamp(ConsumerRecord<String, GenericRecord> lastRecord) {
        TimestampType timestampType = lastRecord.timestampType();
        String timestampTypeStr = null;
        switch (timestampType) {
            case NO_TIMESTAMP_TYPE:
                timestampTypeStr = "NONE";
                break;
            case CREATE_TIME:
                timestampTypeStr = "CT";
                break;
            case LOG_APPEND_TIME:
                timestampTypeStr = "LAT";
                break;
        }
        return lastRecord.timestamp() + " " + timestampTypeStr;
    }

    private static String formatDuration(long durationMillis) {
        long totalMinutes = TimeUnit.MILLISECONDS.toMinutes(durationMillis);
        long totalSeconds = TimeUnit.MILLISECONDS.toSeconds(durationMillis);
        long seconds = totalSeconds - TimeUnit.MINUTES.toSeconds(totalMinutes);
        long millis = durationMillis - TimeUnit.SECONDS.toMillis(totalSeconds);
        return String.format("%d:%02d:%03d", totalMinutes, seconds, millis);
    }

    private static void RunHttpGate(Properties props, Schema schema, String topic, MetricsReporter metricsReporter) throws IOException {
        Log.info("Starting http gate");

        props.put("acks", "1");
        props.put("retries", 0);
        props.put("linger.ms", 20);
        props.put("batch.size", 64 * 1000);
        props.put("buffer.memory", 256 * 1000 * 1000);
        props.put("max.request.size", 20 * 1000 * 1000);
        props.put("compression.type", "none");
        props.put("metadata.fetch.timeout.ms", 25);
        props.put("max.block.ms", 25);
        props.put("max.in.flight.requests.per.connection", 500);
        props.put("key.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");
        props.put("value.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");

        Producer<String, GenericRecord> producer = new KafkaProducer<>(props);
        LoadGenerator loadGenerator = new LoadGenerator(schema, producer, topic, 100);
        Server server = new HttpServer(metricsReporter, loadGenerator).listen(8888);
        new BufferedReader(new InputStreamReader(System.in)).readLine();
        server.shutdown();
        producer.close();
    }
}