// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoguelikeExample.UI
{
    /// <summary>
    /// はい/いいえダイアログ
    /// </summary>
    public class YesNoDialog : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI message;

        [SerializeField]
        private Button yesButton;

        [SerializeField]
        private Button noButton;

        private PlayerInputActions _inputActions;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        public void SetMessage(string messageString)
        {
            this.message.text = messageString;
        }

        public void SetOnYesButtonClickListener(UnityAction onClick)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(onClick);
        }

        public void SetOnNoButtonClickListener(UnityAction onClick)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(onClick);
        }
    }
}
