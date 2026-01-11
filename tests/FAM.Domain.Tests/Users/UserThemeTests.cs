using FAM.Domain.Users.Entities;

using FluentAssertions;

namespace FAM.Domain.Tests.Users;

public class UserThemeTests
{
    [Fact]
    public void CreateDefault_ShouldCreateThemeWithDefaultValues()
    {
        // Arrange
        long userId = 1L;

        // Act
        UserTheme theme = UserTheme.CreateDefault(userId);

        // Assert
        theme.Should().NotBeNull();
        theme.UserId.Should().Be(userId);
        theme.Theme.Should().Be("System");
        theme.PrimaryColor.Should().Be("#2563EB");
        theme.Transparency.Should().Be(0.5m);
        theme.BorderRadius.Should().Be(8);
        theme.DarkTheme.Should().BeFalse();
        theme.PinNavbar.Should().BeFalse();
        theme.CompactMode.Should().BeFalse();
    }

    [Fact]
    public void Create_WithCustomValues_ShouldCreateThemeWithProvidedValues()
    {
        // Arrange
        long userId = 1L;
        string theme = "Dark";
        string primaryColor = "#FF5733";
        decimal transparency = 0.8m;
        int borderRadius = 12;
        bool darkTheme = true;
        bool pinNavbar = true;
        bool compactMode = true;

        // Act
        UserTheme userTheme = UserTheme.Create(
            userId,
            theme,
            primaryColor,
            transparency,
            borderRadius,
            darkTheme,
            pinNavbar,
            compactMode);

        // Assert
        userTheme.Should().NotBeNull();
        userTheme.UserId.Should().Be(userId);
        userTheme.Theme.Should().Be(theme);
        userTheme.PrimaryColor.Should().Be(primaryColor);
        userTheme.Transparency.Should().Be(transparency);
        userTheme.BorderRadius.Should().Be(borderRadius);
        userTheme.DarkTheme.Should().Be(darkTheme);
        userTheme.PinNavbar.Should().Be(pinNavbar);
        userTheme.CompactMode.Should().Be(compactMode);
    }

    [Fact]
    public void UpdateTheme_ShouldUpdateAllThemeProperties()
    {
        // Arrange
        UserTheme theme = UserTheme.CreateDefault(1L);
        string newTheme = "BlueJelly";
        string newPrimaryColor = "#3B82F6";
        decimal newTransparency = 0.7m;
        int newBorderRadius = 16;
        bool newDarkTheme = true;
        bool newPinNavbar = true;
        bool newCompactMode = false;

        // Act
        theme.UpdateTheme(
            newTheme,
            newPrimaryColor,
            newTransparency,
            newBorderRadius,
            newDarkTheme,
            newPinNavbar,
            newCompactMode);

        // Assert
        theme.Theme.Should().Be(newTheme);
        theme.PrimaryColor.Should().Be(newPrimaryColor);
        theme.Transparency.Should().Be(newTransparency);
        theme.BorderRadius.Should().Be(newBorderRadius);
        theme.DarkTheme.Should().Be(newDarkTheme);
        theme.PinNavbar.Should().Be(newPinNavbar);
        theme.CompactMode.Should().Be(newCompactMode);
    }

    [Theory]
    [InlineData("System")]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("Leaf")]
    [InlineData("Blossom")]
    [InlineData("BlueJelly")]
    public void Create_WithValidThemeName_ShouldSucceed(string themeName)
    {
        // Act
        UserTheme theme = UserTheme.Create(1L, themeName);

        // Assert
        theme.Theme.Should().Be(themeName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Create_WithValidTransparency_ShouldSucceed(decimal transparency)
    {
        // Act
        UserTheme theme = UserTheme.Create(1L, "System", null, transparency);

        // Assert
        theme.Transparency.Should().Be(transparency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(50)]
    public void Create_WithValidBorderRadius_ShouldSucceed(int borderRadius)
    {
        // Act
        UserTheme theme = UserTheme.Create(1L, "System", null, 0.5m, borderRadius);

        // Assert
        theme.BorderRadius.Should().Be(borderRadius);
    }
}
