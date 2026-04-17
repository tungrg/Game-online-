using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.UI.Tests.EditMode
{
    public class LeaderboardTests
    {
        [Test]
        public void IsFallbackName_WithPlayerPrefix_ReturnsTrue()
        {
            // Arrange
            var method = typeof(Leaderboard).GetMethod("IsFallbackName", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            bool result = (bool)method.Invoke(null, new object[] { "Player 1" });

            // Assert
            Assert.IsTrue(result, "'Player 1' is a fallback name.");
        }

        [Test]
        public void IsFallbackName_WithRealName_ReturnsFalse()
        {
            // Arrange
            var method = typeof(Leaderboard).GetMethod("IsFallbackName", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            bool result = (bool)method.Invoke(null, new object[] { "TungRG" });

            // Assert
            Assert.IsFalse(result, "'TungRG' is NOT a fallback name.");
        }

        [Test]
        public void IsFallbackName_WithNull_ReturnsTrue()
        {
            // Arrange
            var method = typeof(Leaderboard).GetMethod("IsFallbackName", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            bool result = (bool)method.Invoke(null, new object[] { null });

            // Assert
            Assert.IsTrue(result, "Null should be treated as a fallback name.");
        }

        [Test]
        public void IsFallbackName_WithWhitespace_ReturnsTrue()
        {
            // Arrange
            var method = typeof(Leaderboard).GetMethod("IsFallbackName", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            bool result = (bool)method.Invoke(null, new object[] { "   " });

            // Assert
            Assert.IsTrue(result, "Whitespace-only should be treated as a fallback name.");
        }

        [Test]
        public void IsFallbackName_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var method = typeof(Leaderboard).GetMethod("IsFallbackName", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            bool result = (bool)method.Invoke(null, new object[] { "player 5" });

            // Assert
            Assert.IsTrue(result, "'player 5' (lowercase) should still be detected as a fallback name.");
        }

        [Test]
        public void GetSnapshot_WhenNoInstance_ReturnsEmptyList()
        {
            // Arrange
            var instanceField = typeof(Leaderboard).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            var original = instanceField.GetValue(null);
            instanceField.SetValue(null, null);

            // Act
            var snapshot = Leaderboard.GetSnapshot();

            // Assert
            Assert.IsNotNull(snapshot);
            Assert.AreEqual(0, snapshot.Count, "Snapshot should be empty when no Leaderboard instance exists.");

            // Cleanup
            instanceField.SetValue(null, original);
        }
    }
}
