using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Gameplay.Tests.PlayMode
{
    public class BulletPhysicsTests
    {
        [UnityTest]
        public IEnumerator Bullet_HasIsKinematicRigidbody_ForPhysicsNonAlloc()
        {
            // Arrange
            GameObject bulletObject = new GameObject("TestBullet");
            Rigidbody rb = bulletObject.AddComponent<Rigidbody>();
            SphereCollider col = bulletObject.AddComponent<SphereCollider>();
            Bullet bullet = bulletObject.AddComponent<Bullet>();

            // Act
            // Spawned is normally called by Fusion, but we manually trigger it to test logic
            bullet.Spawned();
            
            yield return null;

            // Assert
            Assert.IsTrue(rb.isKinematic, "Bullet Rigidbody should be kinematic according to best practices.");
            Assert.IsFalse(rb.useGravity, "Bullet should not use gravity by default.");
            Assert.IsTrue(col.isTrigger, "Bullet Collider should be a trigger.");

            // Cleanup
            Object.Destroy(bulletObject);
        }
    }
}
