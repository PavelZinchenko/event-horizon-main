using Services.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Gui.Behaviours
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonBehaviour : MonoBehaviour, IPointerDownHandler
    {
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [SerializeField] private AudioClip _pressedSound;
        [SerializeField] private AudioClip _clickedSound;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!GetComponent<Selectable>().interactable) return;

            if (_soundPlayer != null && _pressedSound)
                _soundPlayer.Play(_pressedSound);
        }

        private void Start()
        {
            GetComponent<Button>()?.onClick.AddListener(OnButtonClicked);
            GetComponent<Toggle>()?.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnButtonClicked()
        {
            if (_soundPlayer != null && _clickedSound)
                _soundPlayer.Play(_clickedSound);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value) OnButtonClicked();
        }
    }
}
