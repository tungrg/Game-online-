using NUnit.Framework;
using UnityEngine;

namespace Game.Legacy.Tests.EditMode
{
    public class CameraFollowTests
    {
        [Test]
        public void AssignCamera_WithNullCinemachine_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();
            var cameraFollow = go.AddComponent<CameraFollow>();
            // No CinemachineCamera component added — Awake will log error but not crash

            GameObject target = new GameObject("Target");

            // Act & Assert
            Assert.DoesNotThrow(() => cameraFollow.AssignCamera(target.transform),
                "AssignCamera should not throw when CinemachineCamera is null.");

            // Cleanup
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(go);
        }
    }
}
