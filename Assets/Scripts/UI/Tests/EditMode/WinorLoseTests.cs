using NUnit.Framework;
using UnityEngine;

namespace Game.UI.Tests.EditMode
{
    public class WinorLoseTests
    {
        [Test]
        public void ShowResult_ActivatesTextAndSetsMessage()
        {
            // Arrange
            GameObject go = new GameObject();
            var winOrLose = go.AddComponent<WinorLose>();
            
            // Add TMP_Text using reflection to bypass dependency in test
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform);
            var textComponent = textGo.AddComponent<TMPro.TextMeshProUGUI>();
            textGo.SetActive(false);
            
            typeof(WinorLose).GetField("resultText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(winOrLose, textComponent);

            // Act
            typeof(WinorLose).GetMethod("ShowResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(winOrLose, new object[] { "VICTORY!" });

            // Assert
            Assert.IsTrue(textGo.activeSelf, "Text object should be activated.");
            Assert.AreEqual("VICTORY!", textComponent.text, "Text message should match the input.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
