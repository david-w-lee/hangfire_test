using Hangfire;
using Hangfire.MemoryStorage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace hangfire_test
{
    class Program
    {
        private void Test1()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Enqueueing tasks");
            BackgroundJob.Enqueue(() => new Job().ShowMessage("Background Job!"));
            RecurringJob.AddOrUpdate(() => new Job().ShowMessage($"Recurring Job!"), Cron.Minutely);
            var jobId = BackgroundJob.Schedule(() => new Job().ShowMessage($"Delayed!"), TimeSpan.FromSeconds(5));
            BackgroundJob.ContinueJobWith(jobId, () => new Job().ShowMessage($"Continuation!"));

            using (new BackgroundJobServer())
            {
                Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Hangfire Server started. Press ENTER to exit...");
                Console.ReadLine();
            }
        }

        private void Test2()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            var foregroundTask1 = Task.Run(() =>
            {
                Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Enqueueing tasks");
                
                // Look at the stack trace.
                //  Notice that this Expression Tree gets serialized and saved into the DB
                //  and then later gets executed using Reflection in one of the BackgroundJobServers.
                BackgroundJob.Enqueue(() => new Job().ShowTrackTrace());
            });

            var foregroundTask2 = Task.Run(() =>
            {
                Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Enqueueing tasks");

                // Look at the thread id and see how they are different from thread ids of foreground tasks.
                for (int i = 0; i < 5; i++)
                {
                    BackgroundJob.Enqueue(() => new Job().ShowMessage("BackgroundJob!"));
                }
            });

            var backgroundTask1 = Task.Run(() =>
            {
                using (new BackgroundJobServer())
                {
                    Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Hangfire Server started. Press ENTER to exit...");
                    Console.ReadLine();
                }
            });

            var backgroundTask2 = Task.Run(() =>
            {
                using (new BackgroundJobServer())
                {
                    Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): Hangfire Server started. Press ENTER to exit...");
                    Console.ReadLine();
                }
            });

            // Because there are two BackgroundJobServers picking up jobs, even if you press Enter once to terminate one BackgroundJobServer,
            //  another one still picks up Jobs and run them.
            Task.WaitAll(foregroundTask1, foregroundTask2, backgroundTask1, backgroundTask2);
        }

        public class Job
        {
            public Job()
            {
            }

            public void ShowMessage(string message)
            {
                Console.WriteLine($"Thread({Thread.CurrentThread.ManagedThreadId}): {message}!");
            }

            public void ShowTrackTrace()
            {
                Console.WriteLine($"{Environment.StackTrace}");
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Test2();
        }
    }
}
