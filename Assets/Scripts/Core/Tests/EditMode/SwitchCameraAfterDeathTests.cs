using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Game.Core.Tests.EditMode
{
    public class SwitchCameraAfterDeathTests
    {
        private List<TankController> _activeTanksList;

        [SetUp]
        public void SetUp()
        {
            _activeTanksList = (List<TankController>)typeof(TankController)
                .GetField("ActiveTanks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            _activeTanksList.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _activeTanksList.Clear();
        }

        [Test]
        public void RebuildCandidateList_ExcludesDeadTanks()
        {
            // Arrange
            GameObject go = new GameObject();
            var camScript = go.AddComponent<SwitchCameraAfterDeath>();

            GameObject tankGo = new GameObject("Tank");
            var health = tankGo.AddComponent<Health>();
            health.HP = 0; // Dead
            var tankController = tankGo.AddComponent<TankController>();
            _activeTanksList.Add(tankController);

            // Act
            typeof(SwitchCameraAfterDeath).GetMethod("RebuildCandidateList", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(camScript, null);

            // Assert
            var candidates = (List<TankController>)typeof(SwitchCameraAfterDeath)
                .GetField("_candidates", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(camScript);

            Assert.AreEqual(0, candidates.Count, "Dead tanks should not be added to the spectator candidate list.");

            // Cleanup
            Object.DestroyImmediate(tankGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StepTarget_Forward_WrapsAround()
        {
            // Arrange
            GameObject go = new GameObject();
            var camScript = go.AddComponent<SwitchCameraAfterDeath>();

            var candidates = (List<TankController>)typeof(SwitchCameraAfterDeath)
                .GetField("_candidates", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(camScript);

            // Create 3 mock tanks
            GameObject t1Go = new GameObject("Tank1");
            var tc1 = t1Go.AddComponent<TankController>();
            GameObject t2Go = new GameObject("Tank2");
            var tc2 = t2Go.AddComponent<TankController>();
            GameObject t3Go = new GameObject("Tank3");
            var tc3 = t3Go.AddComponent<TankController>();

            candidates.Add(tc1);
            candidates.Add(tc2);
            candidates.Add(tc3);

            // Set _currentIndex to the last item (index 2)
            typeof(SwitchCameraAfterDeath)
                .GetField("_currentIndex", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(camScript, 2);

            // Act — step forward, should wrap to 0
            typeof(SwitchCameraAfterDeath)
                .GetMethod("StepTarget", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(camScript, new object[] { 1 });

            // Assert
            int newIndex = (int)typeof(SwitchCameraAfterDeath)
                .GetField("_currentIndex", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(camScript);
            Assert.AreEqual(0, newIndex, "Stepping forward from the last index should wrap to 0.");

            // Cleanup
            Object.DestroyImmediate(t1Go);
            Object.DestroyImmediate(t2Go);
            Object.DestroyImmediate(t3Go);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StepTarget_Backward_WrapsAround()
        {
            // Arrange
            GameObject go = new GameObject();
            var camScript = go.AddComponent<SwitchCameraAfterDeath>();

            var candidates = (List<TankController>)typeof(SwitchCameraAfterDeath)
                .GetField("_candidates", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(camScript);

            GameObject t1Go = new GameObject("Tank1");
            var tc1 = t1Go.AddComponent<TankController>();
            GameObject t2Go = new GameObject("Tank2");
            var tc2 = t2Go.AddComponent<TankController>();
            candidates.Add(tc1);
            candidates.Add(tc2);

            // Set _currentIndex to 0
            typeof(SwitchCameraAfterDeath)
                .GetField("_currentIndex", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(camScript, 0);

            // Act — step backward from 0, should wrap to count-1
            typeof(SwitchCameraAfterDeath)
                .GetMethod("StepTarget", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(camScript, new object[] { -1 });

            // Assert
            int newIndex = (int)typeof(SwitchCameraAfterDeath)
                .GetField("_currentIndex", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(camScript);
            Assert.AreEqual(1, newIndex, "Stepping backward from index 0 should wrap to the last index.");

            // Cleanup
            Object.DestroyImmediate(t1Go);
            Object.DestroyImmediate(t2Go);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StepTarget_EmptyCandidates_DoesNotThrow()
        {
            // Arrange
            GameObject go = new GameObject();
            var camScript = go.AddComponent<SwitchCameraAfterDeath>();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                typeof(SwitchCameraAfterDeath)
                    .GetMethod("StepTarget", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(camScript, new object[] { 1 });
            }, "StepTarget should not throw when candidate list is empty.");

            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}
