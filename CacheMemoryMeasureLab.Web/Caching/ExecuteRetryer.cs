using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace CacheMemoryMeasureLab.Web.Caching
{
    public class ExecuteRetryer
    {
        protected int RetryTimes { get; set; }
        protected int WaitSeconds { get; set; }

        private int CurrentRetry { get; set; }

        public ExecuteRetryer(int retryTimes, int waitSeconds)
        {
            this.RetryTimes = retryTimes;
            this.WaitSeconds = waitSeconds;
        }

        public ExecuteRetryer() : this(5, 200)
        {
        }

        public void Execute(Action action)
        {
            CurrentRetry = 0;
            RetryExecute(action);
        }

        public T Execute<T>(Func<T> action)
        {
            CurrentRetry = 0;
            return RetryExecute(action);
        }

        private T RetryExecute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch
            {
                if (CurrentRetry >= RetryTimes)
                    throw;
                System.Threading.Thread.Sleep(WaitSeconds);
                Debug.WriteLine("retry:{0}", CurrentRetry);
                CurrentRetry += 1;
                return RetryExecute(action);
            }
        }

        private void RetryExecute(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                if (CurrentRetry >= RetryTimes)
                    throw;
                System.Threading.Thread.Sleep(WaitSeconds);
                Debug.WriteLine("retry:{0}", CurrentRetry);
                CurrentRetry += 1;
                RetryExecute(action);
            }
        }
    }
}