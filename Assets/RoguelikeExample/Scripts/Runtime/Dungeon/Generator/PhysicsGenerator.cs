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
            var root = new GameObject();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    GameObject gameObject;
                    switch (map[i, j])
                    {
                        case MapChip.Wall:
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            break;
                        case MapChip.Room:
                        case MapChip.Corridor:
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                            gameObject.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
                            break;
                        case MapChip.UpStair:
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            break;
                        case MapChip.DownStair:
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    gameObject.transform.position = new Vector3(i, 0, j);
                    gameObject.transform.SetParent(root.transform);
                }
            }

            return root;
        }
    }
}
