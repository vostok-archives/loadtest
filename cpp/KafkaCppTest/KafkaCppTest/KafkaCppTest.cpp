// KafkaCppTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <librdkafka\rdkafka.h>
#include <time.h>
#include <windows.h> //I've ommited this line.
#if defined(_MSC_VER) || defined(_MSC_EXTENSIONS)
#define DELTA_EPOCH_IN_MICROSECS  11644473600000000Ui64
#else
#define DELTA_EPOCH_IN_MICROSECS  11644473600000000ULL
#endif

struct timezone
{
    int  tz_minuteswest; /* minutes W of Greenwich */
    int  tz_dsttime;     /* type of dst correction */
};

int gettimeofday(struct timeval *tv)
{
    FILETIME ft;
    unsigned __int64 tmpres = 0;
    static int tzflag;

    if (NULL != tv)
    {
        GetSystemTimeAsFileTime(&ft);

        tmpres |= ft.dwHighDateTime;
        tmpres <<= 32;
        tmpres |= ft.dwLowDateTime;

        /*converting file time to unix epoch*/
        tmpres -= DELTA_EPOCH_IN_MICROSECS;
        tmpres /= 10;  /*convert into microseconds*/
        tv->tv_sec = (long)(tmpres / 1000000UL);
        tv->tv_usec = (long)(tmpres % 1000000UL);
    }

    return 0;
}

static int run = 1;
int counter = 0;
/**
* @brief Signal termination of program
*/
static void stop(int sig) {
    run = 0;
    fclose(stdin); /* abort fgets() */
}


/**
* @brief Message delivery report callback.
*
* This callback is called exactly once per message, indicating if
* the message was succesfully delivered
* (rkmessage->err == RD_KAFKA_RESP_ERR_NO_ERROR) or permanently
* failed delivery (rkmessage->err != RD_KAFKA_RESP_ERR_NO_ERROR).
*
* The callback is triggered from rd_kafka_poll() and executes on
* the application's thread.
*/

static void dr_msg_cb(rd_kafka_t *rk,
    const rd_kafka_message_t *rkmessage, void *opaque) {
    if (rkmessage->err)
        fprintf(stdout, "%% Message delivery failed: %s\n",
            rd_kafka_err2str(rkmessage->err));
    else {
        counter++;
        //fprintf(stdout, "%% Message delivered (%zd bytes, partition %d)\n", rkmessage->len, rkmessage->partition);
    }

    /* The rkmessage is destroyed automatically by librdkafka */
}

double get_sec() {
    struct timeval tp;
    gettimeofday(&tp);
    return (double)(tp.tv_sec * 1000 + tp.tv_usec / 1000) / 1000;
}

int main(int argc, char **argv)
{
    rd_kafka_t *rk;         /* Producer instance handle */
    rd_kafka_topic_t *rkt;  /* Topic object */
    rd_kafka_conf_t *conf;  /* Temporary configuration object */
    char errstr[512];       /* librdkafka API error reporting buffer */
    char buf[512];          /* Message value temporary buffer */
    const char *brokers;    /* Argument: broker list */
    const char *topic;      /* Argument: topic to produce to */

                            /*
                            * Argument validation
                            */
    brokers = "127.0.0.1:9092";
    topic = "cpptopic";


    /*
    * Create Kafka client configuration place-holder
    */
    conf = rd_kafka_conf_new();

    /* Set bootstrap broker(s) as a comma-separated list of
    * host or host:port (default port 9092).
    * librdkafka will use the bootstrap brokers to acquire the full
    * set of brokers from the cluster. */
    if (rd_kafka_conf_set(conf, "bootstrap.servers", brokers,
        errstr, sizeof(errstr)) != RD_KAFKA_CONF_OK) {
        fprintf(stdout, "%s\n", errstr);
        return 1;
    }

    /* Set the delivery report callback.
    * This callback will be called once per message to inform
    * the application if delivery succeeded or failed.
    * See dr_msg_cb() above. */
    rd_kafka_conf_set_dr_msg_cb(conf, dr_msg_cb);


    /*
    * Create producer instance.
    *
    * NOTE: rd_kafka_new() takes ownership of the conf object
    *       and the application must not reference it again after
    *       this call.
    */
    rk = rd_kafka_new(RD_KAFKA_PRODUCER, conf, errstr, sizeof(errstr));
    if (!rk) {
        fprintf(stdout,
            "%% Failed to create new producer: %s\n", errstr);
        return 1;
    }


    /* Create topic object that will be reused for each message
    * produced.
    *
    * Both the producer instance (rd_kafka_t) and topic objects (topic_t)
    * are long-lived objects that should be reused as much as possible.
    */
    rkt = rd_kafka_topic_new(rk, topic, NULL);
    if (!rkt) {
        fprintf(stdout, "%% Failed to create topic object: %s\n",
            rd_kafka_err2str(rd_kafka_last_error()));
        rd_kafka_destroy(rk);
        return 1;
    }

    /* Signal handler for clean shutdown */
    //signal(SIGINT, stop);

    fprintf(stdout,
        "%% Type some text and hit enter to produce message\n"
        "%% Or just hit enter to only serve delivery reports\n"
        "%% Press Ctrl-C or Ctrl-D to exit\n");

    char * msg = "1234567890";

    double sec = get_sec();

    for (int i = 0; i < 100000000; i++) {
        size_t len = strlen(msg);

retry:
        if (rd_kafka_produce(
            /* Topic object */
            rkt,
            /* Use builtin partitioner to select partition*/
            RD_KAFKA_PARTITION_UA,
            /* Make a copy of the payload. */
            RD_KAFKA_MSG_F_COPY,
            /* Message payload (value) and length */
            msg, len,
            /* Optional key and its length */
            NULL, 0,
            /* Message opaque, provided in
            * delivery report callback as
            * msg_opaque. */
            NULL) == -1) {
            /**
            * Failed to *enqueue* message for producing.
            */
            fprintf(stdout,
                "%% Failed to produce to topic %s: %s\n",
                rd_kafka_topic_name(rkt),
                rd_kafka_err2str(rd_kafka_last_error()));

            /* Poll to handle delivery reports */
            if (rd_kafka_last_error() ==
                RD_KAFKA_RESP_ERR__QUEUE_FULL) {
                /* If the internal queue is full, wait for
                * messages to be delivered and then retry.
                * The internal queue represents both
                * messages to be sent and messages that have
                * been sent or failed, awaiting their
                * delivery report callback to be called.
                *
                * The internal queue is limited by the
                * configuration property
                * queue.buffering.max.messages */
                rd_kafka_poll(rk, 1000/*block for max 1000ms*/);
                goto retry;
            }
        }
        //else {
        //    fprintf(stdout, "%% Enqueued message (%zd bytes) "
        //        "for topic %s\n",
        //        len, rd_kafka_topic_name(rkt));
        //}
        rd_kafka_poll(rk, 0);
        if (i % 1000000 == 0) {
            printf("i = %d, counter=%d\n", i, counter);
        }
    }

    double sec2 = get_sec();
    printf("diff sec = %f\n", sec2 - sec);

    /* Wait for final messages to be delivered or fail.
    * rd_kafka_flush() is an abstraction over rd_kafka_poll() which
    * waits for all messages to be delivered. */
    fprintf(stdout, "%% Flushing final messages..\n");
    rd_kafka_flush(rk, 10 * 1000 /* wait for max 10 seconds */);
    printf("counter=%d\n", counter);

    /* Destroy topic object */
    rd_kafka_topic_destroy(rkt);

    /* Destroy the producer instance */
    rd_kafka_destroy(rk);

    return 0;
}

