using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Services.Audio;

namespace Gui.Presenter.MainMenu
{
	public partial class CheatPanelPresenter : PresenterBase
	{
		[Inject] private readonly ISoundPlayer _soundPlayer;
		[Inject] private readonly Cheats _cheats;

		[SerializeField] private AudioClip _clickSound;
		[SerializeField] private AudioClip _confirmSound;
		[SerializeField] private int _maxLength = 8;
		[SerializeField] private float _maxClickInterval = 0.4f;
		[SerializeField] private float _maxIdleTime = 5f;

		private bool _visible;
		private float _idletime;
		private float _lastClickTime;
		private int _clickCount;
		private int _hash;
		private string _command = string.Empty;

		public bool Visible
		{
			get => _visible;
			set
			{
				_visible = value;
				Cheats.style.opacity = _visible ? 1f : 0f;
			}
		}

		private string Command
		{
			get => _command;
			set
			{
				_command = value.Length < _maxLength ? value : value[^_maxLength..];
				Cheats_Code.text = _command;
			}
		}

		private void Awake()
		{
			if (!AppConfig.enableCheats)
				Destroy(gameObject);
		}

		private void Start()
		{
			_hash = Security.DebugCommands.GetHashCode(SystemInfo.deviceUniqueIdentifier);
			Command = string.Empty;
			Visible = false;
		}

		private void OnEnable()
		{
			Cheats.RegisterCallback<ClickEvent>(OnPanelClicked);
			Cheats_Num0.RegisterCallback<ClickEvent>(OnButton0Clicked);
			Cheats_Num1.RegisterCallback<ClickEvent>(OnButton1Clicked);
			Cheats_Num2.RegisterCallback<ClickEvent>(OnButton2Clicked);
			Cheats_Num3.RegisterCallback<ClickEvent>(OnButton3Clicked);
			Cheats_Num4.RegisterCallback<ClickEvent>(OnButton4Clicked);
			Cheats_Num5.RegisterCallback<ClickEvent>(OnButton5Clicked);
			Cheats_Num6.RegisterCallback<ClickEvent>(OnButton6Clicked);
			Cheats_Num7.RegisterCallback<ClickEvent>(OnButton7Clicked);
			Cheats_Num8.RegisterCallback<ClickEvent>(OnButton8Clicked);
			Cheats_Num9.RegisterCallback<ClickEvent>(OnButton9Clicked);
			Cheats_Enter.RegisterCallback<ClickEvent>(OnEnterClicked);
			Cheats_Reset.RegisterCallback<ClickEvent>(OnResetClicked);
		}

		private void OnDisable()
		{
			Cheats.UnregisterCallback<ClickEvent>(OnPanelClicked);
			Cheats_Num0.UnregisterCallback<ClickEvent>(OnButton0Clicked);
			Cheats_Num1.UnregisterCallback<ClickEvent>(OnButton1Clicked);
			Cheats_Num2.UnregisterCallback<ClickEvent>(OnButton2Clicked);
			Cheats_Num3.UnregisterCallback<ClickEvent>(OnButton3Clicked);
			Cheats_Num4.UnregisterCallback<ClickEvent>(OnButton4Clicked);
			Cheats_Num5.UnregisterCallback<ClickEvent>(OnButton5Clicked);
			Cheats_Num6.UnregisterCallback<ClickEvent>(OnButton6Clicked);
			Cheats_Num7.UnregisterCallback<ClickEvent>(OnButton7Clicked);
			Cheats_Num8.UnregisterCallback<ClickEvent>(OnButton8Clicked);
			Cheats_Num9.UnregisterCallback<ClickEvent>(OnButton9Clicked);
			Cheats_Enter.UnregisterCallback<ClickEvent>(OnEnterClicked);
			Cheats_Reset.UnregisterCallback<ClickEvent>(OnResetClicked);
		}

		private void OnButton0Clicked(ClickEvent e) => OnButtonClicked('0');
		private void OnButton1Clicked(ClickEvent e) => OnButtonClicked('1');
		private void OnButton2Clicked(ClickEvent e) => OnButtonClicked('2');
		private void OnButton3Clicked(ClickEvent e) => OnButtonClicked('3');
		private void OnButton4Clicked(ClickEvent e) => OnButtonClicked('4');
		private void OnButton5Clicked(ClickEvent e) => OnButtonClicked('5');
		private void OnButton6Clicked(ClickEvent e) => OnButtonClicked('6');
		private void OnButton7Clicked(ClickEvent e) => OnButtonClicked('7');
		private void OnButton8Clicked(ClickEvent e) => OnButtonClicked('8');
		private void OnButton9Clicked(ClickEvent e) => OnButtonClicked('9');

		private void OnResetClicked(ClickEvent e)
		{
			if (!Visible) return;

			Command = string.Empty;
		}

		private void OnEnterClicked(ClickEvent e)
		{
			if (!Visible) return;

			if (TryExecuteCommand(_command))
				Cheats_Code.text = "OK";
			else
				Cheats_Code.text = "ERROR";

			_command = string.Empty;
		}

		private void OnPanelClicked(ClickEvent e)
		{
			if (Time.unscaledTime - _lastClickTime > _maxClickInterval)
			{
				_clickCount = 1;
			}
			else if (++_clickCount == 3)
			{
				_idletime = 0;
				Visible = true;
				Cheats_Code.text = _hash.ToString();
			}

			_lastClickTime = Time.unscaledTime;
		}

		private void Update()
		{
			if (!Visible) return;

			_idletime += Time.deltaTime;
			if (_idletime > _maxIdleTime)
				Visible = false;
		}

		public void OnButtonClicked(char key)
		{
			if (!Visible) return;

			_idletime = 0;
			_soundPlayer.Play(_clickSound);
			Command += key;
		}

		private bool TryExecuteCommand(string command)
		{
			if (_cheats.TryExecuteCommand(command, _hash))
			{
				_soundPlayer.Play(_confirmSound);
				return true;
			}

			return false;
		}
	}
}
