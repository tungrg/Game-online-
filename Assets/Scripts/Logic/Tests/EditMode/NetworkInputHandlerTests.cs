using NUnit.Framework;
using UnityEngine;

namespace Game.Logic.Tests.EditMode
{
    public class NetworkInputHandlerTests
    {
        [Test]
        public void Component_CanBeAdded()
        {
            // Arrange & Act
            GameObject go = new GameObject();
            var handler = go.AddComponent<NetworkInputHandler>();

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
            var handler = go.AddComponent<NetworkInputHandler>();

            // Act & Assert
            Assert.IsTrue(handler is Fusion.Sockets.INetworkRunnerCallbacks,
                "NetworkInputHandler must implement INetworkRunnerCallbacks.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
