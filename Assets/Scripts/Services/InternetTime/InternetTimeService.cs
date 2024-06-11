using System;
using UniRx;
using UnityEngine;
using CommonComponents.Signals;
using Zenject;
using UnityEngine.Networking;

namespace Services.InternetTime
{
    public class InternetTimeService : IInitializable, IDisposable
    {
        private const string Url = "https://worldtimeapi.org/api/timezone/Etc/UTC";

        private readonly ServerTimeReceivedSignal.Trigger _timeReceivedTrigger;
        private IDisposable _timeSubscription;
        private DateTime _dateTime;

        public InternetTimeService(ServerTimeReceivedSignal.Trigger timeReceivedTrigger)
        {
            _timeReceivedTrigger = timeReceivedTrigger;
        }

        public bool HasBeenReceived { get; private set; }
        public DateTime DateTime => HasBeenReceived ? _dateTime : DateTime.Now;

        public int TotalDays => (int) (DateTime.Ticks / TimeSpan.TicksPerDay);

        public void Initialize()
        {
            _timeSubscription = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .SelectMany(_ => GetNetworkTime())
                .Subscribe(
                    OnTimeReceived,
                    error => GameDiagnostics.Trace.LogError($"Error fetching time: {error}")
                );
        }

        public void Dispose()
        {
            _timeSubscription?.Dispose();
        }

        private void OnTimeReceived(DateTime time)
        {
            _dateTime = time;
            HasBeenReceived = true;
            _timeReceivedTrigger.Fire(_dateTime);
        }

        private IObservable<DateTime> GetNetworkTime()
        {
            return Observable.Create<DateTime>(observer =>
            {
                var webRequest = UnityWebRequest.Get(Url);
                var operation = webRequest.SendWebRequest();

                return Observable.EveryUpdate()
                    .Where(_ => operation.isDone)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                        {
                            observer.OnError(new Exception(webRequest.error));
                        }
                        else
                        {
                            var jsonResponse = webRequest.downloadHandler.text;
                            var timeApiResponse = JsonUtility.FromJson<TimeApiResponse>(jsonResponse);
                            var networkTime = DateTime.Parse(timeApiResponse.datetime);
                            observer.OnNext(networkTime);
                            observer.OnCompleted();
                        }
                    });
            });
        }

        [Serializable]
        public class TimeApiResponse
        {
            public string datetime;
        }
    }

    public class ServerTimeReceivedSignal : SmartWeakSignal<ServerTimeReceivedSignal, DateTime> {}
}
