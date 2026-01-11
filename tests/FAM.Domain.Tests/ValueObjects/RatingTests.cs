using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class RatingTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateRating()
    {
        // Arrange
        int value = 4;

        // Act
        Rating rating = Rating.Create(value);

        // Assert
        rating.Should().NotBeNull();
        rating.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithDecimalValue_ShouldRoundAndCreateRating()
    {
        // Arrange
        decimal value = 4.7m;

        // Act
        Rating rating = Rating.Create(value);

        // Assert
        rating.Value.Should().Be(5);
    }

    [Fact]
    public void Create_WithValueTooLow_ShouldThrowDomainException()
    {
        // Arrange
        int value = 0;

        // Act
        Action act = () => Rating.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void Create_WithValueTooHigh_ShouldThrowDomainException()
    {
        // Arrange
        int value = 6;

        // Act
        Action act = () => Rating.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void Max_ShouldReturnFiveStars()
    {
        // Act
        Rating maxRating = Rating.Max;

        // Assert
        maxRating.Value.Should().Be(5);
    }

    [Fact]
    public void Min_ShouldReturnOneStar()
    {
        // Act
        Rating minRating = Rating.Min;

        // Assert
        minRating.Value.Should().Be(1);
    }

    [Fact]
    public void Average_ShouldReturnThreeStars()
    {
        // Act
        Rating averageRating = Rating.Average;

        // Assert
        averageRating.Value.Should().Be(3);
    }

    [Fact]
    public void IsHighRating_WithFourStars_ShouldReturnTrue()
    {
        // Arrange
        Rating rating = Rating.Create(4);

        // Act
        bool result = rating.IsHighRating();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsHighRating_WithThreeStars_ShouldReturnFalse()
    {
        // Arrange
        Rating rating = Rating.Create(3);

        // Act
        bool result = rating.IsHighRating();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLowRating_WithTwoStars_ShouldReturnTrue()
    {
        // Arrange
        Rating rating = Rating.Create(2);

        // Act
        bool result = rating.IsLowRating();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLowRating_WithThreeStars_ShouldReturnFalse()
    {
        // Arrange
        Rating rating = Rating.Create(3);

        // Act
        bool result = rating.IsLowRating();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAverageRating_WithThreeStars_ShouldReturnTrue()
    {
        // Arrange
        Rating rating = Rating.Create(3);

        // Act
        bool result = rating.IsAverageRating();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAverageRating_WithFourStars_ShouldReturnFalse()
    {
        // Arrange
        Rating rating = Rating.Create(4);

        // Act
        bool result = rating.IsAverageRating();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetDescription_WithFiveStars_ShouldReturnExcellent()
    {
        // Arrange
        Rating rating = Rating.Create(5);

        // Act
        string result = rating.GetDescription();

        // Assert
        result.Should().Be("Excellent");
    }

    [Fact]
    public void GetStars_WithThreeStars_ShouldReturnCorrectStars()
    {
        // Arrange
        Rating rating = Rating.Create(3);

        // Act
        string result = rating.GetStars();

        // Assert
        result.Should().Be("★★★☆☆");
    }

    [Fact]
    public void CompareTo_WithHigherRating_ShouldReturnNegative()
    {
        // Arrange
        Rating rating1 = Rating.Create(3);
        Rating rating2 = Rating.Create(4);

        // Act
        int result = rating1.CompareTo(rating2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void AverageWith_ShouldReturnAverageRating()
    {
        // Arrange
        Rating rating1 = Rating.Create(3);
        Rating rating2 = Rating.Create(5);

        // Act
        Rating result = rating1.AverageWith(rating2);

        // Assert
        result.Value.Should().Be(4);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToInt()
    {
        // Arrange
        Rating rating = Rating.Create(4);

        // Act
        int value = rating;

        // Assert
        value.Should().Be(4);
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromInt()
    {
        // Arrange
        int value = 4;

        // Act
        Rating rating = (Rating)value;

        // Assert
        rating.Value.Should().Be(4);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        Rating rating = Rating.Create(4);

        // Act
        string result = rating.ToString();

        // Assert
        result.Should().Be("4/5 stars");
    }
}
