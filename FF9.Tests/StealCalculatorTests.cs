using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Items;
using FluentAssertions;
using Moq;

namespace FF9.Tests;

public class StealCalculatorTests
{
    [Fact]
    public void Steal_Success_IfHitRateIsHigherThanEvade()
    {
        EquipmentItem eqItem = new(ItemName.Potion);
        Unit thief = new UnitBuilder()
            .WithLv(1)
            .WithSpirit(100)
            .Build();
        
        Unit warrior = new UnitBuilder()
            .WithLv(1)
            .WithStealable(new Item?[] { null, null, null, eqItem })
            .WithStealRates(Enumerable.Repeat<int>(byte.MaxValue, 4).ToArray())
            .Build();

        Mock<IRandomProvider> provider = new();
        provider.SetupSequence(p => p.Next16())
            .Returns(1)
            .Returns(1);

        StealCalculator calc = new(provider.Object);
        Item? item = calc.Steal(thief, warrior);
        item.Should().Be(eqItem);
    }
    
    private readonly Mock<IRandomProvider> _mockRandomProvider = new Mock<IRandomProvider>();

    [Fact]
    public void Steal_WhenTargetIsHigherLevel_ReturnsNull()
    {
        // Arrange
        var source = new UnitBuilder().WithLv(5).WithSpirit(10).Build();
        var target = new UnitBuilder()
            .WithLv(10)
            .WithStealRates(new[] { 50, 0, 0, 0 })
            .Build();
        var calculator = new StealCalculator(_mockRandomProvider.Object);
        _mockRandomProvider.Setup(r => r.Next16()).Returns(0); // Source roll is always 0
        _mockRandomProvider.Setup(r => r.Next16()).Returns(1); // Target roll is always 1

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Steal_WhenTargetIsHigherLevelThanSource_ShouldReturnNull()
    {
        // Arrange
        var source = new UnitBuilder().WithLv(1).WithSpirit(5).Build();
        var target = new UnitBuilder().WithLv(2).WithStealRates(new[] { 0, 0, 0, 0 }).Build();
        var calculator = new StealCalculator(_mockRandomProvider.Object);
        _mockRandomProvider.SetupSequence(p => p.Next16())
            .Returns(1) // source roll
            .Returns(0); // target roll

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Steal_WhenSourceRollIsLessThanTargetRoll_ShouldReturnNull()
    {
        // Arrange
        var source = new UnitBuilder().WithLv(1).WithSpirit(5).Build();
        var target = new UnitBuilder().WithLv(1).WithStealRates(new[] { 0, 0, 0, 0 }).Build();
        var calculator = new StealCalculator(_mockRandomProvider.Object);
        _mockRandomProvider.SetupSequence(p => p.Next16())
            .Returns(1) // source roll
            .Returns(2); // target roll

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Steal_WhenTargetHasNoStealableItems_ShouldReturnNull()
    {
        // Arrange
        var source = new UnitBuilder().WithLv(1).WithSpirit(5).Build();
        var target = new UnitBuilder().WithLv(1).WithStealRates(new[] { 0, 0, 0, 0 }).Build();
        var calculator = new StealCalculator(_mockRandomProvider.Object);
        _mockRandomProvider.SetupSequence(p => p.Next16())
            .Returns(2) // source roll
            .Returns(1); // target roll

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void Steal_WhenTargetHasStealableItemsAndSourceRollIsHigherThanTargetRoll_ShouldReturnStolenItem()
    {
        // Arrange
        var useableItem = new UseableItem(ItemName.Potion, 1);
        var targetBuilder = new UnitBuilder().WithLv(10)
            .WithStealable(new Item?[] { useableItem, (Item?)null, (Item?)null, (Item?)null})
            .WithStealRates(new[] {50, 0, 0, 0});
        
        var target = targetBuilder.Build();
        var source = new UnitBuilder().WithLv(10).WithSpirit(10).Build();
        var randomProviderMock = new Mock<IRandomProvider>();
        randomProviderMock.SetupSequence(rp => rp.Next16())
            .Returns(1) // target roll is 0
            .Returns(0); // source roll is 1
        randomProviderMock.Setup(rp => rp.Next8()).Returns(25); // this will trigger target.Steal(0)

        var calculator = new StealCalculator(randomProviderMock.Object);

        // Act
        var stolenItem = calculator.Steal(source, target);

        // Assert
        stolenItem.Should().NotBeNull();
        stolenItem.Should().BeSameAs(useableItem);
        target.Inventory.Should().NotContain(stolenItem!);
    }
    
    [Fact]
    public void Steal_WhenSourceHasVeryHighHitRate_ShouldReturnItemWithHighestStealRate()
    {
        // Arrange
        var target = new UnitBuilder()
            .WithLv(1)
            .WithSpirit(20)
            .WithStealable(new Item[] { new UseableItem(ItemName.Potion, 1), null, new UseableItem(ItemName.Elixir, 1), null })
            .WithStealRates(new[] { 0, 0, 100, 0 })
            .Build();

        var source = new UnitBuilder()
            .WithLv(20)
            .WithSpirit(20)
            .Build();
    
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.SetupSequence(r => r.Next16())
            .Returns(0)
            .Returns(5)
            .Returns(10);
        mockRandom.Setup(r => r.Next8()).Returns(0);

        var calculator = new StealCalculator(mockRandom.Object);

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(ItemName.Elixir);
    }

    [Fact]
    public void Steal_WhenSourceHasVeryLowHitRate_ShouldReturnNull()
    {
        // Arrange
        var source = new UnitBuilder()
            .WithLv(1)
            .WithSpirit(1)
            .Build();

        var target = new UnitBuilder()
            .WithLv(99)
            .WithSpirit(256)
            .WithStealable(new Item?[] { new UseableItem(ItemName.Potion, 1)})
            .WithStealRates(new[] { 0, 0, 0, 0 })
            .Build();

        var calculator = new StealCalculator(new RandomProvider());

        // Act
        var result = calculator.Steal(source, target);

        // Assert
        result.Should().BeNull();
    }

}