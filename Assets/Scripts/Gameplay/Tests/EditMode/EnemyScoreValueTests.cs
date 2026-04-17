using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.Tests.EditMode
{
    public class EnemyScoreValueTests
    {
        [Test]
        public void KillScore_WhenPositive_ReturnsValue()
        {
            // Arrange
            GameObject go = new GameObject();
            var sv = go.AddComponent<EnemyScoreValue>();
            sv.killScore = 25;

            // Act & Assert
            Assert.AreEqual(25, sv.KillScore);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void KillScore_WhenNegative_ReturnsClamped()
        {
            // Arrange
            GameObject go = new GameObject();
            var sv = go.AddComponent<EnemyScoreValue>();
            sv.killScore = -10;

            // Act & Assert
            Assert.AreEqual(0, sv.KillScore, "Negative killScore should be clamped to 0 by Mathf.Max.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void KillScore_WhenZero_ReturnsZero()
        {
            // Arrange
            GameObject go = new GameObject();
            var sv = go.AddComponent<EnemyScoreValue>();
            sv.killScore = 0;

            // Act & Assert
            Assert.AreEqual(0, sv.KillScore);

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
