using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.UI.Tests.EditMode
{
    public class AfterMatchTests
    {
        [Test]
        public void SanitizeForId_WithNull_ReturnsUnknown()
        {
            // Arrange
            var method = typeof(AfterMatch).GetMethod("SanitizeForId", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            string result = (string)method.Invoke(null, new object[] { null });

            // Assert
            Assert.AreEqual("unknown", result);
        }

        [Test]
        public void SanitizeForId_WithWhitespace_ReturnsUnknown()
        {
            // Arrange
            var method = typeof(AfterMatch).GetMethod("SanitizeForId", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            string result = (string)method.Invoke(null, new object[] { "   " });

            // Assert
            Assert.AreEqual("unknown", result);
        }

        [Test]
        public void SanitizeForId_ReplacesSpecialChars_WithUnderscore()
        {
            // Arrange
            var method = typeof(AfterMatch).GetMethod("SanitizeForId", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            string result = (string)method.Invoke(null, new object[] { "player:123@host" });

            // Assert
            Assert.AreEqual("player_123_host", result);
        }

        [Test]
        public void SanitizeForId_ConvertsToLowerCase()
        {
            // Arrange
            var method = typeof(AfterMatch).GetMethod("SanitizeForId", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            string result = (string)method.Invoke(null, new object[] { "TestPlayer" });

            // Assert
            Assert.AreEqual("testplayer", result);
        }

        [Test]
        public void SanitizeForId_PreservesHyphensAndUnderscores()
        {
            // Arrange
            var method = typeof(AfterMatch).GetMethod("SanitizeForId", BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            string result = (string)method.Invoke(null, new object[] { "test-player_1" });

            // Assert
            Assert.AreEqual("test-player_1", result);
        }

        [Test]
        public void EvaluateMatchEndCondition_WhenAllEnemiesDead_DeclaresWin()
        {
            // Arrange
            GameObject afterMatchObj = new GameObject("AfterMatch");
            var afterMatch = afterMatchObj.AddComponent<AfterMatch>();

            // Setup Player (Alive)
            GameObject playerObj = new GameObject("PlayerTank");
            var playerHealth = playerObj.AddComponent<Health>();
            playerHealth.HP = 100;
            var tankController = playerObj.AddComponent<TankController>();
            var activeTanksList = (System.Collections.Generic.List<TankController>)typeof(TankController)
                .GetField("ActiveTanks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            activeTanksList.Add(tankController);

            // Setup Enemy (Dead)
            GameObject enemyObj = new GameObject("Enemy");
            var enemyShooting = enemyObj.AddComponent<EnemyShooting>();
            var enemyHealth = enemyObj.AddComponent<Health>();
            enemyHealth.HP = 0; // Dead

            // Fast forward time to bypass checkInterval
            typeof(AfterMatch).GetField("_nextCheckAt", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(afterMatch, Time.time - 1f);

            // Act
            afterMatch.SendMessage("Update");

            // Assert
            bool matchEnded = (bool)typeof(AfterMatch).GetField("_matchEnded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(afterMatch);
            bool isWin = (bool)typeof(AfterMatch).GetField("_isWin", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(afterMatch);

            Assert.IsTrue(matchEnded, "Match should end when all enemies are dead.");
            Assert.IsTrue(isWin, "Match should be a win when players are alive and enemies are dead.");

            // Cleanup
            activeTanksList.Clear();
            Object.DestroyImmediate(afterMatchObj);
            Object.DestroyImmediate(playerObj);
            Object.DestroyImmediate(enemyObj);
        }

        [Test]
        public void EvaluateMatchEndCondition_WhenAllPlayersDead_DeclaresLoss()
        {
            // Arrange
            GameObject afterMatchObj = new GameObject("AfterMatch");
            var afterMatch = afterMatchObj.AddComponent<AfterMatch>();

            // Setup Player (Dead)
            GameObject playerObj = new GameObject("PlayerTank");
            var playerHealth = playerObj.AddComponent<Health>();
            playerHealth.HP = 0;
            var tankController = playerObj.AddComponent<TankController>();
            var activeTanksList = (System.Collections.Generic.List<TankController>)typeof(TankController)
                .GetField("ActiveTanks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            activeTanksList.Add(tankController);

            // Setup Enemy (Alive)
            GameObject enemyObj = new GameObject("Enemy");
            var enemyShooting = enemyObj.AddComponent<EnemyShooting>();
            var enemyHealth = enemyObj.AddComponent<Health>();
            enemyHealth.HP = 100;

            // Fast forward time
            typeof(AfterMatch).GetField("_nextCheckAt", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(afterMatch, Time.time - 1f);

            // Act
            afterMatch.SendMessage("Update");

            // Assert
            bool matchEnded = (bool)typeof(AfterMatch).GetField("_matchEnded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(afterMatch);
            bool isWin = (bool)typeof(AfterMatch).GetField("_isWin", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(afterMatch);

            Assert.IsTrue(matchEnded, "Match should end when all players are dead.");
            Assert.IsFalse(isWin, "Match should be a loss when players are dead.");

            // Cleanup
            activeTanksList.Clear();
            Object.DestroyImmediate(afterMatchObj);
            Object.DestroyImmediate(playerObj);
            Object.DestroyImmediate(enemyObj);
        }

        [Test]
        public void EvaluateMatchEndCondition_WhenBothAlive_DoesNotEndMatch()
        {
            // Arrange
            GameObject afterMatchObj = new GameObject("AfterMatch");
            var afterMatch = afterMatchObj.AddComponent<AfterMatch>();

            // Player alive
            GameObject playerObj = new GameObject("PlayerTank");
            playerObj.AddComponent<Health>().HP = 100;
            var tc = playerObj.AddComponent<TankController>();
            var activeTanksList = (System.Collections.Generic.List<TankController>)typeof(TankController)
                .GetField("ActiveTanks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            activeTanksList.Add(tc);

            // Enemy alive
            GameObject enemyObj = new GameObject("Enemy");
            enemyObj.AddComponent<EnemyShooting>();
            enemyObj.AddComponent<Health>().HP = 100;

            typeof(AfterMatch).GetField("_nextCheckAt", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(afterMatch, Time.time - 1f);

            // Act
            afterMatch.SendMessage("Update");

            // Assert
            bool matchEnded = (bool)typeof(AfterMatch).GetField("_matchEnded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(afterMatch);
            Assert.IsFalse(matchEnded, "Match should NOT end when both sides are still alive.");

            // Cleanup
            activeTanksList.Clear();
            Object.DestroyImmediate(afterMatchObj);
            Object.DestroyImmediate(playerObj);
            Object.DestroyImmediate(enemyObj);
        }
    }
}
