import org.apache.avro.Schema;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.rapidoid.log.Log;
import org.rapidoid.net.Server;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Properties;

public class KLoadEntryPoint {

    public static void main(String[] args) throws Exception {
        Properties props = new Properties();
        props.put("bootstrap.servers", "icat-test01:9092");
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
        props.put("schema.registry.url", "http://icat-test01:8881");
        String schemaString = "{\"type\": \"record\", " +
                "\"name\": \"kevent\"," +
                "\"fields\": [" +
                "{\"name\": \"timestamp\", \"type\": \"long\"}," +
                "{\"name\": \"payload\", \"type\": \"bytes\"}" +
                "]}";
        Schema.Parser parser = new Schema.Parser();
        Schema schema = parser.parse(schemaString);
        if (args != null && args.length > 0) {
            String mode = args[0];
            if (mode == "gate")
                RunHttpGate(props, schema);
            else if (mode == "consumer")
                RunConsumer(props, schema);
            else
                Log.error("KLoad mode is not recognized: " + mode);
        }else{
            Log.error("KLoad mode is not specified");
        }
    }

    private static void RunConsumer(Properties props, Schema schema) {
        Log.info("Starting consumer");
    }

    private static void RunHttpGate(Properties props, Schema schema) throws IOException {
        Log.info("Starting http gate");
        Producer<String, GenericRecord> producer = new KafkaProducer<>(props);
        Server server = new KHttpServer(schema, producer).listen(8888);
        new BufferedReader(new InputStreamReader(System.in)).readLine();
        server.shutdown();
        producer.close();
    }
}