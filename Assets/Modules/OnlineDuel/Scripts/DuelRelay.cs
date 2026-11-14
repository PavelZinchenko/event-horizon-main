using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace OnlineDuel
{
    // What went wrong in sign-in, host or join, as the lobby tells the player.
    public enum DuelServiceError
    {
        Unknown,
        InvalidCode,
        CodeNotFound,
        NoInternet,
        ServiceUnavailable,
        TooManyRequests,
        SignInFailed,
    }

    // A failed DuelRelay call. Code: the Unity Services error code, 0 when there is none.
    public sealed class DuelServiceException : Exception
    {
        public DuelServiceException(DuelServiceError error, int code, Exception inner)
            : base($"{error} (code {code}): {inner?.Message}", inner)
        {
            Error = error;
            Code = code;
        }

        public DuelServiceError Error { get; }
        public int Code { get; }
    }

    // Unity Relay with anonymous Unity Authentication and join codes (network decision 10). Main thread only: the
    // Unity Services SDK continues its tasks on the Unity synchronization context.
    // The caller owns the returned transport and disposes it to leave the Relay session.
    public static class DuelRelay
    {
        // UDP keeps the per-datagram overhead that the network gate measured (38-byte Relay header).
        public const string ConnectionType = "udp";
        private const int MaxGuests = 1;

        public static string PlayerId => UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : null;

        // Starts Unity Services and signs in anonymously, once per run. Repeated calls share the same work.
        public static Task SignInAsync()
        {
            if (_signIn == null || _signIn.IsFaulted || _signIn.IsCanceled)
                _signIn = SignIn();
            return _signIn;
        }

        // Creates an allocation for one guest and its join code. The host shows the code to the other player.
        // Service failures throw DuelServiceException.
        public static async Task<(RelayTransport Transport, string JoinCode)> HostAsync(CancellationToken cancellation)
        {
            await Call(SignInAsync(), false);
            cancellation.ThrowIfCancellationRequested();
            var allocation = await Call(RelayService.Instance.CreateAllocationAsync(MaxGuests), false);
            var joinCode = await Call(RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId), false);
            // An allocation that is never bound expires on the Relay server by itself.
            cancellation.ThrowIfCancellationRequested();
            return (RelayTransport.ListenRelay(allocation.ToRelayServerData(ConnectionType)), joinCode);
        }

        // Service failures and a code that cannot be valid throw DuelServiceException.
        public static async Task<RelayTransport> JoinAsync(string joinCode, CancellationToken cancellation)
        {
            joinCode = joinCode?.Trim().ToUpperInvariant();
            if (!IsValidJoinCode(joinCode)) throw new DuelServiceException(DuelServiceError.InvalidCode, 0, null);
            await Call(SignInAsync(), false);
            cancellation.ThrowIfCancellationRequested();
            var allocation = await Call(RelayService.Instance.JoinAllocationAsync(joinCode), true);
            cancellation.ThrowIfCancellationRequested();
            return RelayTransport.ConnectRelay(allocation.ToRelayServerData(ConnectionType));
        }

        // The Relay service's rule: 6 to 12 characters of "6789BCDFGHJKLMNPQRTW", any case.
        public static bool IsValidJoinCode(string joinCode)
        {
            if (string.IsNullOrEmpty(joinCode) || joinCode.Length < 6 || joinCode.Length > 12) return false;
            foreach (var c in joinCode)
                if (JoinCodeCharacters.IndexOf(char.ToUpperInvariant(c)) < 0) return false;
            return true;
        }

        private const string JoinCodeCharacters = "6789BCDFGHJKLMNPQRTW";

        private static async Task Call(Task task, bool joining)
        {
            try { await task; }
            catch (Exception e) when (!(e is OperationCanceledException) && !(e is DuelServiceException))
            {
                throw Classify(e, joining);
            }
        }

        private static async Task<T> Call<T>(Task<T> task, bool joining)
        {
            try { return await task; }
            catch (Exception e) when (!(e is OperationCanceledException) && !(e is DuelServiceException))
            {
                throw Classify(e, joining);
            }
        }

        // joining: an invalid request or a missing allocation means the join code is wrong.
        private static DuelServiceException Classify(Exception e, bool joining)
        {
            switch (e)
            {
                case RelayServiceException relay:
                    return new DuelServiceException(Classify(relay.Reason, joining), (int)relay.Reason, e);
                case AuthenticationException auth:
                    return new DuelServiceException(Common(auth.ErrorCode) ?? DuelServiceError.SignInFailed, auth.ErrorCode, e);
                case RequestFailedException request:
                    return new DuelServiceException(Common(request.ErrorCode) ?? DuelServiceError.Unknown, request.ErrorCode, e);
                case ServicesInitializationException _:
                    return new DuelServiceException(DuelServiceError.SignInFailed, 0, e);
                default:
                    return new DuelServiceException(DuelServiceError.Unknown, 0, e);
            }
        }

        private static DuelServiceError Classify(RelayExceptionReason reason, bool joining)
        {
            switch (reason)
            {
                case RelayExceptionReason.JoinCodeNotFound:
                case RelayExceptionReason.AllocationNotFound when joining:
                case RelayExceptionReason.EntityNotFound when joining:
                    return DuelServiceError.CodeNotFound;
                case RelayExceptionReason.InvalidRequest when joining:
                case RelayExceptionReason.InvalidArgument when joining:
                    return DuelServiceError.InvalidCode;
                case RelayExceptionReason.NetworkError:
                case RelayExceptionReason.RequestTimeOut:
                case RelayExceptionReason.GatewayTimeout:
                    return DuelServiceError.NoInternet;
                case RelayExceptionReason.RateLimited:
                    return DuelServiceError.TooManyRequests;
                case RelayExceptionReason.Unauthorized:
                case RelayExceptionReason.Forbidden:
                    return DuelServiceError.SignInFailed;
                case RelayExceptionReason.InactiveProject:
                case RelayExceptionReason.PaymentRequired:
                case RelayExceptionReason.NoSuitableRelay:
                case RelayExceptionReason.RegionNotFound:
                case RelayExceptionReason.InternalServerError:
                case RelayExceptionReason.BadGateway:
                case RelayExceptionReason.ServiceUnavailable:
                    return DuelServiceError.ServiceUnavailable;
                default:
                    return DuelServiceError.Unknown;
            }
        }

        private static DuelServiceError? Common(int code)
        {
            switch (code)
            {
                case CommonErrorCodes.TransportError:
                case CommonErrorCodes.Timeout:
                    return DuelServiceError.NoInternet;
                case CommonErrorCodes.ServiceUnavailable:
                    return DuelServiceError.ServiceUnavailable;
                case CommonErrorCodes.TooManyRequests:
                    return DuelServiceError.TooManyRequests;
                default:
                    return null;
            }
        }

        private static async Task SignIn()
        {
            var options = new InitializationOptions();
            var profile = EditorProfile();
            if (profile != null) options.SetProfile(profile);
            await UnityServices.InitializeAsync(options);
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Editor instances of Multiplayer Play Mode share PlayerPrefs, where Authentication keeps the session token.
        // Without a profile per instance, host and guest sign in as the same player. Builds use the default profile.
        private static string EditorProfile()
        {
#if UNITY_EDITOR
            // FNV-1a: string.GetHashCode is not stable between runs, and the profile must be.
            var hash = 2166136261u;
            foreach (var c in Application.dataPath) hash = (hash ^ c) * 16777619u;
            return "editor_" + hash.ToString("x8");
#else
            return null;
#endif
        }

        private static Task _signIn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayMode() => _signIn = null;
    }
}
