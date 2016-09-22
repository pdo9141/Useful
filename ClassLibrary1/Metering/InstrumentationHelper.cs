using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace ClassLibrary1.Metering
{
    public class InstrumentationHelper
    {
        public bool Enabled { get; set; }
        public long OrderId { get; set; }
        public string OrderReferenceNumber { get; set; }
        public int ClientId { get; set; }
        public Guid DocumentId { get; set; }
        public int EventId { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Message { get; set; }
        public int ItemCount { get; set; }
        public string ItemType { get; set; }
        public int MeteringTypeId { get; set; }

        public delegate int MeteringDataStorageProvider(DataTable meteringData, string connectionString, int commandTimeout);

        private static bool globallyEnabled = true;
        private static bool emergencyDisabled = false;
        private static AutoResetEvent startEvent = new AutoResetEvent(false);
        private static ManualResetEvent stopEvent = new ManualResetEvent(false);
        private static Thread backgroundProcessingThread = null;
        private static ConcurrentQueue<InstrumentationHelper> meteringDataQueue = new ConcurrentQueue<InstrumentationHelper>();
        private static int maximumQueueSize = 10000;
        private static int maximumBatchSize = 1000;
        private static TimeSpan queueFlushInterval = TimeSpan.FromMinutes(3);
        private static Timer queueFlushTimer = new Timer(BackgroundProcessingQueueFlush);
        private static int consecutiveSendFailures = 0;
        private static int maximumSendFailures = 3;
        private static object startStopLock = new object();
        private static MeteringDataStorageProvider meteringDataStorageProvider = new MeteringDataStorageProvider(MeteringDataStorageSqlProvider);
        
        private static string databaseConnection;
        private static int commandTimeout;
        private static string machineName = String.Empty;
        private static string processName = String.Empty;

        static InstrumentationHelper()
        {
            try
            {
                // Initialize app config values here
                databaseConnection = "app value";
                commandTimeout = 0;

                SetMachineAndProcessNames();
                StartBackgroundProcessing();
            }
            catch (Exception ex)
            {
            }
        }
        
        private static void StartBackgroundProcessing()
        {
            lock (startStopLock)
            {
                if (backgroundProcessingThread == null)
                {
                    backgroundProcessingThread = new Thread(new ThreadStart(BackgroundProcessingThreadFunction));
                    backgroundProcessingThread.Name = "MeteringBackgroundProcessing";
                    backgroundProcessingThread.IsBackground = true;
                    backgroundProcessingThread.Start();

                    queueFlushTimer.Change(queueFlushInterval, queueFlushInterval);
                }
            }
        }

        public static void StopBackgroundProcessing()
        {
            lock (startStopLock)
            {
                if (backgroundProcessingThread != null)
                {
                    queueFlushTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    stopEvent.Set();
                    backgroundProcessingThread.Join();
                    backgroundProcessingThread = null;
                }
            }
        }

        private static void BackgroundProcessingThreadFunction()
        {
            var waitHandles = new WaitHandle[] { startEvent, stopEvent };
            for (;;)
            {
                try
                {
                    WaitHandle.WaitAny(waitHandles);

                    if (meteringDataQueue.Count > 0)
                        SendMeteringDataToStorageProvider();

                    if (stopEvent.WaitOne(0))
                    {
                        stopEvent.Reset();
                        break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private static void SendMeteringDataToStorageProvider()
        {
            var batch = new DataTable();
            batch.Columns.Add("", typeof(int));
            batch.Columns.Add("", typeof(int));
            batch.Columns.Add("", typeof(int));
            batch.Columns.Add("", typeof(int));

            InstrumentationHelper meteringData;
            var batchRecovery = new Queue<InstrumentationHelper>();

            do
            {
                batch.Clear();
                batchRecovery.Clear();

                while (batch.Rows.Count < maximumBatchSize && meteringDataQueue.TryDequeue(out meteringData))
                {
                    batchRecovery.Enqueue(meteringData);
                    batch.Rows.Add(
                        meteringData.MeteringTypeId,
                        meteringData.OrderId,
                        meteringData.EventId,
                        meteringData.OrderReferenceNumber);
                }

                if (batch.Rows.Count > 0)
                {
                    try
                    {
                        if (batch.Rows.Count != meteringDataStorageProvider(batch, databaseConnection, commandTimeout))
                            throw new ApplicationException("");

                        consecutiveSendFailures = 0;
                    }
                    catch (Exception ex)
                    {

                        if (consecutiveSendFailures < maximumSendFailures)
                        {
                            while (batchRecovery.Count > 0)
                            {
                                meteringDataQueue.Enqueue(batchRecovery.Dequeue());
                            }
                        }
                        else
                        {
                        }

                        consecutiveSendFailures = Math.Min(consecutiveSendFailures + 1, maximumSendFailures);
                    }
                }
            }
            while (consecutiveSendFailures == 0 && batch.Rows.Count == maximumBatchSize);
        }

        private static void SetMachineAndProcessNames()
        {
            machineName = Environment.MachineName ?? String.Empty;
            processName = Process.GetCurrentProcess().ProcessName ?? String.Empty;
        }

        public static MeteringDataStorageProvider ReplaceMeteringDataStorageProvider(MeteringDataStorageProvider newMeteringDataStorageProvider)
        {
            MeteringDataStorageProvider oldMeteringDataStorageProvider = meteringDataStorageProvider;
            meteringDataStorageProvider = newMeteringDataStorageProvider;

            return oldMeteringDataStorageProvider;
        }

        public void StartTimer(Stopwatch sw)
        {
            if (globallyEnabled || Enabled)
                sw.Start();
        }

        public void WriteToConsole(Stopwatch sw, string message, MeteringTypes meteringType = MeteringTypes.Unspecified, int itemCount = 1, string itemType = null)
        {
            try
            {
                if (globallyEnabled || Enabled)
                {
                    sw.Stop();
                    ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    Message = message;
                    ItemCount = itemCount;
                    ItemType = itemType;
                    MeteringTypeId = (int)meteringType;

                    EnqueueMeteringData(this);
                    sw.Reset();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public long GetElaspsedMilliseconds(Stopwatch sw)
        {
            if (globallyEnabled || Enabled)
            {
                sw.Stop();
                ElapsedMilliseconds = sw.ElapsedMilliseconds;
                sw.Reset();
            }

            return ElapsedMilliseconds;
        }

        public static void AddMeteringData(long orderId, int eventId, string orderReferenceNumber, int clientId, Guid documentId, long elapsedMilliseconds, string message, int itemCount = 1, string itemType = null)
        {
            EnqueueMeteringData(new InstrumentationHelper
            {
                OrderId = orderId,
                EventId = eventId,
                OrderReferenceNumber = orderReferenceNumber,
                ClientId = clientId,
                DocumentId = documentId,
                ElapsedMilliseconds = elapsedMilliseconds,
                Message = message,
                ItemCount = itemCount,
                ItemType = itemType
            });
        }

        private static void EnqueueMeteringData(InstrumentationHelper meteringData)
        {
            if (emergencyDisabled)
                return;

            meteringDataQueue.Enqueue(meteringData);

            if (meteringDataQueue.Count >= maximumQueueSize)
            {
                queueFlushTimer.Change(queueFlushInterval, queueFlushInterval);
                startEvent.Set();
            }
        }

        private static void BackgroundProcessingQueueFlush(object state)
        {
            startEvent.Set();
        }

        private static int MeteringDataStorageSqlProvider(DataTable meteringData, string connectionString, int commandTimeout)
        {
            using (var connection = new SqlConnection(databaseConnection))
            {
                using (var command = new SqlCommand("MeteringEntries_InsertBatch", connection))
                {
                    connection.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;

                    var parameter = command.Parameters.AddWithValue("@BatchEntries", meteringData);
                    parameter.SqlDbType = SqlDbType.Structured;

                    return command.ExecuteNonQuery();
                }
            }
        }
    }

    public enum MeteringTypes
    {
        Unspecified
    }
}
