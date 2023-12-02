using System;
using UnityEngine;
using CommonComponents.Utils;

namespace Services.Account
{
    public enum Status
    {
        NotConnected,
        Connecting,
        Connected,
        FailedToConnect,
    }

    public interface IAccount
    {
        void SignIn();
        void SignOut();
        bool CanSignOut { get; }
        bool CanSignIn { get; }

        Status Status { get; }
        string DisplayName { get; }
        string Id { get; }
        IObservable<Texture2D> LoadUserIcon();
    }

    public class AccountStatusChangedSignal : SmartWeakSignal<Status>
    {
        public class Trigger : TriggerBase { }
    }
}
