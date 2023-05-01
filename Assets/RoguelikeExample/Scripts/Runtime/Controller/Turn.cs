// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// 行動ターンのステートマシン
    ///
    /// <c>DungeonManager</c> がインゲーム開始時に生成・保持する
    /// 行動フェーズの遷移イベントを <c>OnPhaseTransition</c> で購読できる
    /// ステート遷移はREADME.md「行動ターンのステート遷移図」を参照
    /// </summary>
    public class Turn
    {
        /// <summary>
        /// 現在のターン数（レベル移動してもリセットされない）
        /// </summary>
        public int TurnCount { get; private set; } = 1;

        /// <summary>
        /// 現在のステート（a.k.a. 行動フェーズ）
        /// </summary>
        public TurnState State { get; private set; } = TurnState.PlayerIdol;

        /// <summary>
        /// プレイヤーが高速移動中かどうか
        /// 本来 <c>PlayerCharacterController</c> が持つべきものだが、利便性でここに持たせている
        /// </summary>
        public bool IsRun { get; set; }

        /// <summary>
        /// 行動フェーズ遷移イベント
        /// </summary>
        public event EventHandler OnPhaseTransition;

        /// <summary>
        /// 次のフェーズに遷移
        /// </summary>
        public async UniTask NextPhase()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            switch (State)
            {
                case TurnState.PlayerIdol:
                    State = TurnState.PlayerAction; // PlayerRunはスキップ
                    break;
                case TurnState.PlayerRun:
                case TurnState.PlayerAction:
                case TurnState.EnemyAction:
                    State = (TurnState)((int)State + 1);
                    break;
                case TurnState.EnemyPopup:
                    // TODO: is on stair?
                    State = IsRun ? TurnState.PlayerRun : TurnState.PlayerIdol; // 高速移動中はPlayerIdolをスキップ
                    TurnCount++;
                    break;
                case TurnState.OnStairs:
                    State = TurnState.EnemyAction; // 階段キャンセル
                    break;
                case TurnState.Dead:
                    break; // Nextでは遷移しない
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnPhaseTransition?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 高速移動を止めてIdolに戻る
        /// 移動先候補がない（行き止まり）もしくは、移動先候補に敵キャラクターがいたときに使用される想定。
        /// 移動後に呼ばないこと（2回行動になってしまう）
        /// </summary>
        public void CanselRun()
        {
            Assert.IsTrue(State == TurnState.PlayerRun, "CanselRunはPlayerRunのときしか呼ばれない");

            State = TurnState.PlayerIdol;
            IsRun = false;
            OnPhaseTransition?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 階段の座標に乗ったときに <c>NextPhase</c> のかわりに呼ばれる
        /// </summary>
        public void OnStairs()
        {
            Assert.IsTrue(State == TurnState.PlayerAction, "OnStairsにはPlayerActionからしか遷移しない");

            State = TurnState.OnStairs;
            IsRun = false;
            OnPhaseTransition?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 行動フェーズをリセット
        /// レベルを移動したときに呼ばれる想定
        /// </summary>
        public void Reset()
        {
            State = TurnState.PlayerIdol;
            IsRun = false;
            OnPhaseTransition?.Invoke(this, EventArgs.Empty);
        }
    }
}
