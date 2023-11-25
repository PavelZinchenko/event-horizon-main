using UnityEngine;
using UnityEngine.UIElements;

namespace Gui.Presenter.MainMenu
{
    public partial class PrivacyPolicyButtonPresenter : PresenterBase
    {
        private void Awake()
        {
#if !UNITY_ANDROID && !UNITY_IOS
        PrivacyPolicy.style.display = DisplayStyle.None;
#endif
        }

        private void OnEnable()
        {
            PrivacyPolicy_button.RegisterCallback<ClickEvent>(OnPrivacyPolicyClicked);
        }

        private void OnDisable()
        {
            PrivacyPolicy_button.UnregisterCallback<ClickEvent>(OnPrivacyPolicyClicked);
        }

        private void OnPrivacyPolicyClicked(ClickEvent e)
        {
            Application.OpenURL("https://zipagames.com/policy.html");
        }
    }
}
