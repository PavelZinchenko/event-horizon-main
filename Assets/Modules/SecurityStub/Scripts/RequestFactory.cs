using System;
using UnityEngine;
using UniRx;

namespace Security
{
    public static class RequestFactory
    {
        public static IObservable<string> CreateGetPlayerInfoRequest(string playerId)
        {
            return Observable.Empty<string>();
        }

        public static IObservable<string> CreateGetTimeFromLastFightRequest(string playerId)
        {
            return Observable.Empty<string>();
        }

        public static IObservable<string> CreateRegisterPlayerRequest(string playerId, string userName, int rating, int aiRating, string fleet)
        {
            return Observable.Empty<string>();
        }

        public static IObservable<string> CreateFindOpponentRequest(string playerId, int rating)
        {
            return Observable.Empty<string>();
        }

        public static IObservable<string> CreateGetFleetRequest(int playerId)
        {
            return Observable.Empty<string>();
        }

        public static IObservable<string> CreateUpdateStatsRequest(string playerId, int enemyId, int status)
        {
            return Observable.Empty<string>();
        }

        public static WWW CreateGetItemsRequest(string id)
        {
            return new WWW(string.Empty);
        }

        public static WWW CreateSetItemsRequest(string id, string data)
        {
            return new WWW(string.Empty);
        }

        public static WWW CreateGetIapDataRequest(string id)
        {
            return new WWW(string.Empty);
        }

        public static WWW CreateSetIapDataRequest(string id, string data)
        {
            return new WWW(string.Empty);
        }
    }
}
