using Hangfire;
using Hangfire.MemoryStorage;
using System;

namespace hangfire_test
{
    class Program
    {
        private void Test1()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            BackgroundJob.Enqueue(() => Console.WriteLine("Background Job!"));
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Recurring Job!"), Cron.Minutely);
            var jobId = BackgroundJob.Schedule(() => Console.WriteLine("Delayed!"), TimeSpan.FromSeconds(5));
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Continuation!"));

            using (new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press ENTER to exit...");
                Console.ReadLine();
            }
        }
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Test1();
        }
    }
}
