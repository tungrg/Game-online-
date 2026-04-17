using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.UI.Tests.EditMode
{
    public class LeaderboardEntryUITests
    {
        [Test]
        public void SetData_SetsRankNameScore_WhenTextFieldsExist()
        {
            // Arrange
            GameObject go = new GameObject("Entry");
            var entryUI = go.AddComponent<LeaderboardEntryUI>();

            // Create child TMP_Text objects with correct names for auto-binding
            GameObject rankGo = new GameObject("Rank");
            rankGo.transform.SetParent(go.transform);
            var rankText = rankGo.AddComponent<TMPro.TextMeshProUGUI>();

            GameObject nameGo = new GameObject("Name");
            nameGo.transform.SetParent(go.transform);
            var nameText = nameGo.AddComponent<TMPro.TextMeshProUGUI>();

            GameObject scoreGo = new GameObject("Score");
            scoreGo.transform.SetParent(go.transform);
            var scoreText = scoreGo.AddComponent<TMPro.TextMeshProUGUI>();

            // Act
            entryUI.SetData(1, "TestPlayer", 250);

            // Assert
            Assert.AreEqual("1", rankText.text);
            Assert.AreEqual("TestPlayer", nameText.text);
            Assert.AreEqual("250", scoreText.text);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetData_DoesNotThrow_WhenNoTextChildren()
        {
            // Arrange
            GameObject go = new GameObject("EmptyEntry");
            var entryUI = go.AddComponent<LeaderboardEntryUI>();

            // Act & Assert
            Assert.DoesNotThrow(() => entryUI.SetData(1, "Test", 100),
                "SetData should not throw even when no TMP_Text children exist.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
