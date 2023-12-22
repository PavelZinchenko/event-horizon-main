namespace Gui.Presenter.MainMenu
{
    public partial class VersionPresenter : PresenterBase
    {
        private void Start()
        {
			Version.text = AppConfig.version + " (build " + AppConfig.buildNumber + ")";
		}
	}
}
