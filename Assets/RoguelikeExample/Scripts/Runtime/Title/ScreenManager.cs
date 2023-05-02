// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoguelikeExample.Title
{
    /// <summary>
    /// タイトル画面の画面遷移ステートマシン
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private Screens _screen = Screens.Title; // 現在表示している画面==ステート
        private Canvas _canvas;

        private void Start()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            Assert.IsNotNull(_canvas);
        }

        /// <summary>
        /// 画面遷移
        /// </summary>
        /// <param name="sender">遷移元画面のルートオブジェクト</param>
        /// <param name="buttonType">押された遷移ボタン種類</param>
        public void Transit(GameObject sender, TransitionButtonType buttonType)
        {
            switch (_screen)
            {
                case Screens.Title:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Start:
                            Transit(sender, Screens.StageSelect);
                            break;
                        case TransitionButtonType.Ranking:
                            Transit(sender, Screens.Ranking);
                            break;
                        case TransitionButtonType.Option:
                            Transit(sender, Screens.Option);
                            break;
                        case TransitionButtonType.Credit:
                            Transit(sender, Screens.Credit);
                            break;
                        case TransitionButtonType.Exit:
                            Transit(sender, Screens.Exit);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.StageSelect:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Forward:
                            Transit(sender, Screens.DifficultySelect);
                            break;
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.Title);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.DifficultySelect:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Forward:
                            Transit(sender, Screens.Ready);
                            break;
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.StageSelect);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.Ready:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Forward:
                            StartInGame();
                            break;
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.DifficultySelect);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.Ranking:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.Title);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.Option:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.Title);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.Credit:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.Title);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                case Screens.Exit:
                    switch (buttonType)
                    {
                        case TransitionButtonType.Forward:
                            ExitGame();
                            break;
                        case TransitionButtonType.Back:
                            Transit(sender, Screens.Title);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_screen), _screen, null);
            }
        }

        private void Transit(GameObject sender, Screens nextScreen)
        {
            var nextScreenObject = GetScreenObject(nextScreen);

            _screen = nextScreen;
            sender.SetActive(false);
            nextScreenObject.SetActive(true);
        }

        private GameObject GetScreenObject(Screens screen)
        {
            var screenTransform = _canvas.transform.Find(screen.ToString());
            if (screenTransform == null)
            {
                throw new ArgumentException($"Canvas下に{screen.ToString()}が見つかりません");
            }

            return screenTransform.gameObject;
        }

        private static void StartInGame()
        {
            SceneManager.LoadScene("Dungeon");
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private enum Screens
        {
            Title = 0,
            StageSelect,
            DifficultySelect,
            Ready,
            Ranking,
            Option,
            Credit,
            Exit,
        }
    }
}
