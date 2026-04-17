using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class BulletTests
    {
        [Test]
        public void SetInitialState_SetsTeamAndSpeed()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            // Act
            bullet.SetInitialState(2, 15f, null, null, Vector3.zero, Quaternion.identity);

            // Assert
            Assert.AreEqual(2, bullet.Team);
            float speed = (float)typeof(Bullet).GetField("speed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bullet);
            Assert.AreEqual(15f, speed, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetInitialState_NegativeSpeed_ClampedToZero()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            // Act
            bullet.SetInitialState(0, -10f, null, null, Vector3.zero, Quaternion.identity);

            // Assert
            float speed = (float)typeof(Bullet).GetField("speed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bullet);
            Assert.AreEqual(0f, speed, 0.001f, "Negative speed should be clamped to 0.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetInitialState_SetsCorrectMoveDirection()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            Quaternion rotation = Quaternion.LookRotation(Vector3.right); // Facing +X

            // Act
            bullet.SetInitialState(1, 10f, null, null, Vector3.zero, rotation);

            // Assert
            Vector3 moveDir = (Vector3)typeof(Bullet).GetField("_moveDirection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bullet);
            Assert.AreEqual(1f, moveDir.x, 0.01f, "Move direction X should be ~1 when facing right.");
            Assert.AreEqual(0f, moveDir.y, 0.01f);
            Assert.AreEqual(0f, moveDir.z, 0.01f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetInitialState_MarksAsInitialized()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            // Act
            bullet.SetInitialState(1, 10f, null);

            // Assert
            bool init = (bool)typeof(Bullet).GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bullet);
            Assert.IsTrue(init, "Bullet should be marked as initialized after SetInitialState.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultDamage_Is10()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            // Act & Assert
            Assert.AreEqual(10, bullet.damage);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void FindHealth_ReturnsNull_WhenNoHealthComponent()
        {
            // Arrange
            GameObject go = new GameObject();
            var bullet = go.AddComponent<Bullet>();

            GameObject targetGo = new GameObject();
            var targetCol = targetGo.AddComponent<BoxCollider>();

            // Act
            var findHealthMethod = typeof(Bullet).GetMethod("FindHealth", BindingFlags.NonPublic | BindingFlags.Instance);
            Health result = (Health)findHealthMethod.Invoke(bullet, new object[] { targetCol });

            // Assert
            Assert.IsNull(result, "Should return null when collider has no Health component.");

            // Cleanup
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void FindHealth_ReturnsHealth_WhenParentHasHealth()
        {
            // Arrange
            GameObject bulletGo = new GameObject();
            var bullet = bulletGo.AddComponent<Bullet>();

            GameObject parentGo = new GameObject("Parent");
            var health = parentGo.AddComponent<Health>();
            health.HP = 50;

            GameObject childGo = new GameObject("Child");
            childGo.transform.SetParent(parentGo.transform);
            var childCol = childGo.AddComponent<BoxCollider>();

            // Act
            var findHealthMethod = typeof(Bullet).GetMethod("FindHealth", BindingFlags.NonPublic | BindingFlags.Instance);
            Health result = (Health)findHealthMethod.Invoke(bullet, new object[] { childCol });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(50, result.HP);

            // Cleanup
            Object.DestroyImmediate(parentGo);
            Object.DestroyImmediate(bulletGo);
        }
    }
}
