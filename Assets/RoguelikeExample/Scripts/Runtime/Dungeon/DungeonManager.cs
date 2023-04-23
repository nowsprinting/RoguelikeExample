// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using RoguelikeExample.Controller;
using RoguelikeExample.Random;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField, Tooltip("Enemy Manager")]
        internal EnemyManager enemyManager;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [FormerlySerializedAs("playerCharacter")]
        [SerializeField, Tooltip("Player Character")]
        internal PlayerCharacterController playerCharacterController;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("レベル")]
        private int level = 1;

        // TODO: マップ生成に使用するパラメータをSerializeFieldにする

        [SerializeField, Tooltip("ルートとなる擬似乱数のシード値（再生モードに入ってから変更しても無効）")]
        private string randomSeed;

        private IRandom _random; // ルートとなる擬似乱数発生器
        private readonly Turn _turn = new Turn(); // 行動ターンのステート

        private MapChip[,] _map; // 現在のレベルのマップ

        private void Start()
        {
            if (string.IsNullOrEmpty(randomSeed))
            {
                _random = new RandomImpl();
            }
            else
            {
                var seed = Convert.ToInt32(randomSeed);
                _random = new RandomImpl(seed);
            }

            if (enemyManager != null)
            {
                IRandom newRandom = new RandomImpl(_random.Next());
                enemyManager.Initialize(newRandom, playerCharacterController);
            }

            if (playerCharacterController != null)
            {
                IRandom newRandom = new RandomImpl(_random.Next());
                playerCharacterController.Initialize(newRandom, _turn, enemyManager);
            }

            NewLevel();
        }

        private void NewLevel()
        {
            // TODO: マップ生成
            // TODO: enemyManagerとplayerCharacterControllerにマップを渡す
        }
    }
}
