using System;
using System.Collections.Generic;
using handyNews.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Inoreader
{
    public class ApplicationInsightsTelemetryManager : ITelemetryManager
    {
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsTelemetryManager(TelemetryClient telemetryClient)
        {
            if (telemetryClient == null) throw new ArgumentNullException(nameof(telemetryClient));
            _telemetryClient = telemetryClient;
        }

        public void TrackEvent(string eventName)
        {
            _telemetryClient.TrackEvent(eventName);
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
            var eventTelemetry = new EventTelemetry(eventName);
            foreach (var kvp in properties)
            {
                eventTelemetry.Properties.Add(kvp.Key, kvp.Value);
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        public void TrackError(Exception exception)
        {
            _telemetryClient.TrackException(exception);
        }

        public void TrackMetric(string name, double value)
        {
            _telemetryClient.TrackMetric(name, value);
        }
    }
}