using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class EffectDestroyTests
    {
        [Test]
        public void SetSpawnScale_ClampsMinimumTo001()
        {
            // Arrange
            GameObject go = new GameObject();
            var effect = go.AddComponent<EffectDestroy>();

            // Act
            effect.SetSpawnScale(0f);

            // Assert
            float spawnScale = (float)typeof(EffectDestroy)
                .GetField("spawnScale", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(effect);
            Assert.AreEqual(0.01f, spawnScale, 0.001f, "Scale should be clamped to minimum 0.01.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetSpawnScale_AcceptsValidScale()
        {
            // Arrange
            GameObject go = new GameObject();
            var effect = go.AddComponent<EffectDestroy>();

            // Act
            effect.SetSpawnScale(2.5f);

            // Assert
            float spawnScale = (float)typeof(EffectDestroy)
                .GetField("spawnScale", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(effect);
            Assert.AreEqual(2.5f, spawnScale, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetSpawnScale_NegativeValue_ClampsTo001()
        {
            // Arrange
            GameObject go = new GameObject();
            var effect = go.AddComponent<EffectDestroy>();

            // Act
            effect.SetSpawnScale(-5f);

            // Assert
            float spawnScale = (float)typeof(EffectDestroy)
                .GetField("spawnScale", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(effect);
            Assert.AreEqual(0.01f, spawnScale, 0.001f, "Negative scale should be clamped to 0.01.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DefaultLifeIs1Point5Seconds()
        {
            // Arrange
            GameObject go = new GameObject();
            var effect = go.AddComponent<EffectDestroy>();

            // Act & Assert
            Assert.AreEqual(1.5f, effect.life, 0.001f, "Default lifetime should be 1.5 seconds.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
