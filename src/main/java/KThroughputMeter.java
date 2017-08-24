import org.rapidoid.log.Log;

import java.text.DecimalFormat;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class KThroughputMeter {
    private final int reportingIntervalSeconds;
    private long totalCount = 0;

    public KThroughputMeter(int reportingIntervalSeconds) {
        this.reportingIntervalSeconds = reportingIntervalSeconds;

        ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
        executor.scheduleAtFixedRate(() -> {
            long localTotalCount;
            synchronized (this) {
                localTotalCount = totalCount;
                totalCount = 0;
            }
            double th = ((double) localTotalCount) / reportingIntervalSeconds;
            Log.info(String.format("KThroughput: %s events/sec", new DecimalFormat("#.##").format(th)));
        }, reportingIntervalSeconds, reportingIntervalSeconds, TimeUnit.SECONDS);
    }

    public void increment(int count) {
        synchronized (this) {
            totalCount += count;
        }
    }
}