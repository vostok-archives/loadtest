import org.apache.avro.Schema;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.record.TimestampType;
import org.rapidoid.log.Log;

import java.util.Arrays;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

public class ConsumerLoop implements Runnable {
    private final Schema schema;
    private final Properties props;
    private final String topic;
    private final MetricsReporter metricsReporter;
    private final boolean verboseLogging;
    private final int consumerId;
    private final int goBackOnRebalanceSeconds;
    private final KafkaConsumer<String, GenericRecord> consumer;

    public ConsumerLoop(Schema schema, Properties props, String topic, MetricsReporter metricsReporter, boolean verboseLogging, int consumerId, int goBackOnRebalanceSeconds) {
        this.schema = schema;
        this.props = props;
        this.topic = topic;
        this.metricsReporter = metricsReporter;
        this.verboseLogging = verboseLogging;
        this.consumerId = consumerId;
        this.goBackOnRebalanceSeconds = goBackOnRebalanceSeconds;
        consumer = new KafkaConsumer<>(props);
    }

    private static String formatTimestamp(ConsumerRecord<String, GenericRecord> record) {
        TimestampType timestampType = record.timestampType();
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
        return record.timestamp() + " " + timestampTypeStr;
    }

    private static String formatDuration(long durationMillis) {
        long totalMinutes = TimeUnit.MILLISECONDS.toMinutes(durationMillis);
        long totalSeconds = TimeUnit.MILLISECONDS.toSeconds(durationMillis);
        long seconds = totalSeconds - TimeUnit.MINUTES.toSeconds(totalMinutes);
        long millis = durationMillis - TimeUnit.SECONDS.toMillis(totalSeconds);
        return String.format("%d:%02d:%03d", totalMinutes, seconds, millis);
    }

    @Override
    public void run() {
        try {
            consumer.subscribe(Arrays.asList(topic), new GoBackOnRebalance(consumer, consumerId, goBackOnRebalanceSeconds));
            while (true) {
                ConsumerRecords<String, GenericRecord> records = consumer.poll(Long.MAX_VALUE);
                long currentTimestamp = System.currentTimeMillis();
                ConsumerRecord<String, GenericRecord> lastRecord = null;
                long lastTravelTime = 0;
                for (ConsumerRecord<String, GenericRecord> record : records) {
                    lastRecord = record;
                    lastTravelTime = currentTimestamp - (long) lastRecord.value().get("timestamp");
                    int  payloadSize = ((byte[])lastRecord.value().get("payload")).length;
                    metricsReporter.consumed(lastTravelTime, payloadSize);
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
            Log.info("Shutdown requested for consumer " + consumerId);
        } finally {
            consumer.close();
            Log.info("Consumer " + consumerId + " closed");
        }
    }

    public void shutdown() {
        consumer.wakeup();
    }
}
