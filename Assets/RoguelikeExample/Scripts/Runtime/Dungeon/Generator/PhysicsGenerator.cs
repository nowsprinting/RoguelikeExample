// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace RoguelikeExample.Dungeon.Generator
{
    public static class PhysicsGenerator
    {
        /// <summary>
        /// ダンジョンマップをもとに、Scene上にGameObjectを配置する
        /// </summary>
        /// <param name="map"><c>MapGenerator</c>で生成されたダンジョンマップ</param>
        /// <returns>配置したGameObjectのルート</returns>
        public static GameObject Generate(MapChip[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var rootObject = new GameObject();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var newObject = CreateObject(map[i, j]);
                    newObject.transform.position = new Vector3(i, 0, -j);
                    newObject.transform.SetParent(rootObject.transform);
                }
            }

            return rootObject;
        }

        private static GameObject CreateObject(MapChip chip)
        {
            GameObject gameObject;
            switch (chip)
            {
                case MapChip.Wall:
                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case MapChip.Room:
                case MapChip.Corridor:
                    gameObject = new GameObject(); // 床なし（暫定）
                    break;
                case MapChip.UpStairs:
                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case MapChip.DownStairs:
                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return gameObject;
        }
    }
}
