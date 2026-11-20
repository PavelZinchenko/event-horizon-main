using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CommonComponents.Signals;
using Zenject;
using UnityEngine.Networking;

namespace Services.InternetTime
{
    public class InternetTimeService : IInitializable, IDisposable
    {
        private const string Url = "https://zipagames.com/time.php";
        // UnityWebRequest has no timeout by default, so a stalled connection would block the loop.
        private const int RequestTimeoutSeconds = 10;
        private static readonly TimeSpan _requestInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(1);

        private readonly ServerTimeReceivedSignal.Trigger _timeReceivedTrigger;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
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
            RequestTimeLoop(_cancellation.Token).Forget();
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        // A failed request does not stop the loop; the next attempt comes after the retry interval.
        private async UniTaskVoid RequestTimeLoop(CancellationToken cancellationToken)
        {
            while (true)
            {
                var delay = _requestInterval;
                try
                {
                    OnTimeReceived(await GetNetworkTime(cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    GameDiagnostics.Trace.Log($"Error fetching time: {e.Message}");
                    delay = _retryInterval;
                }

                try
                {
                    await UniTask.Delay(delay, DelayType.Realtime, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private void OnTimeReceived(DateTime time)
        {
            _dateTime = time;
            HasBeenReceived = true;
            _timeReceivedTrigger.Fire(_dateTime);
        }

        // Throws UnityWebRequestException on connection and HTTP errors.
        private static async UniTask<DateTime> GetNetworkTime(CancellationToken cancellationToken)
        {
            using var webRequest = UnityWebRequest.Get(Url);
            webRequest.timeout = RequestTimeoutSeconds;
            await webRequest.SendWebRequest().WithCancellation(cancellationToken);

            var jsonResponse = webRequest.downloadHandler.text;
            var timeApiResponse = JsonUtility.FromJson<TimeApiResponse>(jsonResponse);
            return DateTime.Parse(timeApiResponse.datetime);
        }

        [Serializable]
        public class TimeApiResponse
        {
            public string datetime;
        }
    }

    public class ServerTimeReceivedSignal : SmartWeakSignal<ServerTimeReceivedSignal, DateTime> {}
}
