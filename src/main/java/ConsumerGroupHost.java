import org.apache.avro.Schema;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
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

public class ConsumerGroupHost {
    private final Schema schema;
    private final Properties props;
    private final String topic;
    private final MetricsReporter metricsReporter;
    private final boolean verboseLogging;

    public ConsumerGroupHost(Schema schema, Properties props, String topic, MetricsReporter metricsReporter, boolean verboseLogging) {
        this.schema = schema;
        this.props = props;
        this.topic = topic;
        this.metricsReporter = metricsReporter;
        this.verboseLogging = verboseLogging;
    }

    public void run() {
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
                    if (verboseLogging) {
                        Log.info("[" + lastRecord.partition() + ":" + lastRecord.offset() + "]:"
                                + " ts=" + formatTimestamp(lastRecord)
                                + " key=" + lastRecord.key()
                                + " v.ts=" + lastRecord.value().get("timestamp")
                                + " v.size=" + lastRecord.serializedValueSize()
                                + " tt=" + formatDuration(lastTravelTime)
                                + " RC=" + records.count());
                    }
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
}