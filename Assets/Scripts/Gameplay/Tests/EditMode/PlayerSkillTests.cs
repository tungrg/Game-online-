using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class PlayerSkillTests
    {
        [Test]
        public void TryBlockSegment_WhenShieldInactive_ReturnsFalse()
        {
            // Arrange
            GameObject go = new GameObject("PlayerSkillTest");
            PlayerSkill skill = go.AddComponent<PlayerSkill>();
            
            // Shield is inactive by default (_shieldActiveLocal = false)

            // Act
            bool blocked = skill.TryBlockSegment(1, null, Vector3.zero, Vector3.forward * 10f, out Vector3 blockPoint);

            // Assert
            Assert.IsFalse(blocked, "Should not block if the shield is inactive.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryBlockSegment_WhenSegmentIntersectsShield_ReturnsTrueAndCorrectPoint()
        {
            // Arrange
            GameObject go = new GameObject("PlayerSkillTest");
            go.transform.position = new Vector3(0, 0, 5); // Shield at Z=5
            PlayerSkill skill = go.AddComponent<PlayerSkill>();

            // Use reflection to activate shield and set radius since they are private fields
            FieldInfo activeField = typeof(PlayerSkill).GetField("_shieldActiveLocal", BindingFlags.NonPublic | BindingFlags.Instance);
            activeField.SetValue(skill, true);

            FieldInfo radiusField = typeof(PlayerSkill).GetField("shieldRadius", BindingFlags.NonPublic | BindingFlags.Instance);
            radiusField.SetValue(skill, 2.0f); // Radius 2

            // Act: Shoot a bullet ray from origin along Z axis passing through the shield
            bool blocked = skill.TryBlockSegment(1, null, Vector3.zero, new Vector3(0, 0, 10), out Vector3 blockPoint);

            // Assert
            Assert.IsTrue(blocked, "Should block the segment since it passes directly through the shield's radius.");
            // The closest point on the line segment (0,0,0 to 0,0,10) to the center (0,0,5) is (0,0,5).
            Assert.AreEqual(new Vector3(0, 0, 5), blockPoint);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TryBlockSegment_WhenSegmentMissesShield_ReturnsFalse()
        {
            // Arrange
            GameObject go = new GameObject("PlayerSkillTest");
            go.transform.position = new Vector3(0, 5, 5); // Shield at Y=5, Z=5
            PlayerSkill skill = go.AddComponent<PlayerSkill>();

            FieldInfo activeField = typeof(PlayerSkill).GetField("_shieldActiveLocal", BindingFlags.NonPublic | BindingFlags.Instance);
            activeField.SetValue(skill, true);

            FieldInfo radiusField = typeof(PlayerSkill).GetField("shieldRadius", BindingFlags.NonPublic | BindingFlags.Instance);
            radiusField.SetValue(skill, 2.0f); // Radius is 2. Shield reaches down to Y=3.

            // Act: Shoot a ray from origin along Z axis (Y=0)
            bool blocked = skill.TryBlockSegment(1, null, Vector3.zero, new Vector3(0, 0, 10), out Vector3 blockPoint);

            // Assert: The segment is at Y=0, shield bottom is at Y=3. Distance is > radius.
            Assert.IsFalse(blocked, "Should not block if the bullet passes completely outside the shield radius.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
