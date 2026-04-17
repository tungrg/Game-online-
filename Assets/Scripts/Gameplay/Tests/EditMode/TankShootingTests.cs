using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class TankShootingTests
    {
        [Test]
        public void DefaultFireCooldown_Is1Point5()
        {
            // Arrange
            GameObject go = new GameObject();
            var ts = go.AddComponent<TankShooting>();

            // Act & Assert
            Assert.AreEqual(1.5f, ts.fireCooldown, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BulletSpeed_DefaultIs20()
        {
            // Arrange
            GameObject go = new GameObject();
            var ts = go.AddComponent<TankShooting>();

            // Act & Assert — bulletSpeed is public, access directly
            Assert.AreEqual(20f, ts.bulletSpeed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BulletPrefab_DefaultIsNull()
        {
            // Arrange
            GameObject go = new GameObject();
            var ts = go.AddComponent<TankShooting>();

            // Act & Assert
            Assert.IsNull(ts.bulletPrefab, "Bullet prefab should default to null (assigned in Inspector).");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
