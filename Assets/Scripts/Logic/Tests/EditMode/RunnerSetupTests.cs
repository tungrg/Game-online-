using NUnit.Framework;
using UnityEngine;

namespace Game.Logic.Tests.EditMode
{
    public class RunnerSetupTests
    {
        [Test]
        public void Component_CanBeAdded()
        {
            // Arrange & Act
            GameObject go = new GameObject();
            var setup = go.AddComponent<RunnerSetup>();

            // Assert
            Assert.IsNotNull(setup);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Awake_WithNoNetworkRunner_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();

            // Act & Assert — Awake is called by AddComponent, should early-return when no Runner
            Assert.DoesNotThrow(() => go.AddComponent<RunnerSetup>(),
                "Awake should early-return gracefully when no NetworkRunner is present.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
