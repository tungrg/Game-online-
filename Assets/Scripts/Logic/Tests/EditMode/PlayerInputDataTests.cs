using NUnit.Framework;

namespace Game.Logic.Tests.EditMode
{
    public class PlayerInputDataTests
    {
        [Test]
        public void DefaultValues_AreZeroOrFalse()
        {
            // Arrange & Act
            PlayerInputData data = default;

            // Assert
            Assert.AreEqual(0f, data.moveX, 0.001f);
            Assert.AreEqual(0f, data.moveY, 0.001f);
            Assert.AreEqual(0f, data.aimX, 0.001f);
            Assert.AreEqual(0f, data.aimZ, 0.001f);
            Assert.IsFalse(data.hasAim);
            Assert.IsFalse(data.isShooting);
            Assert.IsFalse(data.isShield);
        }

        [Test]
        public void CanSetAndReadMovement()
        {
            // Arrange & Act
            PlayerInputData data = new PlayerInputData
            {
                moveX = 0.5f,
                moveY = -0.3f
            };

            // Assert
            Assert.AreEqual(0.5f, data.moveX, 0.001f);
            Assert.AreEqual(-0.3f, data.moveY, 0.001f);
        }

        [Test]
        public void CanSetAndReadAim()
        {
            // Arrange & Act
            PlayerInputData data = new PlayerInputData
            {
                aimX = 10f,
                aimZ = 20f,
                hasAim = true
            };

            // Assert
            Assert.AreEqual(10f, data.aimX, 0.001f);
            Assert.AreEqual(20f, data.aimZ, 0.001f);
            Assert.IsTrue(data.hasAim);
        }

        [Test]
        public void CanSetAndReadShootingAndShield()
        {
            // Arrange & Act
            PlayerInputData data = new PlayerInputData
            {
                isShooting = true,
                isShield = true
            };

            // Assert
            Assert.IsTrue(data.isShooting);
            Assert.IsTrue(data.isShield);
        }
    }
}
