using NUnit.Framework;
using UnityEngine;

namespace Game.Legacy.Tests.EditMode
{
    public class PlayerSpawnerTests
    {
        [Test]
        public void GetSpawnPosition_AppliesYOffset()
        {
            // Arrange
            GameObject go = new GameObject();
            go.transform.position = new Vector3(5f, 0f, 10f);
            var spawner = go.AddComponent<PlayerSpawner>();

            // The default spawnYOffset is 1.0f (SerializeField)
            var method = typeof(PlayerSpawner).GetMethod("GetSpawnPosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Vector3 result = (Vector3)method.Invoke(spawner, null);

            // Assert
            Assert.AreEqual(5f, result.x, 0.001f);
            Assert.AreEqual(1f, result.y, 0.001f, "Y should equal transform Y + spawnYOffset (0 + 1).");
            Assert.AreEqual(10f, result.z, 0.001f);

            // Cleanup
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetSpawnPosition_CustomYOffset()
        {
            // Arrange
            GameObject go = new GameObject();
            go.transform.position = Vector3.zero;
            var spawner = go.AddComponent<PlayerSpawner>();

            typeof(PlayerSpawner).GetField("spawnYOffset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(spawner, 3.5f);

            var method = typeof(PlayerSpawner).GetMethod("GetSpawnPosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            Vector3 result = (Vector3)method.Invoke(spawner, null);

            // Assert
            Assert.AreEqual(3.5f, result.y, 0.001f, "Y should reflect the custom spawnYOffset.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
