using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Security
{
    // The open-source build has no arena server. Every arena request fails.
    public static class RequestFactory
    {
        public static UniTask<string> CreateGetPlayerInfoRequest(string playerId)
        {
            return NotAvailable();
        }

        public static UniTask<string> CreateGetTimeFromLastFightRequest(string playerId)
        {
            return NotAvailable();
        }

        public static UniTask<string> CreateRegisterPlayerRequest(string playerId, string userName, int rating, int aiRating, string fleet)
        {
            return NotAvailable();
        }

        public static UniTask<string> CreateFindOpponentRequest(string playerId, int rating)
        {
            return NotAvailable();
        }

        public static UniTask<string> CreateGetFleetRequest(int playerId)
        {
            return NotAvailable();
        }

        public static UniTask<string> CreateUpdateStatsRequest(string playerId, int enemyId, int status)
        {
            return NotAvailable();
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

        private static UniTask<string> NotAvailable()
        {
            return UniTask.FromException<string>(new NotSupportedException("The arena server is not available in this build"));
        }
    }
}
