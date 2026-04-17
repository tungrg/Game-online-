using NUnit.Framework;
using UnityEngine;

namespace Game.Logic.Tests.EditMode
{
    public class PlayerInputHandlerTests
    {
        [Test]
        public void Component_CanBeAdded()
        {
            // Arrange & Act
            GameObject go = new GameObject();
            var handler = go.AddComponent<PlayerInputHandler>();

            // Assert
            Assert.IsNotNull(handler);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ImplementsINetworkRunnerCallbacks()
        {
            // Arrange
            GameObject go = new GameObject();
            var handler = go.AddComponent<PlayerInputHandler>();

            // Act & Assert
            Assert.IsTrue(handler is Fusion.Sockets.INetworkRunnerCallbacks,
                "PlayerInputHandler must implement INetworkRunnerCallbacks.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ShootBuffer_DefaultIsFalse()
        {
            // Arrange
            GameObject go = new GameObject();
            var handler = go.AddComponent<PlayerInputHandler>();

            // Act
            bool buffer = (bool)typeof(PlayerInputHandler)
                .GetField("_shootBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(handler);

            // Assert
            Assert.IsFalse(buffer, "Shoot buffer should default to false.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
