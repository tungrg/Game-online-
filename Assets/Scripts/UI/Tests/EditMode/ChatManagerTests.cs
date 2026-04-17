using NUnit.Framework;
using UnityEngine;

namespace Game.UI.Tests.EditMode
{
    public class ChatManagerTests
    {
        [Test]
        public void Awake_SetsSingletonInstance()
        {
            // Arrange
            GameObject go = new GameObject();
            var manager = go.AddComponent<ChatManager>();

            // Act — Awake is called automatically by AddComponent

            // Assert
            Assert.AreEqual(manager, ChatManager.Instance, "Awake should set Instance to this.");

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SendChatMessage_WithNullOrWhitespace_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();
            var manager = go.AddComponent<ChatManager>();

            // Act & Assert
            Assert.DoesNotThrow(() => manager.SendChatMessage(null));
            Assert.DoesNotThrow(() => manager.SendChatMessage(""));
            Assert.DoesNotThrow(() => manager.SendChatMessage("   "));

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
