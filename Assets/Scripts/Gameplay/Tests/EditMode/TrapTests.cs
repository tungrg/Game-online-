using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class TrapTests
    {
        [Test]
        public void ApplyColliderMode_ShootOnlyBarrel_SetsTriggerFalse()
        {
            // Arrange
            GameObject go = new GameObject();
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true; // Start as trigger
            var trap = go.AddComponent<Trap>();

            // Set trapType to ShootOnlyBarrel and autoConfigureCollider to true
            typeof(Trap).GetField("trapType", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, Trap.TrapType.ShootOnlyBarrel);
            typeof(Trap).GetField("autoConfigureCollider", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, true);

            // Act
            typeof(Trap).GetMethod("ApplyColliderMode", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trap, null);

            // Assert
            Assert.IsFalse(col.isTrigger, "ShootOnlyBarrel collider should NOT be a trigger.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyColliderMode_TouchOrShootMine_SetsTriggerTrue()
        {
            // Arrange
            GameObject go = new GameObject();
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = false; // Start as non-trigger
            var trap = go.AddComponent<Trap>();

            typeof(Trap).GetField("trapType", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, Trap.TrapType.TouchOrShootMine);
            typeof(Trap).GetField("autoConfigureCollider", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, true);

            // Act
            typeof(Trap).GetMethod("ApplyColliderMode", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trap, null);

            // Assert
            Assert.IsTrue(col.isTrigger, "TouchOrShootMine collider SHOULD be a trigger.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnsurePhysicsSetup_TouchOrShootMine_AddsKinematicRigidbody()
        {
            // Arrange
            GameObject go = new GameObject();
            var trap = go.AddComponent<Trap>();

            typeof(Trap).GetField("trapType", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, Trap.TrapType.TouchOrShootMine);

            // Act
            typeof(Trap).GetMethod("EnsurePhysicsSetupRuntime", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trap, null);

            // Assert
            Rigidbody rb = go.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb, "TouchOrShootMine should auto-create a Rigidbody.");
            Assert.IsTrue(rb.isKinematic, "Rigidbody should be kinematic.");
            Assert.IsFalse(rb.useGravity, "Rigidbody should not use gravity.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnsurePhysicsSetup_ShootOnlyBarrel_WithExistingRb_FreezesAll()
        {
            // Arrange
            GameObject go = new GameObject();
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            var trap = go.AddComponent<Trap>();

            typeof(Trap).GetField("trapType", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, Trap.TrapType.ShootOnlyBarrel);

            // Act
            typeof(Trap).GetMethod("EnsurePhysicsSetupRuntime", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(trap, null);

            // Assert
            Assert.IsTrue(rb.isKinematic, "ShootOnlyBarrel Rigidbody should be kinematic.");
            Assert.AreEqual(RigidbodyConstraints.FreezeAll, rb.constraints);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryTriggerExplosionByShot_WhenAlreadyExploded_ReturnsFalse()
        {
            // Arrange
            GameObject go = new GameObject();
            var trap = go.AddComponent<Trap>();

            typeof(Trap).GetField("_exploded", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(trap, true);

            // Act
            bool result = trap.TryTriggerExplosionByShot();

            // Assert
            Assert.IsFalse(result, "Should return false if the trap already exploded.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultExplosionDamage_Is35()
        {
            // Arrange
            GameObject go = new GameObject();
            var trap = go.AddComponent<Trap>();

            // Act
            int dmg = (int)typeof(Trap).GetField("explosionDamage", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(trap);

            // Assert
            Assert.AreEqual(35, dmg);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultExplosionRadius_Is4()
        {
            // Arrange
            GameObject go = new GameObject();
            var trap = go.AddComponent<Trap>();

            // Act
            float radius = (float)typeof(Trap).GetField("explosionRadius", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(trap);

            // Assert
            Assert.AreEqual(4f, radius, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
