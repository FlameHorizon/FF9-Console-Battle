using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Items;
using Moq;

namespace FF9.Tests
{
    public class ItemScriptsTests
    {
        [Fact]
        public void HiPotion_Should_Heal_Target_Without_Chemist()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsDead).Returns(false);
            
            var mockSource = new Mock<IUnit>();
            mockSource.Setup(x => x.HasSupportAbility(SupportAbility.Chemist)).Returns(false);

            var ctx = new BattleContext { Source = mockSource.Object, Target = mockTarget.Object };

            var hipotion = new ItemScripts.HiPotion();

            // Act
            hipotion.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.TakeHeal(300), Times.Once());
        }
        
        [Fact]
        public void HiPotion_Should_Heal_Target_With_Chemist()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsDead).Returns(false);
            
            var mockSource = new Mock<IUnit>();
            mockSource.Setup(x => x.HasSupportAbility(SupportAbility.Chemist)).Returns(true);

            var ctx = new BattleContext { Source = mockSource.Object, Target = mockTarget.Object, InCombat = false};

            var hipotion = new ItemScripts.HiPotion();

            // Act
            hipotion.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.TakeHeal(600), Times.Once());
        }

        [Fact]
        public void Potion_Should_Heal_Target_WithoutChemist()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsDead).Returns(false);
            
            var mockSource = new Mock<IUnit>();
            mockSource.Setup(x => x.HasSupportAbility(SupportAbility.Chemist)).Returns(false);

            var ctx = new BattleContext { Source = mockSource.Object, Target = mockTarget.Object };

            var potion = new ItemScripts.Potion();

            // Act
            potion.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.TakeHeal(100), Times.Once());
        }

        [Fact]
        public void PhoenixDown_Should_Revive_Target()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.Hp).Returns(0);

            var ctx = new BattleContext { Source = null, Target = mockTarget.Object };

            var phoenixDown = new ItemScripts.PhoenixDown();

            // Act
            phoenixDown.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.Revive(), Times.Once());
        }

        [Fact]
        public void PhoenixPinion_Should_Revive_Player_Target()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsPlayer).Returns(true);

            var ctx = new BattleContext { Source = null, Target = mockTarget.Object };

            var phoenixPinion = new ItemScripts.PhoenixPinion();

            // Act
            phoenixPinion.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.Revive(), Times.Once());
        }

        [Fact]
        public void PhoenixPinion_Should_Kill_Undead_Enemy_Target()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsPlayer).Returns(false);
            mockTarget.Setup(x => x.IsEnemy).Returns(true);
            mockTarget.Setup(x => x.IsType(UnitType.Undead)).Returns(true);
            
            var randomProvider = new Mock<IRandomProvider>();
            randomProvider.Setup(x => x.Next(1, 11)).Returns(10);
            
            var ctx = new BattleContext { Source = null!, Target = mockTarget.Object };

            var phoenixPinion = new ItemScripts.PhoenixPinion(randomProvider.Object);

            // Act
            phoenixPinion.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.InstantDeath(), Times.Once());
        }

        [Fact]
        public void PhoenixPinion_Should_NotHeal_Living_Enemy_Target()
        {
            // Arrange
            var mockTarget = new Mock<IUnit>();
            mockTarget.Setup(x => x.IsPlayer).Returns(false);
            mockTarget.Setup(x => x.Hp).Returns(50);
            
            var mockSource = new Mock<IUnit>();
            mockSource.Setup(x => x.IsPlayer).Returns(true);
            
            var item = new ItemScripts.PhoenixPinion();
            var ctx = new BattleContext { Source = mockSource.Object, Target = mockTarget.Object };

            // Act
            item.Use(ctx);

            // Assert
            mockTarget.Verify(x => x.TakeHeal(It.IsAny<int>()), Times.Never());
        }
    }
}