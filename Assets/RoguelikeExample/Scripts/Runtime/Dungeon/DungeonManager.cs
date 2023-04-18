// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.Controller;
using UnityEngine;

namespace RoguelikeExample.Dungeon
{
    /// <summary>
    /// Dungeonシーンを管理する
    ///
    /// 責務
    /// - ダンジョンマップの生成
    /// - 階段に到達したときの判定、昇降処理
    /// - インゲーム終了処理（死亡・地上に出る）
    /// - アイテムを拾う処理
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Player Character")]
        internal PlayerCharacterController playerCharacter;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("レベル")]
        private int level = 1;

        private MapChip[,] _map; // 現在のレベルのマップ

        public EnemyManager EnemyManager { get; private set; } // 現在のレベルの敵キャラクター管理
    }
}
