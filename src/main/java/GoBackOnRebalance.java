import org.apache.kafka.clients.consumer.Consumer;
import org.apache.kafka.clients.consumer.ConsumerRebalanceListener;
import org.apache.kafka.clients.consumer.OffsetAndTimestamp;
import org.apache.kafka.common.TopicPartition;
import org.rapidoid.log.Log;

import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.concurrent.TimeUnit;

public class GoBackOnRebalance implements ConsumerRebalanceListener {
    private final int consumerId;
    private final int seconds;
    private Consumer<?, ?> consumer;

    public GoBackOnRebalance(Consumer<?, ?> consumer, int consumerId, int seconds) {
        this.consumer = consumer;
        this.consumerId = consumerId;
        this.seconds = seconds;
    }

    public void onPartitionsRevoked(Collection<TopicPartition> partitions) {
    }

    public void onPartitionsAssigned(Collection<TopicPartition> partitions) {
        long startTimestamp = System.currentTimeMillis() - TimeUnit.SECONDS.toMillis(seconds);
        for (TopicPartition topicPartition : partitions) {
            OffsetAndTimestamp offsetAndTimestamp = consumer.offsetsForTimes(Collections.singletonMap(topicPartition, startTimestamp)).get(topicPartition);
            long offset;
            if (offsetAndTimestamp != null) {
                offset = offsetAndTimestamp.offset();
                Log.info("Rewind consumer " + consumerId + " for " + topicPartition + " to " + offsetAndTimestamp);
            } else {
                offset = consumer.endOffsets(Arrays.asList(topicPartition)).get(topicPartition);
                Log.info("Rewind consumer " + consumerId + " for " + topicPartition + " to the end");
            }
            consumer.seek(topicPartition, offset);
        }
    }
}
