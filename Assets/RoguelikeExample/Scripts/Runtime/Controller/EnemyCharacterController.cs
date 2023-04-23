// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
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
        // インスタンス生成時に <c>EnemyManager</c> から設定されるもの（テストでは省略されることもある）
        private EnemyManager _enemyManager;
        private PlayerCharacterController _playerCharacterController;

        public EnemyStatus Status { get; private set; }
        private AbstractAI _ai;
        public bool HasIncompleteAction { get; private set; }

        /// <summary>
        /// インスタンス生成時に <c>EnemyManager</c> から設定される
        /// </summary>
        /// <param name="race">敵種族</param>
        /// <param name="level">敵レベル</param>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="location">キャラクターの初期座標</param>
        /// <param name="random">このキャラクターが消費する擬似乱数生成器インスタンス</param>
        /// <param name="enemyManager">敵管理（親）</param>
        /// <param name="playerCharacterController">プレイヤーキャラクターのコントローラー</param>
        public void Initialize(EnemyRace race, int level, MapChip[,] map, (int colum, int row) location, IRandom random,
            EnemyManager enemyManager = null, PlayerCharacterController playerCharacterController = null)
        {
            _random = random;
            _enemyManager = enemyManager;
            _playerCharacterController = playerCharacterController;

            Status = new EnemyStatus(race, level);
            _ai = AIFactory.CreateAI(race.aiType, new RandomImpl(random.Next()));
            _map = map;
            SetPositionFromMapLocation(location.colum, location.row);

            var textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = race.displayCharacter;
            }
        }

        private void Awake()
        {
            Turn.OnPhaseTransition += HandlePhaseTransition;
        }

        private void OnDestroy()
        {
            Turn.OnPhaseTransition -= HandlePhaseTransition;
        }

        private void HandlePhaseTransition(object sender, EventArgs _)
        {
            if (((Turn)sender).State == TurnState.EnemyAction)
            {
                DoAction().Forget();
            }
        }

        internal async UniTask DoAction()
        {
            if (_ai == null)
            {
                return; // AIがないときは何もしない
            }

            var pcLocation = _playerCharacterController.NextLocation;
            var dist = _ai.Think(_map, this, _playerCharacterController);

            if (_map.IsWall(dist.column, dist.row))
            {
                return; // 移動先が壁なら何もしない
            }

            if (_enemyManager.ExistEnemy(dist))
            {
                return; // 移動先が敵キャラのときは何もしない
            }

            HasIncompleteAction = true;

            if (dist == pcLocation)
            {
                // TODO: 攻撃
            }
            else
            {
                NextLocation = dist; // 移動先をセット
                await MoveToNextLocation(_playerCharacterController.AnimationMillis());
            }

            HasIncompleteAction = false;
        }
    }
}
