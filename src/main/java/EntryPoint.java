import ch.qos.logback.classic.Level;
import org.apache.avro.Schema;
import org.rapidoid.log.Log;
import org.rapidoid.net.Server;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Properties;

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
            else if (mode.equals("consumer")) {
                int numConsumers = 3;
                if (args.length > 1)
                    numConsumers = Integer.parseInt(args[1]);
                int goBackOnRebalanceSeconds = 30;
                if (args.length > 2)
                    goBackOnRebalanceSeconds = Integer.parseInt(args[2]);
                RunConsumerGroup(props, schema, topic, metricsReporter, numConsumers, goBackOnRebalanceSeconds);
            } else
                Log.error("KLoad mode is not recognized: " + mode);
        } else {
            Log.error("KLoad mode is not specified");
        }
    }

    private static void RunHttpGate(Properties props, Schema schema, String topic, MetricsReporter metricsReporter) throws IOException {
        Log.info("Starting http gate");

        props.put("acks", "1");
        props.put("retries", 0);
        props.put("linger.ms", 20);
        props.put("batch.size", 128 * 1000);
        props.put("buffer.memory", 256 * 1000 * 1000);
        props.put("max.request.size", 20 * 1000 * 1000);
        props.put("compression.type", "none");
        props.put("metadata.fetch.timeout.ms", 25);
        props.put("max.block.ms", 25);
        props.put("max.in.flight.requests.per.connection", 1000);
        props.put("key.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");
        props.put("value.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");

        LoadGenerator loadGenerator = new LoadGenerator(schema, props, topic, 100);
        Server httpServer = new HttpServer(metricsReporter, loadGenerator).listen(8888);
        new BufferedReader(new InputStreamReader(System.in)).readLine();
        httpServer.shutdown();
        loadGenerator.shutdown();
    }

    private static void RunConsumerGroup(Properties props, Schema schema, String topic, MetricsReporter metricsReporter, int numConsumers, int goBackOnRebalanceSeconds) throws IOException {
        Log.info("Starting consumer group");

        props.put("group.id", "kgroup");
        props.put("auto.offset.reset", "latest");
        props.put("enable.auto.commit", "true");
        props.put("auto.commit.interval.ms", 1000);
        props.put("session.timeout.ms", 60000);
        props.put("max.poll.records", 128 * 1000);
        props.put("max.partition.fetch.bytes", 10485760);
        props.put("fetch.min.bytes", 1);
        props.put("fetch.max.bytes", 52428800);
        props.put("fetch.max.wait.ms", 500);
        props.put("key.deserializer", "io.confluent.kafka.serializers.KafkaAvroDeserializer");
        props.put("value.deserializer", "io.confluent.kafka.serializers.KafkaAvroDeserializer");

        ConsumerGroupHost consumerGroupHost = new ConsumerGroupHost(schema, props, topic, metricsReporter, false, numConsumers, goBackOnRebalanceSeconds);
        consumerGroupHost.run();
        Server httpServer = new HttpServer(metricsReporter, null).listen(8889);
        new BufferedReader(new InputStreamReader(System.in)).readLine();
        httpServer.shutdown();
    }
}