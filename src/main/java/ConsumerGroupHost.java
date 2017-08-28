import org.apache.avro.Schema;
import org.rapidoid.log.Log;

import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

public class ConsumerGroupHost {
    private final Schema schema;
    private final Properties props;
    private final String topic;
    private final MetricsReporter metricsReporter;
    private final boolean verboseLogging;
    private final int numConsumers;
    private final int goBackOnRebalanceSeconds;

    public ConsumerGroupHost(Schema schema, Properties props, String topic, MetricsReporter metricsReporter, boolean verboseLogging, int numConsumers, int goBackOnRebalanceSeconds) {
        this.schema = schema;
        this.props = props;
        this.topic = topic;
        this.metricsReporter = metricsReporter;
        this.verboseLogging = verboseLogging;
        this.numConsumers = numConsumers;
        this.goBackOnRebalanceSeconds = goBackOnRebalanceSeconds;
    }

    public void run() {
        Log.info("Start consuming in " + numConsumers + " threads");
        final List<ConsumerLoop> consumers = new ArrayList<>();
        ExecutorService executor = Executors.newFixedThreadPool(numConsumers);
        for (int consumerId = 0; consumerId < numConsumers; consumerId++) {
            ConsumerLoop consumer = new ConsumerLoop(schema, props, topic, metricsReporter, verboseLogging, consumerId, goBackOnRebalanceSeconds);
            consumers.add(consumer);
            executor.submit(consumer);
        }
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            Log.info("Termination requested");
            for (ConsumerLoop consumer : consumers) {
                consumer.shutdown();
            }
            executor.shutdown();
            try {
                executor.awaitTermination(10, TimeUnit.SECONDS);
            } catch (InterruptedException e) {
                Log.error("InterruptedException has been caught ", e);
            }
            Log.info("Now exiting");
        }));
    }
}