// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace RoguelikeExample.Controller
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        public Transform trackedTarget;
        public Vector3 relativePosition = new Vector3(0, 1, -10);

        private void Update()
        {
            if (trackedTarget == null)
                return;

            var targetPosition = trackedTarget.position;
            transform.position = targetPosition + relativePosition;
            transform.LookAt(targetPosition);
        }
    }
}
