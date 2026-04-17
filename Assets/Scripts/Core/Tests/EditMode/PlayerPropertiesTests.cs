using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Core.Tests.EditMode
{
    public class PlayerPropertiesTests
    {
        [Test]
        public void SanitizeName_TruncatesLongNames()
        {
            // Arrange
            GameObject go = new GameObject();
            var props = go.AddComponent<PlayerProperties>();
            var method = typeof(PlayerProperties).GetMethod("SanitizeName", BindingFlags.NonPublic | BindingFlags.Instance);

            string longName = new string('A', 50); // 50 chars, limit is 32

            // Act
            string result = (string)method.Invoke(props, new object[] { longName });

            // Assert
            Assert.AreEqual(32, result.Length, "Names longer than 32 characters should be truncated.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SanitizeName_TrimsWhitespace()
        {
            // Arrange
            GameObject go = new GameObject();
            var props = go.AddComponent<PlayerProperties>();
            var method = typeof(PlayerProperties).GetMethod("SanitizeName", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(props, new object[] { "  TestPlayer  " });

            // Assert
            Assert.AreEqual("TestPlayer", result);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SanitizeName_WhitespaceOnly_ReturnsFallback()
        {
            // Arrange
            GameObject go = new GameObject();
            var props = go.AddComponent<PlayerProperties>();
            var method = typeof(PlayerProperties).GetMethod("SanitizeName", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(props, new object[] { "   " });

            // Assert
            // When whitespace-only, it calls BuildDefaultName which returns "Player {id}"
            Assert.IsTrue(result.StartsWith("Player"), "Whitespace-only should fall back to default name.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SanitizeName_NormalName_ReturnsAsIs()
        {
            // Arrange
            GameObject go = new GameObject();
            var props = go.AddComponent<PlayerProperties>();
            var method = typeof(PlayerProperties).GetMethod("SanitizeName", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(props, new object[] { "TungRG" });

            // Assert
            Assert.AreEqual("TungRG", result);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DisplayName_WhenNoDisplayNameSet_ReturnsFallback()
        {
            // Arrange
            GameObject go = new GameObject();
            var props = go.AddComponent<PlayerProperties>();

            // Act
            string name = props.DisplayName;

            // Assert
            Assert.IsTrue(name.StartsWith("Player"), "DisplayName should return a fallback 'Player {id}' when no name is set.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BindReferences_FindsHealthOnSameObject()
        {
            // Arrange
            GameObject go = new GameObject();
            go.AddComponent<Health>();
            var props = go.AddComponent<PlayerProperties>();

            // Act
            typeof(PlayerProperties).GetMethod("BindReferences", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(props, null);

            // Assert
            Health cachedHealth = (Health)typeof(PlayerProperties)
                .GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(props);
            Assert.IsNotNull(cachedHealth, "BindReferences should find Health on the same GameObject.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BindReferences_FindsHealthOnParent()
        {
            // Arrange
            GameObject parent = new GameObject("Parent");
            parent.AddComponent<Health>();

            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            var props = child.AddComponent<PlayerProperties>();

            // Act
            typeof(PlayerProperties).GetMethod("BindReferences", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(props, null);

            // Assert
            Health cachedHealth = (Health)typeof(PlayerProperties)
                .GetField("_health", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(props);
            Assert.IsNotNull(cachedHealth, "BindReferences should find Health on parent.");

            // Cleanup
            Object.DestroyImmediate(parent);
        }
    }
}
