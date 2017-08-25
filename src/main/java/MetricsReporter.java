import org.rapidoid.log.Log;

import java.text.DecimalFormat;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class MetricsReporter {
    private long totalCount = 0;
    private long totalTravelTimeMs = 0;
    private double lastThroughput = 0;
    private long lastMeanTravelTimeMs = 0;

    public MetricsReporter(int reportingIntervalSeconds) {
        ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
        executor.scheduleAtFixedRate(() -> {
            synchronized (this) {
                lastThroughput = ((double) totalCount) / reportingIntervalSeconds;
                lastMeanTravelTimeMs = totalCount > 0 ? totalTravelTimeMs / totalCount : 0;
                totalCount = 0;
                totalTravelTimeMs = 0;
            }
            Log.info(String.format("Throughput: %s events/sec; MeanTravelTime: %s ms", getLastThroughput(), getLastMeanTravelTime()));
        }, 0, reportingIntervalSeconds, TimeUnit.SECONDS);
    }

    public void produced(int count) {
        synchronized (this) {
            totalCount += count;
        }
    }

    public void consumed(long travelTimeMs) {
        synchronized (this) {
            totalCount += 1;
            totalTravelTimeMs += travelTimeMs;
        }
    }

    public String getLastThroughput() {
        double localLastThroughput;
        synchronized (this) {
            localLastThroughput = lastThroughput;
        }
        return new DecimalFormat("#.##").format(localLastThroughput);
    }

    public String getLastMeanTravelTime() {
        long localLastTravelTime;
        synchronized (this) {
            localLastTravelTime = lastMeanTravelTimeMs;
        }
        return String.format("%d", localLastTravelTime);
    }
}