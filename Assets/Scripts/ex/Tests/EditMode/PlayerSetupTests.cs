using NUnit.Framework;
using UnityEngine;

namespace Game.Legacy.Tests.EditMode
{
    public class PlayerSetupTests
    {
        [Test]
        public void Component_CanBeAdded()
        {
            // Arrange
            GameObject go = new GameObject();

            // Act
            var setup = go.AddComponent<PlayerSetup>();

            // Assert
            Assert.IsNotNull(setup);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetupCamera_WithNullCameraFollow_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();
            var setup = go.AddComponent<PlayerSetup>();
            // No CameraFollow component — Object.HasStateAuthority is false in EditMode,
            // so the method early-returns. This verifies the guard works.

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                // Call via reflection since it depends on NetworkBehaviour.Object
                var method = typeof(PlayerSetup).GetMethod("SetupCamera");
                method.Invoke(setup, new object[] { go.transform });
            });

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
