using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class TankControllerTests
    {
        private System.Collections.Generic.List<TankController> _activeTanksList;

        [SetUp]
        public void SetUp()
        {
            _activeTanksList = (System.Collections.Generic.List<TankController>)typeof(TankController)
                .GetField("ActiveTanks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            _activeTanksList.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _activeTanksList.Clear();
        }

        [Test]
        public void GetActiveTanks_ReturnsEmptyListByDefault()
        {
            // Arrange & Act
            var tanks = TankController.GetActiveTanks();

            // Assert
            Assert.IsNotNull(tanks);
            Assert.AreEqual(0, tanks.Count);
        }

        [Test]
        public void DefaultMoveSpeed_Is10()
        {
            // Arrange
            GameObject go = new GameObject();
            var tc = go.AddComponent<TankController>();

            // Act & Assert
            Assert.AreEqual(10f, tc.moveSpeed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultRotateSpeed_Is10()
        {
            // Arrange
            GameObject go = new GameObject();
            var tc = go.AddComponent<TankController>();

            // Act & Assert
            Assert.AreEqual(10f, tc.rotateSpeed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultFriction_Is085()
        {
            // Arrange
            GameObject go = new GameObject();
            var tc = go.AddComponent<TankController>();

            // Act & Assert
            Assert.AreEqual(0.85f, tc.friction, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultTeam_IsZero()
        {
            // Arrange
            GameObject go = new GameObject();
            var tc = go.AddComponent<TankController>();

            // Act & Assert
            Assert.AreEqual(0, tc.Team);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void OnDestroy_RemovesFromActiveTanks()
        {
            // Arrange
            GameObject go = new GameObject();
            var tc = go.AddComponent<TankController>();
            _activeTanksList.Add(tc);
            Assert.AreEqual(1, _activeTanksList.Count);

            // Act
            Object.DestroyImmediate(go);

            // Assert
            Assert.AreEqual(0, _activeTanksList.Count, "Tank should be removed from ActiveTanks on destroy.");
        }
    }
}
