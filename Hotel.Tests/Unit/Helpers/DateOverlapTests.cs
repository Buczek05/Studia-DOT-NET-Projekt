using FluentAssertions;

namespace Hotel.Tests.Unit.Helpers;

public class DateOverlapTests
{
    /// <summary>
    /// Date overlap formula: (A.start < B.end) AND (B.start < A.end)
    /// </summary>
    private static bool HasOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        => aStart < bEnd && bStart < aEnd;

    [Theory]
    [InlineData("2025-06-01", "2025-06-05", "2025-06-03", "2025-06-07", true)]  // partial overlap
    [InlineData("2025-06-01", "2025-06-05", "2025-06-05", "2025-06-10", false)] // touching (checkout = checkin)
    [InlineData("2025-06-01", "2025-06-05", "2025-06-10", "2025-06-15", false)] // disjoint
    [InlineData("2025-06-01", "2025-06-10", "2025-06-03", "2025-06-07", true)]  // B inside A
    [InlineData("2025-06-03", "2025-06-07", "2025-06-01", "2025-06-10", true)]  // A inside B
    [InlineData("2025-06-01", "2025-06-05", "2025-06-01", "2025-06-05", true)]  // identical
    [InlineData("2025-06-01", "2025-06-05", "2025-06-04", "2025-06-06", true)]  // overlap at end
    [InlineData("2025-06-03", "2025-06-07", "2025-06-01", "2025-06-04", true)]  // overlap at start
    public void HasOverlap_ShouldDetectCollisions(
        string aStartStr, string aEndStr,
        string bStartStr, string bEndStr,
        bool expectedOverlap)
    {
        // Arrange
        var aStart = DateTime.Parse(aStartStr);
        var aEnd = DateTime.Parse(aEndStr);
        var bStart = DateTime.Parse(bStartStr);
        var bEnd = DateTime.Parse(bEndStr);

        // Act
        var result = HasOverlap(aStart, aEnd, bStart, bEnd);

        // Assert
        result.Should().Be(expectedOverlap);
    }

    [Fact]
    public void HasOverlap_WhenSameDay_ShouldNotOverlap()
    {
        // Guest A checks out on June 5, Guest B checks in on June 5
        // This should NOT be a collision (checkout day = checkin day is OK)
        var aStart = new DateTime(2025, 6, 1);
        var aEnd = new DateTime(2025, 6, 5);
        var bStart = new DateTime(2025, 6, 5);
        var bEnd = new DateTime(2025, 6, 10);

        var result = HasOverlap(aStart, aEnd, bStart, bEnd);

        result.Should().BeFalse("checkout date equals checkin date should not be a conflict");
    }

    [Fact]
    public void HasOverlap_WhenCompletelyBefore_ShouldNotOverlap()
    {
        var aStart = new DateTime(2025, 6, 1);
        var aEnd = new DateTime(2025, 6, 5);
        var bStart = new DateTime(2025, 6, 10);
        var bEnd = new DateTime(2025, 6, 15);

        var result = HasOverlap(aStart, aEnd, bStart, bEnd);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasOverlap_WhenCompletelyAfter_ShouldNotOverlap()
    {
        var aStart = new DateTime(2025, 6, 10);
        var aEnd = new DateTime(2025, 6, 15);
        var bStart = new DateTime(2025, 6, 1);
        var bEnd = new DateTime(2025, 6, 5);

        var result = HasOverlap(aStart, aEnd, bStart, bEnd);

        result.Should().BeFalse();
    }
}
