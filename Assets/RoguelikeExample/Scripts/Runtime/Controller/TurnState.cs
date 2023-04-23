// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// 行動ターンのステート（a.k.a. 行動フェーズ）定義
    ///
    /// ステート遷移はREADME.md「行動ターンのステート遷移図」を参照
    /// </summary>
    public enum TurnState
    {
        PlayerIdol = 0, // プレイヤー入力待ち（PlayerRunは排他）
        PlayerRun, // プレイヤー高速移動中の思考（PlayerIdolとは排他）
        PlayerAction, // プレイヤー行動実行
        EnemyAction, // 敵思考・行動実行
        EnemyPopup, // 敵キャラクター出現数が不足していたら補充
    }
}
