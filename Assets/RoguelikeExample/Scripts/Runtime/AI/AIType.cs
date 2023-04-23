// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace RoguelikeExample.AI
{
    /// <summary>
    /// 敵キャラクター行動AIの種類
    /// ScriptableObjectのInspectorで選択させるためのenum
    /// </summary>
    public enum AIType
    {
        /// <summary>
        /// なにもしないAI
        /// </summary>
        None = 0,

        /// <summary>
        /// 直線的に往復する行動AI。接敵したらその場で攻撃する
        /// <see cref="BackAndForthAI"/>
        /// </summary>
        BackAndForth,
    }
}
