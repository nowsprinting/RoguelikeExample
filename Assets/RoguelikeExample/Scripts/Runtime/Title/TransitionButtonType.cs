// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace RoguelikeExample.Title
{
    /// <summary>
    /// 画面遷移ボタンの種類
    /// </summary>
    public enum TransitionButtonType
    {
        Forward = 0, // 進む, Yes, Go, 選択肢を選択
        Back, // 戻る, No, Cancel
        Start,
        Ranking,
        Option,
        Credit,
        Exit,
    }
}
