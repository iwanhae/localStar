using localStar.Logger;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace localStar.Connection
{
    public static class HandleLoop
    {
        const int HOW_MANY_WORKER = 10;
        private static ConcurrentQueue<Func<JobStatus>> jobQueue = new ConcurrentQueue<Func<JobStatus>>();
        private static Thread[] worker = new Thread[HOW_MANY_WORKER];
        public static void Init()
        {
            for (int i = 0; i < HOW_MANY_WORKER; i++)
            {
                worker[i] = new Thread(handleWorker);
                worker[i].Start();
            }
        }

        public static int getCount() => jobQueue.Count;
        public static void addJob(Func<JobStatus> job)
        {
            if (job != null) jobQueue.Enqueue(job);
        }
        public static void handleWorker()
        {
            int pendingCounter = 0;
            int Counter = 0;
            while (true)
            {
                Func<JobStatus> job = null;
                if (jobQueue.TryDequeue(out job))
                {
                    Counter++;
                    try
                    {
                        JobStatus result = job();
                        switch (result)
                        {
                            case JobStatus.Good:
                                jobQueue.Enqueue(job);
                                break;
                            case JobStatus.Pending:
                                jobQueue.Enqueue(job);
                                pendingCounter++;
                                break;
                            case JobStatus.Failed:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.error("Loop : Unexpected Exception : {0}", e);
                    }
                    if (jobQueue.Count < Counter)
                    {
                        if (pendingCounter == Counter)
                        {
                            Thread.Sleep(10);
                        }
                        Counter = 0;
                        pendingCounter = 0;
                    }
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
        }
    }
    public enum JobStatus { Good, Pending, Failed }
}