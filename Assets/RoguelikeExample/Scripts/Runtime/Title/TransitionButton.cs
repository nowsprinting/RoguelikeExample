// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoguelikeExample.Title
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public class TransitionButton : MonoBehaviour
    {
        [SerializeField]
        private TransitionButtonType type;

        [SerializeField]
        private bool defaultSelected;

        private void OnEnable()
        {
            if (defaultSelected)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }

            GetComponent<Button>().onClick.AddListener(Transit);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(Transit);
        }

        private void Transit()
        {
            var screenManager = FindAnyObjectByType<ScreenManager>();
            screenManager.Transit(transform.parent.gameObject, type);
        }
    }
}
