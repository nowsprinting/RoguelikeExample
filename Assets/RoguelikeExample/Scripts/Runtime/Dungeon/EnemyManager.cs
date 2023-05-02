// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RoguelikeExample.Controller;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Random;
using UnityEditor;
using UnityEngine;

namespace RoguelikeExample.Dungeon
{
    /// <summary>
    /// ダンジョンに出現する敵キャラクターを管理する
    ///
    /// 責務
    /// - 敵キャラクターの初期配置
    /// - 敵キャラクターの出現数が規定数を下回っているとき、視界の外に敵をランダムに生成
    /// - 敵ターンの処理。敵キャラクターの<c>DoAction</c>を順番に呼ぶ
    /// - 敵キャラクター同士の衝突判定（移動先に敵キャラがいるか判定）
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField, Tooltip("敵キャラクターの床面積あたり最大出現数")]
        internal float maxInstantiateEnemiesPercentageOfFloor = 0.01f;

        [SerializeField, Tooltip("敵キャラクターのポップ試行回数")]
        internal int popTrialCount = 5;

        // インゲーム開始時に <c>DungeonManager</c> から設定されるもの
        private IRandom _random;

        // インゲーム開始時に <c>DungeonManager</c> から設定されるもの（テストでは省略されることもある）
        private PlayerCharacterController _playerCharacterController;

        // 新しいレベルに移動したときに設定されるもの
        private int _level;
        private MapChip[,] _map;
        private int _floorCount; // 床の数（部屋+通路）
        private List<EnemyRace> _enemyRaces; // このレベルに出現する敵種族のリスト

        /// <summary>
        /// インゲーム開始時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="random">擬似乱数生成器インスタンス</param>
        /// <param name="playerCharacterController">プレイヤーキャラクターのコントローラー</param>
        public void Initialize(IRandom random, PlayerCharacterController playerCharacterController = null)
        {
            _random = random;
            _playerCharacterController = playerCharacterController;
        }

        private void Awake()
        {
            Turn.OnPhaseTransition += HandlePhaseTransition;
        }

        private void OnDestroy()
        {
            Turn.OnPhaseTransition -= HandlePhaseTransition;
        }

        private async void HandlePhaseTransition(object sender, EventArgs _)
        {
            var turn = (Turn)sender;
            switch (turn.State)
            {
                case TurnState.EnemyAction:
                    await WaitForAllEnemiesAction();
                    turn.NextPhase().Forget();
                    break;
                case TurnState.EnemyPopup:
                    RefillEnemies();
                    turn.NextPhase().Forget();
                    break;
            }
        }

        private async UniTask WaitForAllEnemiesAction()
        {
            while (GetComponentsInChildren<EnemyCharacterController>().Any(x => x.HasIncompleteAction))
            {
                await UniTask.NextFrame(); // すべての敵キャラクターが行動を終えるまで待機
            }
        }

        private void RefillEnemies()
        {
            var enemies = GetComponentsInChildren<EnemyCharacterController>();
            if (enemies.Length < _floorCount * maxInstantiateEnemiesPercentageOfFloor)
            {
                CreateEnemies(1); // 補充は1ターンに1体ずつ
            }
        }

        /// <summary>
        /// 新しいレベルに移動したときに <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="level">当該レベル==敵キャラクターのレベル</param>
        /// <param name="map">当該レベルのマップ</param>
        public void NewLevel(int level, MapChip[,] map)
        {
            _level = level;
            _map = map;
            _floorCount = map.Cast<MapChip>().Count(mapChip => mapChip == MapChip.Room || mapChip == MapChip.Corridor);
            _enemyRaces = new List<EnemyRace>();

            foreach (var path in AssetDatabase.FindAssets("t:EnemyRace", new[] { "Assets/RoguelikeExample" })
                         .Select(AssetDatabase.GUIDToAssetPath))
            {
                var race = AssetDatabase.LoadAssetAtPath<EnemyRace>(path); // TODO: ランタイムでAssetDatabaseは使えない
                if (race.lowestSpawnLevel <= _level && _level <= race.highestSpawnLevel)
                {
                    _enemyRaces.Add(race);
                }
            }

            CreateEnemies((int)(_floorCount * maxInstantiateEnemiesPercentageOfFloor));
        }

        private void CreateEnemies(int count = 1)
        {
            if (_enemyRaces.Count == 0)
            {
                Debug.LogWarning($"Level {_level} に出現できる敵種族が定義されていません");
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var location = GetPopLocation();
                if (location.column == -1)
                {
                    continue;
                }

                var race = _enemyRaces[_random.Next(_enemyRaces.Count)];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/RoguelikeExample/Prefabs/EnemyCharacter.prefab"); // TODO: ランタイムでAssetDatabaseは使えない
                var enemyObject = Instantiate(prefab, transform); // 自分の下に配置
                enemyObject.name = race.name;

                var enemy = enemyObject.GetComponent<EnemyCharacterController>();
                enemy.Initialize(race, _level, _map, location, _random, this, _playerCharacterController);
            }
        }

        private (int column, int row) GetPopLocation()
        {
            for (var i = 0; i < popTrialCount; i++) // 数回試行する
            {
                var column = _random.Next(_map.GetLength(0));
                var row = _random.Next(_map.GetLength(1));

                if (!(_map.IsRoom(column, row) || _map.IsCorridor(column, row)))
                {
                    continue;
                }

                if (ExistEnemy((column, row)) != null)
                {
                    continue;
                }

                if (_playerCharacterController != null &&
                    _playerCharacterController.MapLocation().column == column &&
                    _playerCharacterController.MapLocation().row == row)
                {
                    continue;
                }

                return (column, row);
            }

            return (-1, -1); // 配置に失敗
        }

        /// <summary>
        /// 指定座標にすでに敵キャラクターがいればインスタンスを返す
        /// なお、現在座標ではなく、現在ターンの移動先座標で判定する
        /// </summary>
        /// <param name="location">判定するマップ座標</param>
        /// <returns>ヒットした敵キャラクターインスタンス。存在しない場合はnull</returns>
        public EnemyCharacterController ExistEnemy((int column, int row) location)
        {
            return GetComponentsInChildren<EnemyCharacterController>()
                .FirstOrDefault(enemyCharacterController => enemyCharacterController.NextLocation == location);
            // Note: 呼び出し元を除外していないが、移動しないという結果に変わりはないので許容
        }
    }
}
