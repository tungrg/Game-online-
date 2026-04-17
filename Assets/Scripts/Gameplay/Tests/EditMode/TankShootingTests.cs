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

            // Act
            // bulletSpeed is [System.NonSerialized] public, default = 20f
            float speed = (float)typeof(TankShooting)
                .GetField("bulletSpeed", BindingFlags.Public | BindingFlags.Instance)
                .GetValue(ts);

            // Assert
            Assert.AreEqual(20f, speed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Shoot_WithNullBulletPrefab_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();
            var ts = go.AddComponent<TankShooting>();
            ts.bulletPrefab = null;

            // Act & Assert — calling Shoot via reflection with null prefab should early-return
            var shootMethod = typeof(TankShooting).GetMethod("Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.DoesNotThrow(() =>
            {
                shootMethod.Invoke(ts, new object[] { default(PlayerInputData) });
            }, "Shoot should early-return when bulletPrefab is null.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
