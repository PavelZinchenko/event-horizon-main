// This code was automatically generated.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.

using Gui.Presenter;
using UnityEngine.UIElements;

namespace Gui.Presenter.MainMenu
{
    public partial class VersionPresenter : PresenterBase
    {
        private Label _Version;

        public override VisualElement RootElement => Version;
        public Label Version => _Version ??= (Label)UiDocument.rootVisualElement[2];
    }
}
