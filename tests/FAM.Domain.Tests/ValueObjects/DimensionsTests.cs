using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class DimensionsTests
{
    [Fact]
    public void Create_WithValidDimensions_ShouldCreateDimensions()
    {
        // Arrange
        var length = 100.5m;
        var width = 50.25m;
        var height = 30.75m;
        var unit = "cm";

        // Act
        var dimensions = Dimensions.Create(length, width, height, unit);

        // Assert
        dimensions.Should().NotBeNull();
        dimensions.Length.Should().Be(length);
        dimensions.Width.Should().Be(width);
        dimensions.Height.Should().Be(height);
        dimensions.Unit.Should().Be(unit);
    }

    [Fact]
    public void Create_WithDefaultUnit_ShouldCreateDimensionsWithCm()
    {
        // Arrange
        var length = 100m;
        var width = 50m;
        var height = 30m;

        // Act
        var dimensions = Dimensions.Create(length, width, height);

        // Assert
        dimensions.Unit.Should().Be("cm");
    }

    [Fact]
    public void Create_WithZeroLength_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Dimensions.Create(0, 50, 30));
    }

    [Fact]
    public void Create_WithNegativeLength_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Dimensions.Create(-10, 50, 30));
    }

    [Fact]
    public void Create_WithZeroWidth_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Dimensions.Create(100, 0, 30));
    }

    [Fact]
    public void Create_WithZeroHeight_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Dimensions.Create(100, 50, 0));
    }

    [Fact]
    public void Create_WithInvalidUnit_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Dimensions.Create(100, 50, 30, "invalid"));
    }

    [Fact]
    public void Create_WithValidUnits_ShouldCreateDimensions()
    {
        // Arrange
        var validUnits = new[] { "cm", "m", "mm", "in", "ft" };

        // Act & Assert
        foreach (var unit in validUnits)
        {
            var dimensions = Dimensions.Create(100, 50, 30, unit);
            dimensions.Unit.Should().Be(unit.ToLower());
        }
    }

    [Fact]
    public void Create_WithUppercaseUnit_ShouldConvertToLowercase()
    {
        // Arrange
        var unit = "CM";

        // Act
        var dimensions = Dimensions.Create(100, 50, 30, unit);

        // Assert
        dimensions.Unit.Should().Be("cm");
    }

    [Fact]
    public void Parse_WithValidString_ShouldParseDimensions()
    {
        // Arrange
        var dimensionsString = "100 x 50 x 30 cm";

        // Act
        var dimensions = Dimensions.Parse(dimensionsString);

        // Assert
        dimensions.Should().NotBeNull();
        dimensions!.Length.Should().Be(100);
        dimensions.Width.Should().Be(50);
        dimensions.Height.Should().Be(30);
        dimensions.Unit.Should().Be("cm");
    }

    [Fact]
    public void Parse_WithStringWithoutUnit_ShouldUseDefaultUnit()
    {
        // Arrange
        var dimensionsString = "100 x 50 x 30";

        // Act
        var dimensions = Dimensions.Parse(dimensionsString);

        // Assert
        dimensions.Should().NotBeNull();
        dimensions!.Unit.Should().Be("cm");
    }

    [Fact]
    public void Parse_WithDifferentSeparators_ShouldParseCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            "100 x 50 x 30",
            "100 X 50 X 30",
            "100*50*30"
        };

        // Act & Assert
        foreach (var dimensionsString in testCases)
        {
            var dimensions = Dimensions.Parse(dimensionsString);
            dimensions.Should().NotBeNull();
            dimensions!.Length.Should().Be(100);
            dimensions.Width.Should().Be(50);
            dimensions.Height.Should().Be(30);
        }
    }

    [Fact]
    public void Parse_WithInvalidString_ShouldReturnNull()
    {
        // Arrange
        var invalidStrings = new[]
        {
            "",
            "   ",
            "100 x 50",
            "invalid x 50 x 30",
            "100 x invalid x 30",
            "100 x 50 x invalid"
        };

        // Act & Assert
        foreach (var dimensionsString in invalidStrings)
        {
            var dimensions = Dimensions.Parse(dimensionsString);
            dimensions.Should().BeNull();
        }
    }

    [Fact]
    public void Parse_WithNegativeValues_ShouldReturnNull()
    {
        // Arrange
        var dimensionsString = "100 x -50 x 30";

        // Act
        var dimensions = Dimensions.Parse(dimensionsString);

        // Assert
        dimensions.Should().BeNull();
    }

    [Fact]
    public void Volume_ShouldCalculateCorrectly()
    {
        // Arrange
        var dimensions = Dimensions.Create(10, 5, 2);

        // Act
        var volume = dimensions.Volume();

        // Assert
        volume.Should().Be(100); // 10 * 5 * 2
    }

    [Fact]
    public void ConvertTo_WithSameUnit_ShouldReturnSameDimensions()
    {
        // Arrange
        var dimensions = Dimensions.Create(100, 50, 30, "cm");

        // Act
        Dimensions converted = dimensions.ConvertTo("cm");

        // Assert
        converted.Length.Should().Be(100);
        converted.Width.Should().Be(50);
        converted.Height.Should().Be(30);
        converted.Unit.Should().Be("cm");
    }

    [Fact]
    public void ConvertTo_FromCmToM_ShouldConvertCorrectly()
    {
        // Arrange
        var dimensions = Dimensions.Create(100, 50, 30, "cm");

        // Act
        Dimensions converted = dimensions.ConvertTo("m");

        // Assert
        converted.Length.Should().Be(1);
        converted.Width.Should().Be(0.5m);
        converted.Height.Should().Be(0.3m);
        converted.Unit.Should().Be("m");
    }

    [Fact]
    public void ConvertTo_FromMToCm_ShouldConvertCorrectly()
    {
        // Arrange
        var dimensions = Dimensions.Create(1, 0.5m, 0.3m, "m");

        // Act
        Dimensions converted = dimensions.ConvertTo("cm");

        // Assert
        converted.Length.Should().Be(100);
        converted.Width.Should().Be(50);
        converted.Height.Should().Be(30);
        converted.Unit.Should().Be("cm");
    }

    [Fact]
    public void ConvertTo_FromMmToCm_ShouldConvertCorrectly()
    {
        // Arrange
        var dimensions = Dimensions.Create(1000, 500, 300, "mm");

        // Act
        Dimensions converted = dimensions.ConvertTo("cm");

        // Assert
        converted.Length.Should().Be(100);
        converted.Width.Should().Be(50);
        converted.Height.Should().Be(30);
        converted.Unit.Should().Be("cm");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var dimensions = Dimensions.Create(100.5m, 50.25m, 30.75m, "cm");

        // Act
        var result = dimensions.ToString();

        // Assert
        result.Should().Be("100.5 x 50.25 x 30.75 cm");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var dimensions1 = Dimensions.Create(100, 50, 30, "cm");
        var dimensions2 = Dimensions.Create(100, 50, 30, "cm");

        // Act & Assert
        dimensions1.Should().Be(dimensions2);
        (dimensions1 == dimensions2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var dimensions1 = Dimensions.Create(100, 50, 30, "cm");
        var dimensions2 = Dimensions.Create(100, 50, 30, "m");

        // Act & Assert
        dimensions1.Should().NotBe(dimensions2);
        (dimensions1 != dimensions2).Should().BeTrue();
    }
}
