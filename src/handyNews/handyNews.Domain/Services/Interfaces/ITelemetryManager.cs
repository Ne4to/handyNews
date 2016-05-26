using System;
using System.Collections.Generic;

namespace handyNews.Domain.Services.Interfaces
{
    public interface ITelemetryManager
    {
        void TrackEvent(string eventName);
        void TrackEvent(string eventName, IDictionary<string, string> properties);
        void TrackError(Exception exception);
        void TrackMetric(string name, double value);
    }
}