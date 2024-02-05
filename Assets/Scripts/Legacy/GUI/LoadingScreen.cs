using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GameDatabase;
using GameDatabase.Query;
using GameServices.SceneManager;
using Services.Localization;
using Services.Resources;
using Services.Settings;
using Zenject;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField] private Canvas _canvas;
    [SerializeField] private Text _shipNameText;
    [SerializeField] private Image _shipSprite;
    [SerializeField] private Image _shipIcon;
    [SerializeField] private Image _background;
	[SerializeField] private Text _loadingText;
	[SerializeField] private float _delay = 0.1f;

	[Inject] private readonly IResourceLocator _resourceLocator;
    [Inject] private readonly IDatabase _database;
	[Inject] private readonly ILocalization _localization;
	[Inject] private readonly IGameSettings _settings;

	private bool _active;
	private bool _coroutineRunning;

	[Inject]
    private void Initialize(
        SceneManagerStateChangedSignal sceneManagerStateChangedSignal,
        LocalizationChangedSignal localizationChangedSignal)
    {
        _sceneManagerStateChangedSignal = sceneManagerStateChangedSignal;
		_sceneManagerStateChangedSignal.Event += OnSceneManagerStateChanged;
        _localizationChangedSignal = localizationChangedSignal;
        _localizationChangedSignal.Event += OnLocalizationChanged;
	}

	private void OnLocalizationChanged(string language)
	{
		_shipNameText.text = _localization.GetString("$Credits_Title");
		_loadingText.text = _localization.GetString("$Loading");
	}

	private void OnSceneManagerStateChanged(State state)
	{
		_active = state == State.Loading;

		if (_active && _firstTime)
			Show();
		else if (!_coroutineRunning)
			StartCoroutine(UpdateVisibility(_delay));
	}

	private IEnumerator UpdateVisibility(float delay)
	{
		_coroutineRunning = true;
		yield return new WaitForSecondsRealtime(delay);

		if (_active)
			Show();
		else
			Hide();

		_coroutineRunning = false;
	}

	private void Show()
    {
		_canvas.enabled = true;

        if (_firstTime)
        {
            _shipIcon.gameObject.SetActive(false);
            _background.gameObject.SetActive(false);
            _firstTime = false;
        }
        else
        {
            _shipIcon.gameObject.SetActive(true);
            _background.gameObject.SetActive(true);
            var ship = ShipBuildQuery.PlayerShips(_database).CommonAndRare().Random(_random).Ship;
            _shipNameText.text = _localization.GetString(ship.Name);
            _shipIcon.sprite = _resourceLocator.GetSprite(ship.IconImage) ?? _resourceLocator.GetSprite(ship.ModelImage);
            _shipSprite.sprite = _resourceLocator.GetSprite(ship.ModelImage);
        }
    }

    private void Hide()
    {
		_canvas.enabled = false;
    }

    private bool _firstTime = true;
    private readonly System.Random _random = new System.Random();
    private SceneManagerStateChangedSignal _sceneManagerStateChangedSignal;
    private LocalizationChangedSignal _localizationChangedSignal;
}
