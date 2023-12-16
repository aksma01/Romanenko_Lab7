using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace task3
{
    public class LogEntry
    {
        public int ThreadId { get; }
        public DateTime Timestamp { get; }

        public LogEntry(int threadId)
        {
            ThreadId = threadId;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Операція виконана потоком {ThreadId} о {Timestamp}";
        }
    }

    public class SharedLog
    {
        private readonly List<LogEntry> logEntries = new List<LogEntry>();
        private readonly object lockObject = new object();

        public void AddEntry(int threadId)
        {
            lock (lockObject)
            {
                var entry = new LogEntry(threadId);
                logEntries.Add(entry);

                Console.WriteLine(entry);
            }
        }

        public List<LogEntry> GetLogEntries()
        {
            lock (lockObject)
            {
                return new List<LogEntry>(logEntries);
            }
        }
    }

    public class ConflictResolver
    {
        private readonly SharedLog sharedLog;

        public ConflictResolver(SharedLog sharedLog)
        {
            this.sharedLog = sharedLog;
        }

        public void ResolveConflicts()
        {
            var logEntries = sharedLog.GetLogEntries();

            foreach (var entry in logEntries)
            {
                if (entry.ThreadId % 2 == 0)
                {
                    Console.WriteLine($"Виявлено конфлікт для операції виконаної потоком {entry.ThreadId}. Відновлення...");
                }
            }
        }
    }
    public class Program
    {
        static void Main()
        {
            SharedLog sharedLog = new SharedLog();
            ConflictResolver conflictResolver = new ConflictResolver(sharedLog);

            Thread thread1 = new Thread(() => ExecuteOperation(sharedLog, 1));
            Thread thread2 = new Thread(() => ExecuteOperation(sharedLog, 2));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Console.WriteLine("\nЗавершено запис в журнал.");

            conflictResolver.ResolveConflicts();
        }

        static void ExecuteOperation(SharedLog sharedLog, int threadId)
        {
            for (int i = 0; i < 5; i++)
            {
                sharedLog.AddEntry(threadId);
                Thread.Sleep(100);
            }
        }
    }
}