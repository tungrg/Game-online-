using NUnit.Framework;
using UnityEngine;

namespace Game.Legacy.Tests.EditMode
{
    public class PlayerControllerTests
    {
        [Test]
        public void Awake_CachesCharacterController()
        {
            // Arrange
            GameObject go = new GameObject();
            go.AddComponent<CharacterController>();
            var controller = go.AddComponent<PlayerController>();

            // Act — Awake is called by AddComponent

            // Assert
            var cc = (CharacterController)typeof(PlayerController)
                .GetField("characterController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(controller);
            Assert.IsNotNull(cc, "Awake should cache the CharacterController.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Awake_WithMissingCharacterController_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();

            // Act & Assert
            Assert.DoesNotThrow(() => go.AddComponent<PlayerController>(),
                "Awake should not throw even without CharacterController.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
