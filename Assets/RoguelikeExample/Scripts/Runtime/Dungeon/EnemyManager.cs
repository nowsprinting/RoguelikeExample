// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

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
    /// レベル（階層）移動ごとに <c>DungeonManager</c> によって破棄・再生性される
    ///
    /// 責務
    /// - 敵キャラクターの初期配置
    /// - 敵キャラクターの出現数が規定数を下回っているとき、視界の外に敵をランダムに生成
    /// - 敵ターンの処理。敵キャラクターの<c>DoAction</c>を順番に呼ぶ
    /// - 敵キャラクター同士の衝突判定（移動先に敵キャラがいるか判定）
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        private MapChip[,] _map;
        private int _level;
        private int _maxInstantiateEnemies;
        private IRandom _random;
        private List<EnemyRace> _enemyRaces; // このレベルに出現する敵種族のリスト

        /// <summary>
        /// インスタンス生成時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="level">当該レベル==敵キャラクターのレベル</param>
        /// <param name="maxInstantiateEnemies">出現する敵キャラクターの最大数</param>
        /// <param name="random">擬似乱数生成器</param>
        public void Initialize(MapChip[,] map, int level, int maxInstantiateEnemies, IRandom random)
        {
            _map = map;
            _level = level;
            _maxInstantiateEnemies = maxInstantiateEnemies;
            _random = random;

            _enemyRaces = new List<EnemyRace>();
            // TODO: レベル条件を満たすものを抽出して入れておく
            // TODO: ランタイムでAssetDatabase使えない

            // TODO: 敵キャラクターの初期配置（無理に最大数出すまで頑張りはしない）
        }

        private void Update()
        {
            // TODO: 敵の数が規定数を下回っているとき、視界の外に敵をランダムに出現させる
        }

        /// <summary>
        /// 指定座標にすでに敵キャラクターがいればインスタンスを返す
        /// なお、現在座標ではなく、当該ターンの移動先座標で判定する
        /// </summary>
        /// <param name="location">判定するマップ座標</param>
        /// <returns>ヒットした敵キャラクターインスタンス。存在しない場合はnull</returns>
        public EnemyCharacterController ExistEnemy((int column, int row) location)
        {
            return GetComponentsInChildren<EnemyCharacterController>()
                .FirstOrDefault(enemyCharacterController => enemyCharacterController.NextLocation == location);
            // Note: 自分自身を除外していないが、移動しないという結果に変わりはないので許容
        }

        /// <summary>
        /// 敵キャラクターのターン（思考）
        /// </summary>
        /// <param name="playerCharacterController"></param>
        public void ThinkActionEnemies(PlayerCharacterController playerCharacterController)
        {
            foreach (var enemyCharacterController in GetComponentsInChildren<EnemyCharacterController>())
            {
                if ((bool)enemyCharacterController)
                    enemyCharacterController.ThinkAction(this, playerCharacterController);
            }
        }

        /// <summary>
        /// 敵キャラクターのターン（行動）
        /// </summary>
        /// <param name="animationMillis">移動アニメーションにかける時間（ミリ秒）</param>
        public async UniTask DoActionEnemies(int animationMillis)
        {
            var tasks = new List<UniTask>();
            foreach (var enemyCharacterController in GetComponentsInChildren<EnemyCharacterController>())
            {
                if ((bool)enemyCharacterController)
                    tasks.Add(enemyCharacterController.MoveToNextLocation(animationMillis));
            }

            await UniTask.WhenAll(tasks);

            // TODO: 攻撃
        }

        private EnemyCharacterController CreateEnemy((int column, int row) location)
        {
            var race = _enemyRaces[_random.Next(_enemyRaces.Count)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RoguelikeExample/Prefabs/EnemyCharacter.prefab"); // TODO: ランタイムでAssetDatabase使えない
            var enemy = Instantiate(prefab, transform); // 自分の下に生成
            var enemyCharacterController = enemy.GetComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(race, _level, _random, _map, location);

            return enemyCharacterController;
        }
    }
}
