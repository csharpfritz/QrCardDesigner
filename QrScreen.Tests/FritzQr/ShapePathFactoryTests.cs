using QrScreen.Components.FritzQr.Rendering;

namespace QrScreen.Tests.FritzQr;

public class ShapePathFactoryTests
{
	[Fact]
	public void CreateSquare_ProducesRectWithExpectedAttributes()
	{
		string markup = ShapePathFactory.CreateSquare(1, 2, 3, "#112233");

		Assert.Contains("<rect", markup);
		Assert.Contains("x=\"1\"", markup);
		Assert.Contains("y=\"2\"", markup);
		Assert.Contains("width=\"3\"", markup);
		Assert.Contains("height=\"3\"", markup);
		Assert.Contains("fill=\"#112233\"", markup);
	}

	[Fact]
	public void CreateRoundedSquare_IncludesCornerRadius()
	{
		string markup = ShapePathFactory.CreateRoundedSquare(0, 0, 10, "#000000");

		Assert.Contains("rx=\"3\"", markup);
		Assert.Contains("ry=\"3\"", markup);
	}

	[Fact]
	public void CreateCircle_ComputesCenterAndRadiusFromBoundingBox()
	{
		string markup = ShapePathFactory.CreateCircle(2, 4, 6, "#ff0000");

		// center = origin + radius; radius = size / 2
		Assert.Contains("cx=\"5\"", markup);
		Assert.Contains("cy=\"7\"", markup);
		Assert.Contains("r=\"3\"", markup);
	}

	[Theory]
	[InlineData(3)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(8)]
	public void GetRegularPolygonPoints_ReturnsOnePointPerSide(int sides)
	{
		string points = ShapePathFactory.GetRegularPolygonPoints(0, 0, 1, sides);

		Assert.Equal(sides, points.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
	}

	[Fact]
	public void GetRegularPolygonPoints_FirstPointRespectsRotation()
	{
		// With 0 rotation, the first vertex of a unit-radius polygon centered at the origin
		// should land at (radius, 0) = (1, 0).
		string points = ShapePathFactory.GetRegularPolygonPoints(0, 0, 1, 4, rotationDegrees: 0);
		string firstPoint = points.Split(' ')[0];

		Assert.Equal("1,0", firstPoint);
	}

	[Fact]
	public void GetRegularPolygonPoints_ThrowsForFewerThanThreeSides()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => ShapePathFactory.GetRegularPolygonPoints(0, 0, 1, 2));
	}

	[Fact]
	public void SanitizeColor_EncodesAttributeBreakoutCharacters()
	{
		string malicious = "red\" onmouseover=\"alert(1)";

		string sanitized = ShapePathFactory.SanitizeColor(malicious);

		Assert.DoesNotContain("\"", sanitized);
		Assert.DoesNotContain("onmouseover=\"alert", sanitized);
	}

	[Fact]
	public void CreateMarkup_SanitizesColorForAllShapes()
	{
		string malicious = "red\"/><script>alert(1)</script>";

		string square = ShapePathFactory.CreateSquare(0, 0, 1, malicious);
		string circle = ShapePathFactory.CreateCircle(0, 0, 1, malicious);
		string rounded = ShapePathFactory.CreateRoundedSquare(0, 0, 1, malicious);

		Assert.DoesNotContain("<script>", square);
		Assert.DoesNotContain("<script>", circle);
		Assert.DoesNotContain("<script>", rounded);
	}
}
