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
        private EnemyStatus _status;
        private AbstractAI _ai;

        /// <summary>
        /// インスタンス生成時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="race">敵種族</param>
        /// <param name="level">敵レベル</param>
        /// <param name="random">このキャラクターが消費する擬似乱数インスタンス</param>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="location">キャラクターの初期座標</param>
        public void Initialize(EnemyRace race, int level, IRandom random, MapChip[,] map, (int colum, int row) location)
        {
            _status = new EnemyStatus(race, level);
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
        /// 自分のターンの行動
        /// プレイヤーが1ターン終えるたびに呼ばれる
        /// </summary>
        /// <param name="pcLocation">プレイキャラクターのマップ座標</param>
        public void DoAction((int column, int row) pcLocation)
        {
            var nextLocation = _ai.ThinkAction(ref _map, GetMapLocation(), pcLocation);

            if (nextLocation.column == pcLocation.column && nextLocation.row == pcLocation.row)
            {
                // TODO: 攻撃
            }

            if (_map.IsWall(nextLocation.column, nextLocation.row))
            {
                return; // 移動先が壁なら何もしない
            }

            // TODO: 移動先が敵キャラのときも何もしない。判定ロジックはDungeonManagerに持つ？

            // 移動
            SetPositionFromMapLocation(nextLocation.column, nextLocation.row);
        }
    }
}
