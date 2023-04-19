// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.AI;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Random;
using TMPro;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// 敵キャラクターのコントローラー
    /// </summary>
    public class EnemyCharacterController : CharacterController
    {
        public EnemyStatus Status { get; private set; }
        private AbstractAI _ai;

        /// <summary>
        /// インスタンス生成時に <c>EnemyManager</c> から設定される
        /// </summary>
        /// <param name="race">敵種族</param>
        /// <param name="level">敵レベル</param>
        /// <param name="random">このキャラクターが消費する擬似乱数インスタンス</param>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="location">キャラクターの初期座標</param>
        public void Initialize(EnemyRace race, int level, IRandom random, MapChip[,] map, (int colum, int row) location)
        {
            Status = new EnemyStatus(race, level);
            _random = random;
            _ai = AIFactory.CreateAI(race.aiType, new RandomImpl(random.Next()));
            _map = map;
            SetPositionFromMapLocation(location.colum, location.row);

            var textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = race.displayCharacter;
            }
        }

        /// <summary>
        /// 自分のターンの行動を思考
        /// プレイヤーが1ターン終えるたびに呼ばれる
        /// </summary>
        /// <param name="enemyManager"></param>
        /// <param name="playerCharacterController"></param>
        public void ThinkAction(EnemyManager enemyManager, PlayerCharacterController playerCharacterController)
        {
            var pcLocation = playerCharacterController.NextLocation;
            var dist = _ai.ThinkAction(_map, this, playerCharacterController);

            if (dist == pcLocation)
            {
                return; // TODO: 攻撃
            }

            if (_map.IsWall(dist.column, dist.row))
            {
                return; // 移動先が壁なら何もしない
            }

            if (enemyManager.ExistEnemy(dist))
            {
                return; // 移動先が敵キャラのときは何もしない
            }

            // 移動先をセット
            NextLocation = dist;
        }
    }
}
