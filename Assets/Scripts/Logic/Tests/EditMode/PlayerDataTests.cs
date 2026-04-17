using NUnit.Framework;

namespace Game.Logic.Tests.EditMode
{
    public class PlayerDataTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerData.PlayerName = null;
        }

        [Test]
        public void PlayerName_DefaultIsNull()
        {
            // Arrange & Act & Assert
            Assert.IsNull(PlayerData.PlayerName, "PlayerName should default to null.");
        }

        [Test]
        public void PlayerName_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            PlayerData.PlayerName = "TestPlayer";

            // Assert
            Assert.AreEqual("TestPlayer", PlayerData.PlayerName);
        }

        [Test]
        public void PlayerName_CanBeSetToEmpty()
        {
            // Arrange & Act
            PlayerData.PlayerName = "";

            // Assert
            Assert.AreEqual("", PlayerData.PlayerName);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerData.PlayerName = null;
        }
    }
}
