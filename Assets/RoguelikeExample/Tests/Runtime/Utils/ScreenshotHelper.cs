// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RoguelikeExample.Utils
{
    public static class ScreenshotHelper
    {
        /// <summary>
        /// <c>ScreenCapture.CaptureScreenshot</c>を使用してスクリーンショットをファイルに保存します
        /// </summary>
        public static IEnumerator CaptureScreenshot(
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            if (Application.isBatchMode)
            {
                yield break; // スクリーンショット撮影の前提である<c>WaitForEndOfFrame</c>が動作しないため
            }

            if (!Application.isEditor)
            {
                yield break; // <c>ScreenCapture.CaptureScreenshot</c>の保存先パスが異なるため
            }

            yield return new WaitForEndOfFrame(); // スクリーンショット撮影の前提
            // Asyncテストの場合、<c>UniTask.WaitForEndOfFrame(MonoBehaviour)</c>を使用します。Required UniTask v2.3.1+

            var path = CreateStorePath(callerFilePath);
            var filename = Path.Combine(path, $"{callerMemberName}.png");
            ScreenCapture.CaptureScreenshot(filename);
        }

        private static string CreateStorePath(string callerFilePath)
        {
            var className = callerFilePath
                .Substring(callerFilePath.LastIndexOf("/", StringComparison.Ordinal) + 1)
                .Replace(".cs", string.Empty);
            var relativePath = Path.Combine("Logs", "Screenshots", className);
            Directory.CreateDirectory(Path.GetFullPath(relativePath));
            return relativePath;
        }
    }
}
