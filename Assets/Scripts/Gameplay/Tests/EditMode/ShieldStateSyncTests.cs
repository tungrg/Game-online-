using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Game.Gameplay.Tests.EditMode
{
    public class ShieldStateSyncTests
    {
        [Test]
        public void ShieldActive_DefaultIsFalse()
        {
            // Arrange
            GameObject go = new GameObject();
            var sync = go.AddComponent<ShieldStateSync>();

            // Act & Assert
            // NetworkBool defaults to false
            Assert.IsFalse(sync.ShieldActive);

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
