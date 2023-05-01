// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using RoguelikeExample.Controller;
using RoguelikeExample.Dungeon.Generator;
using RoguelikeExample.Random;
using RoguelikeExample.UI;
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
        [SerializeField, Tooltip("Enemy Manager")]
        internal EnemyManager enemyManager;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("Player Character")]
        internal PlayerCharacterController playerCharacterController;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("Yes/No Dialog")]
        internal YesNoDialog yesNoDialog;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("レベル（ゲーム開始レベル）")]
        private int level = 1;

        [SerializeField, Tooltip("ダンジョンのマップ幅")]
        private int width = 50;

        [SerializeField, Tooltip("ダンジョンのマップ高さ")]
        private int height = 30;

        [SerializeField, Tooltip("ダンジョンの最大部屋数")]
        private int roomCount = 6;

        [SerializeField, Tooltip("最大部屋サイズ")]
        private int maxRoomSize = 10;

        [SerializeField, Tooltip("ルートとなる擬似乱数のシード値（再生モードに入ってから変更しても無効）")]
        private string randomSeed;

        private IRandom _random; // ルートとなる擬似乱数発生器
        private readonly Turn _turn = new Turn();

        private MapChip[,] _map; // 現在のレベルのマップ

        private void Awake()
        {
            _turn.OnPhaseTransition += HandlePhaseTransition;
        }

        private void OnDestroy()
        {
            _turn.OnPhaseTransition -= HandlePhaseTransition;
        }

        private void HandlePhaseTransition(object sender, EventArgs _)
        {
            var turn = (Turn)sender;
            switch (turn.State)
            {
                case TurnState.OnStairs:
                    OpenOnStairsDialog();
                    break;
            }
        }

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

            Debug.Log($"Dungeon root random is: {_random}"); // 擬似乱数発生器のシード値をログ出力（再現可能にするため）

            if (enemyManager != null)
            {
                IRandom newRandom = new RandomImpl(_random.Next());
                enemyManager.Initialize(newRandom, _turn, playerCharacterController);
            }

            if (playerCharacterController != null)
            {
                IRandom newRandom = new RandomImpl(_random.Next());
                playerCharacterController.Initialize(newRandom, _turn, enemyManager);
            }

            NewLevel(StairsDirection.Down);
        }

        private void NewLevel(StairsDirection stairsDirection)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _map = MapGenerator.Generate(width, height, roomCount, maxRoomSize, _random);
            var root = PhysicsGenerator.Generate(_map);
            root.name = $"Level {level}";
            root.transform.parent = transform;

            if (enemyManager != null)
            {
                enemyManager.NewLevel(level, _map);
            }

            if (playerCharacterController != null)
            {
                var newPosition = stairsDirection == StairsDirection.Up
                    ? _map.GetDownStairsPosition()
                    : _map.GetUpStairsPosition();
                playerCharacterController.NewLevel(_map, newPosition);
            }
        }

        private void OpenOnStairsDialog()
        {
            var playerLocation = playerCharacterController.MapLocation();
            if (_map.IsUpStairs(playerLocation.column, playerLocation.row))
            {
                yesNoDialog.SetMessage(level == 1 ? "Exit dungeon?" : "Go up?");
                yesNoDialog.SetOnYesButtonClickListener(() => LevelTransition(StairsDirection.Up));
            }
            else
            {
                yesNoDialog.SetMessage("Go down?");
                yesNoDialog.SetOnYesButtonClickListener(() => LevelTransition(StairsDirection.Down));
            }

            yesNoDialog.SetOnNoButtonClickListener(() =>
            {
                yesNoDialog.gameObject.SetActive(false);
                _turn.NextPhase().Forget();
            });

            yesNoDialog.gameObject.SetActive(true);
        }

        private void LevelTransition(StairsDirection stairsDirection)
        {
            level += (int)stairsDirection;
            if (level == 0)
            {
                // TODO: 地上
            }

            NewLevel(stairsDirection);

            yesNoDialog.gameObject.SetActive(false);
            _turn.Reset();
        }

        private enum StairsDirection
        {
            Up = -1,
            Down = +1,
        }
    }
}
