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
        public static async Task<(RelayTransport Transport, string JoinCode)> HostAsync(CancellationToken cancellation)
        {
            await SignInAsync();
            cancellation.ThrowIfCancellationRequested();
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxGuests);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            // An allocation that is never bound expires on the Relay server by itself.
            cancellation.ThrowIfCancellationRequested();
            return (RelayTransport.ListenRelay(allocation.ToRelayServerData(ConnectionType)), joinCode);
        }

        public static async Task<RelayTransport> JoinAsync(string joinCode, CancellationToken cancellation)
        {
            joinCode = joinCode?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(joinCode)) throw new ArgumentException("No join code", nameof(joinCode));
            await SignInAsync();
            cancellation.ThrowIfCancellationRequested();
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            cancellation.ThrowIfCancellationRequested();
            return RelayTransport.ConnectRelay(allocation.ToRelayServerData(ConnectionType));
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
