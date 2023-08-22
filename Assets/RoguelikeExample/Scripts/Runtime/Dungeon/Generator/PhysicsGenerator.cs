// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            switch (chip)
            {
                case MapChip.Wall:
                    return LoadFBX("Wall");
                case MapChip.Room:
                case MapChip.Corridor:
                    return LoadFBX("Floor");
                case MapChip.UpStairs:
                    return LoadFBX("UpStairs");
                case MapChip.DownStairs:
                    return LoadFBX("DownStairs");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static GameObject LoadFBX(string name)
        {
            const string BasePath = "Assets/RoguelikeExample/Prefabs";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(BasePath, $"{name}.fbx"));
            return Object.Instantiate(prefab);
        }
    }
}
