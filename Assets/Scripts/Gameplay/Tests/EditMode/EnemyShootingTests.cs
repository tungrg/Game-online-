using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.Tests.EditMode
{
    public class EnemyShootingTests
    {
        [Test]
        public void DefaultTeam_IsZero_BeforeSpawned()
        {
            // Arrange
            GameObject go = new GameObject();
            var enemy = go.AddComponent<EnemyShooting>();

            // Act & Assert
            Assert.AreEqual(0, enemy.Team, "Team should be 0 before Spawned() is called.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultAttackRange_Is15()
        {
            // Arrange
            GameObject go = new GameObject();
            var enemy = go.AddComponent<EnemyShooting>();

            // Act & Assert
            Assert.AreEqual(15f, enemy.attackRange, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultFireRate_Is2()
        {
            // Arrange
            GameObject go = new GameObject();
            var enemy = go.AddComponent<EnemyShooting>();

            // Act & Assert
            Assert.AreEqual(2f, enemy.fireRate, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultBulletSpeed_Is8()
        {
            // Arrange
            GameObject go = new GameObject();
            var enemy = go.AddComponent<EnemyShooting>();

            // Act & Assert
            Assert.AreEqual(8f, enemy.bulletSpeed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultSearchRange_Is20()
        {
            // Arrange
            GameObject go = new GameObject();
            var enemy = go.AddComponent<EnemyShooting>();

            // Act & Assert
            Assert.AreEqual(20f, enemy.searchRange, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
