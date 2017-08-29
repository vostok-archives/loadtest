import org.rapidoid.log.Log;

import java.text.DecimalFormat;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class MetricsReporter {
    private long totalCount = 0;
    private long totalSize = 0;
    private long totalTravelTimeMs = 0;
    private double lastThroughput = 0;
    private long lastThroughputBytes = 0;
    private long lastMeanTravelTimeMs = 0;

    public MetricsReporter(int reportingIntervalSeconds) {
        ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
        executor.scheduleAtFixedRate(() -> {
            synchronized (this) {
                lastThroughput = ((double) totalCount) / reportingIntervalSeconds;
                lastThroughputBytes = totalSize / reportingIntervalSeconds;
                lastMeanTravelTimeMs = totalCount > 0 ? totalTravelTimeMs / totalCount : 0;
                totalCount = 0;
                totalSize = 0;
                totalTravelTimeMs = 0;
            }
            Log.info(String.format("Thr-put: %s events/sec; Thr-putMb: %s Mb/sec; MeanTrTime: %s ms", getLastThroughput(), getLastThroughputMb(), getLastMeanTravelTime()));
        }, 0, reportingIntervalSeconds, TimeUnit.SECONDS);
    }

    public void produced(int count, int size) {
        synchronized (this) {
            totalCount += count;
            totalSize += count * size;
        }
    }

    public void consumed(long travelTimeMs, int size) {
        synchronized (this) {
            totalCount += 1;
            totalSize += size;
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

    public String getLastThroughputMb() {
        double localLastThroughputBytes;
        synchronized (this) {
            localLastThroughputBytes = lastThroughputBytes;
        }
        return new DecimalFormat("#.##").format(localLastThroughputBytes / 1000 / 1000);
    }

    public String getLastMeanTravelTime() {
        long localLastTravelTime;
        synchronized (this) {
            localLastTravelTime = lastMeanTravelTimeMs;
        }
        return String.format("%d", localLastTravelTime);
    }
}