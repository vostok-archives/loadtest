import org.apache.avro.Schema;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.rapidoid.net.Server;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.Properties;

public class HttpGateEntryPoint {

    public static void main(String[] args) throws Exception {
        Properties props = new Properties();
        props.put("bootstrap.servers", "edi15:9092");
        props.put("acks", "all");
        props.put("retries", 0);
        props.put("key.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");
        props.put("value.serializer", "io.confluent.kafka.serializers.KafkaAvroSerializer");
        props.put("schema.registry.url", "http://edi15:8881");
        String schemaString = "{\"type\": \"record\", " +
                "\"name\": \"kevent\"," +
                "\"fields\": [" +
                "{\"name\": \"timestamp\", \"type\": \"long\"}," +
                "{\"name\": \"payload\", \"type\": \"bytes\"}" +
                "]}";
        Producer<String, GenericRecord> producer = new KafkaProducer<>(props);
        Schema.Parser parser = new Schema.Parser();
        Schema schema = parser.parse(schemaString);
        Server server = new KHttpServer(schema, producer).listen(8888);
        new BufferedReader(new InputStreamReader(System.in)).readLine();
        server.shutdown();
        producer.close();
    }
}