// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.Dungeon;
using RoguelikeExample.Random;

namespace RoguelikeExample.AI
{
    /// <summary>
    /// 敵キャラクター行動AIの抽象クラス
    /// </summary>
    public abstract class AbstractAI
    {
        protected IRandom _random;

        protected AbstractAI(IRandom random)
        {
            _random = random;
        }

        /// <summary>
        /// 敵キャラクターの行動を返す
        /// </summary>
        /// <param name="map">ダンジョンのマップ</param>
        /// <param name="myLocation">当該キャラの座標</param>
        /// <param name="targetLocation">プレイヤーキャラクターの座標</param>
        /// <returns>移動先候補の座標。PC座標を指すときには攻撃する</returns>
        public abstract (int column, int row) ThinkAction(
            MapChip[,] map,
            (int column, int row) myLocation,
            (int column, int row) targetLocation);

        /// <summary>
        /// 接敵しているか判定
        /// </summary>
        /// <param name="myLocation">Location1</param>
        /// <param name="targetLocation">Location2</param>
        /// <returns>true: 接敵している</returns>
        protected static bool IsEngagement((int column, int row) myLocation, (int column, int row) targetLocation)
        {
            return myLocation.column - 1 <= targetLocation.column && targetLocation.column <= myLocation.column + 1 &&
                   myLocation.row - 1 <= targetLocation.row && targetLocation.row <= myLocation.row + 1;
        }
    }
}
