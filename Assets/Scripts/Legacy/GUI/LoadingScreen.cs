using UnityEngine;
using UnityEngine.UI;
using GameDatabase;
using GameDatabase.Query;
using GameServices.SceneManager;
using Services.Localization;
using Services.Reources;
using Zenject;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Text _shipNameText;
    [SerializeField] private Image _shipSprite;
    [SerializeField] private Image _shipIcon;
    [SerializeField] private Image _background;

    [Inject] private readonly IResourceLocator _resourceLocator;
    [Inject] private readonly IDatabase _database;

    [Inject]
    private void Initialize(SceneManagerStateChangedSignal sceneManagerStateChangedSignal, ILocalization localization)
    {
        _sceneManagerStateChangedSignal = sceneManagerStateChangedSignal;
		_sceneManagerStateChangedSignal.Event += OnSceneManagerStateChanged;
		_localization = localization;
    }

	private void OnSceneManagerStateChanged(State state)
	{
		if (state == State.Loading)
		{
			Show();
		}
		else
		{
			Hide();
		}
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
    private ILocalization _localization;
}
