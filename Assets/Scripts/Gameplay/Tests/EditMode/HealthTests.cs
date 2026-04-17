using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class HealthTests
    {
        [Test]
        public void GetMaxHP_ReturnsCorrectValue()
        {
            // Arrange
            GameObject go = new GameObject("HealthObject");
            Health health = go.AddComponent<Health>();
            
            // Set private field startHP using reflection
            FieldInfo startHpField = typeof(Health).GetField("startHP", BindingFlags.NonPublic | BindingFlags.Instance);
            startHpField.SetValue(health, 150);

            // Act
            int maxHp = health.GetMaxHP();

            // Assert
            Assert.AreEqual(150, maxHp);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetMaxHP_WhenStartHPIsZero_ReturnsAtLeastOne()
        {
            // Arrange
            GameObject go = new GameObject("HealthObject");
            Health health = go.AddComponent<Health>();
            
            FieldInfo startHpField = typeof(Health).GetField("startHP", BindingFlags.NonPublic | BindingFlags.Instance);
            startHpField.SetValue(health, 0); // Invalid start HP

            // Act
            int maxHp = health.GetMaxHP();

            // Assert
            Assert.AreEqual(1, maxHp, "Max HP should always be at least 1");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
