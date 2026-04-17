using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.UI.Tests.EditMode
{
    public class ChatTests
    {
        [Test]
        public void EscapeRichText_EscapesAngleBrackets()
        {
            // Arrange
            GameObject go = new GameObject();
            var chat = go.AddComponent<Chat>();
            var method = typeof(Chat).GetMethod("EscapeRichText", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(chat, new object[] { "<b>bold</b>" });

            // Assert
            Assert.AreEqual("&lt;b&gt;bold&lt;/b&gt;", result, "Angle brackets should be escaped to prevent rich text injection.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EscapeRichText_EscapesAmpersand()
        {
            // Arrange
            GameObject go = new GameObject();
            var chat = go.AddComponent<Chat>();
            var method = typeof(Chat).GetMethod("EscapeRichText", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(chat, new object[] { "A & B" });

            // Assert
            Assert.AreEqual("A &amp; B", result);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EscapeRichText_NullInput_ReturnsEmpty()
        {
            // Arrange
            GameObject go = new GameObject();
            var chat = go.AddComponent<Chat>();
            var method = typeof(Chat).GetMethod("EscapeRichText", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(chat, new object[] { null });

            // Assert
            Assert.AreEqual(string.Empty, result);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EscapeRichText_EmptyInput_ReturnsEmpty()
        {
            // Arrange
            GameObject go = new GameObject();
            var chat = go.AddComponent<Chat>();
            var method = typeof(Chat).GetMethod("EscapeRichText", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            string result = (string)method.Invoke(chat, new object[] { "" });

            // Assert
            Assert.AreEqual(string.Empty, result);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsInputFocused_WhenNoInstance_ReturnsFalse()
        {
            // Arrange — ensure static _instance is null
            var instanceField = typeof(Chat).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            var original = instanceField.GetValue(null);
            instanceField.SetValue(null, null);

            // Act & Assert
            Assert.IsFalse(Chat.IsInputFocused);

            // Cleanup
            instanceField.SetValue(null, original);
        }
    }
}
