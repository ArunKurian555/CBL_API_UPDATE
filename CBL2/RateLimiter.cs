using System;
using System.Threading;

namespace CBL2
{

    public class RateLimiter
    {
        private readonly int requestsPerSecond;
        private DateTime lastRequestTime;
        private readonly object lockObject = new object();

        public RateLimiter(int requestsPerSecond)
        {
            this.requestsPerSecond = requestsPerSecond;
            this.lastRequestTime = DateTime.MinValue;
        }

        public bool ShouldSendRequest()
        {
            lock (lockObject)
            {
                // Calculate the minimum interval between requests
                var minInterval = TimeSpan.FromSeconds(1.0 / requestsPerSecond);

                // Calculate the time elapsed since the last request
                var elapsed = DateTime.UtcNow - lastRequestTime;

                // If the elapsed time is less than the minimum interval, skip sending the request
                if (elapsed < minInterval)
                {
                    return false;
                }

                // Update the last request time to the current time
                lastRequestTime = DateTime.UtcNow;
                return true;
            }
        }
    }
}
