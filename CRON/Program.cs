using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace CRON
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Scheduler scheduler = new Scheduler();

                Task<IScheduler> task = scheduler.Initialize();
                IScheduler sched = task.Result;

                string expression = "0/20 * * * * ?";               //RUN EVERY 20 SECONDS
                //string expression = "15 0/2 * * * ?";             //RUN EVERY OTHER MINUTE (AT 15 SECONDS PAST THE MINUTE)
                //string expression = "0 0/2 8-17 * * ?";           //RUN EVERY OTHER MINUTE BUT ONLY BETWEEN 8AM AND 5PM
                //string expression = "0 0/3 17-23 * * ?";          //RUN EVERY THREE MINUTES BUT ONLY BETWEEN 5PM AND 11PM
                //string expression = "0 0 10am 1,15 * ?";          //RUN AT 10AM ON THE 1ST AND 15TH DAYS OF THE MONTH
                //string expression = "0,30 * * ? * MON-FRI";       //RUN EVERY 30 SECONDS BUT ONLY ON WEEKDAYS (MONDAY THROUGH FRIDAY)
                //string expression = "0,30 * * ? * SAT,SUN";       //RUN EVERY 30 SECONDS BUT ONLY ON WEEKENDS (SATURDAY AND SUNDAY)

                scheduler.Schedule(sched, "MyFirstJob", "TestGroup", "MyFirstTrigger", expression);
                scheduler.Start(sched);
                Console.ReadKey(true);
            }
            catch (Exception error)
            {
                Console.WriteLine("Exception caught: " + error.Message);
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey(true);
            }
        }
    }

    public class Job : IJob
    {
        public Job()
        {
        }

        public Task Execute(IJobExecutionContext context)
        {
            JobKey jobKey = context.JobDetail.Key;
            Console.WriteLine(jobKey + "|" + DateTime.UtcNow.ToString("r"));
            return null;
        }
    }

    public class Scheduler
    {
        public Scheduler()
        {}
        public async Task<IScheduler> Initialize()
        {
            IScheduler sched = null;
            try
            {
                ISchedulerFactory sf = new StdSchedulerFactory();
                sched = await sf.GetScheduler();
            }

            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            return sched;
        }
        public async void  Schedule(IScheduler sched, string jobName, string Group, string triggerName, string expression)
        {
            try
            {
                IJobDetail job = JobBuilder.Create<Job>().WithIdentity(jobName, Group).Build();

                ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create().WithIdentity(triggerName, Group).WithCronSchedule(expression).Build();

                 DateTimeOffset ft = await sched.ScheduleJob(job, trigger);

                Console.WriteLine(job.Key + " has been scheduled to run at: " + ft);
                Console.WriteLine("repeat based on expression: " + trigger.CronExpressionString);

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        public void Start(IScheduler sched)
        {
            try
            {
                sched.Start();
                Console.WriteLine("Scheduler Start");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
        public void Stop(IScheduler sched)
        {
            try
            {
                sched.Shutdown(true);
                Console.WriteLine("Scheduler Stop");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        public async void JobExecuted(IScheduler sched)
        {
            int count = 0;
            try
            {
                SchedulerMetaData metaData = await sched.GetMetaData();
                count = metaData.NumberOfJobsExecuted;
                Console.WriteLine("Jobs Executed:" + count);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }
}
