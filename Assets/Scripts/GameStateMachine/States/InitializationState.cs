﻿using System.Linq;
using System.Collections.Generic;
using GameServices.SceneManager;
using Constructor;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameServices.Settings;
using Services.Account;
using UnityEngine;
using Session;
using Services.Advertisements;
using Services.Storage;
using Services.Localization;
using Zenject;
using Services.Audio;
using GameServices.Audio;
using Constructor.Model;

namespace GameStateMachine.States
{
    public class InitializationState : BaseState
    {
        [Inject]
        public InitializationState(
			IStateMachine stateMachine, 
			GameStateFactory stateFactory,
            SessionData sessionData, 
			IDataStorage localStorage, 
			GameSettings settings, 
			IAccount account, 
			IDatabase database, 
            IAdsManager adsManager,
            IMusicPlayer musicPlayer,
            DatabaseMusicPlaylist databaseMusicPlaylist,
			ILocalization localization)
            : base(stateMachine, stateFactory)
        {
            _sessionData = sessionData;
            _database = database;
            _localStorage = localStorage;
            _settings = settings;
            _account = account;
            _adsManager = adsManager;
            _localization = localization;
            _musicPlayer = musicPlayer;
            _databaseMusicPlaylist = databaseMusicPlaylist;
        }

        public override StateType Type { get { return StateType.Initialization; } }

        protected override void OnLoad()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif

#if UNITY_STANDALONE
            Application.runInBackground = _settings.RunInBackground;
#endif
            QualitySettings.SetQualityLevel(_settings.QualityMode < 0 ? 0 : 1);

            Debug.Log (SystemInfo.operatingSystem);
            Debug.Log (SystemInfo.deviceModel);
            Debug.Log (SystemInfo.deviceType.ToString ());
            Debug.Log (SystemInfo.deviceName);

            _database.LookForMods();

            var i = 0;
            while (i < _settings.ExternalMods.Count)
            {
                var path = _settings.ExternalMods[i];
                if (!_database.TryAddModFromFile(path))
                {
                    _settings.ExternalMods.RemoveAt(i);
                    continue;
                }

                i++;
            }

            var mod = _settings.ActiveMod;
            string error;
            if (!string.IsNullOrEmpty(mod) && _database.TryLoad(mod, out error))
            {
                Debug.Log("Mod loaded - " + mod);
            }
            else
            {
                mod = string.Empty;
                _database.LoadDefault();
            }

            _localization.Initialize(_settings.Language, _database);

		    if (_database.IsEditable)
		    {
                GameDiagnostics.Trace.Log("Checking ship builds...");
                
                foreach (var item in _database.ShipBuildList)
		        {
                    var ship = new CommonShip(item, _database);
                    Domain.Shipyard.ShipValidator.RemoveInvalidParts(ship);
		            if ((item.Ship.ShipType == ShipType.Common || item.Ship.ShipType == ShipType.Drone) && (item.AvailableForPlayer || item.AvailableForEnemy) &&
		                !ShipValidator.IsShipViable(new CommonShip(item, _database), _database.ShipSettings))
		            {
		                GameDiagnostics.Trace.LogError("invalid build: " + item.Id);
		            }
		        }

		        var companions = _database.SatelliteBuildList; //Resources.LoadAll<Constructor.CompanionBuildWrapper> ("Prefabs/Constructor/CompanionBuilds");
		        foreach (var item in companions)
		        {
		            var components = item.Components
		                .Select<InstalledComponent, IntegratedComponent>(ComponentExtensions.FromDatabase).ToArray();
                    var layout = new ShipLayoutObsolete(new ShipLayoutAdapter(item.Satellite.Layout), item.Satellite.Barrels, components);
                    if (layout.Components.Count() != components.Length)
		                GameDiagnostics.Trace.LogError("invalid satellite layout: " + item.Id);
		        }

                GameDiagnostics.Trace.Log("Checking techs...");

		        foreach (var tech in _database.TechnologyList)
		        {
		            var index = tech.Dependencies.IndexOf(null);
                    if (index >= 0)
                        GameDiagnostics.Trace.LogError($"{tech.Id}: unknown dependency - {index}");
                }
		    }

		    Debug.Log("InitializationState: signin - " + _settings.SignedIn);
			if (_settings.SignedIn)
			{
				_account.SignIn();
			}

		    if (_localStorage.TryLoad(_sessionData, _database.Id))
		        Debug.Log("Saved game loaded");
		    else if (_database.IsEditable && _localStorage.TryImportOriginalSave(_sessionData, _database.Id))
		        Debug.Log("Original saved game imported");
		    else
		        _sessionData.CreateNewGame(_database.Id);

            if (!_sessionData.Purchases.RemoveAds)
                _adsManager.ShowInterstitial();

            _musicPlayer.Playlist = _databaseMusicPlaylist;

            LoadStateAdditive(StateFactory.CreateMainMenuState());
        }

		public override IEnumerable<GameScene> RequiredScenes 
		{
			get 
			{
				yield return GameScene.Loader;
				yield return GameScene.CommonGui;
			}
		}

		protected override void OnSuspend()
        {
        }

        protected override void OnResume()
        {
        }

        private readonly SessionData _sessionData;
        private readonly IDatabase _database;
        private readonly IDataStorage _localStorage;
        private readonly GameSettings _settings;
        private readonly IAccount _account;
        private readonly IAdsManager _adsManager;
        private readonly IMusicPlayer _musicPlayer;
        private readonly ILocalization _localization;
        private readonly DatabaseMusicPlaylist _databaseMusicPlaylist;

        public class Factory : Factory<InitializationState> { }
    }
}
