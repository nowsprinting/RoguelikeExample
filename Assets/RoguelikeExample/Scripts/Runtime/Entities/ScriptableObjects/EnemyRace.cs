// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.Serialization;

namespace RoguelikeExample.Entities.ScriptableObjects
{
    /// <summary>
    /// 敵の種族
    /// 当たり障りのない伝承とか実在の生きものとします
    /// </summary>
    [CreateAssetMenu(menuName = "RoguelikeExample/" + nameof(EnemyRace))]
    public class EnemyRace : ScriptableObject
    {
        [SerializeField, Tooltip("表示名")]
        internal string displayName;

        [SerializeField, Tooltip("説明・フレーバーテキスト")]
        internal string description;

        [SerializeField, Tooltip("表示記号")]
        internal string displayCharacter;

        // TODO: 消滅時演出

        [FormerlySerializedAs("ai")]
        [SerializeField, Tooltip("行動AI")]
        internal AI.AIType aiType;

        [SerializeField, Tooltip("最低出現レベル")]
        internal int lowestSpawnLevel = 1;

        [SerializeField, Tooltip("最高出現レベル")]
        internal int highestSpawnLevel = 1;

        [SerializeField, Tooltip("最大ヒットポイント（Lv1のとき）")]
        internal int maxHitPoint = 1;

        [SerializeField, Tooltip("防御力（Lv1のとき）")]
        internal int defense;

        [SerializeField, Tooltip("攻撃力（Lv1のとき）")]
        internal int attack;

        [SerializeField, Tooltip("倒したらもらえる経験値（Lv1のとき）")]
        internal int rewardExp;

        [SerializeField, Tooltip("倒したらもらえる通貨（Lv1のとき）")]
        internal int rewardGold;
    }
}
