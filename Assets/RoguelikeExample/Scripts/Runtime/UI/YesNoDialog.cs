// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RoguelikeExample.UI
{
    /// <summary>
    /// はい/いいえダイアログ
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class YesNoDialog : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI message;

        [SerializeField]
        private Button yesButton;

        [SerializeField]
        private Button noButton;

        public void SetMessage(string messageString)
        {
            this.message.text = messageString;
        }

        public void AddOnYesButtonClickListener(UnityAction onClick)
        {
            yesButton.onClick.AddListener(onClick);
        }

        public void AddOnNoButtonClickListener(UnityAction onClick)
        {
            noButton.onClick.AddListener(onClick);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            yesButton.onClick.RemoveAllListeners();
            noButton.onClick.RemoveAllListeners();
        }

        public void OnCancel(InputValue value)
        {
            noButton.onClick.Invoke();
        }
    }
}
