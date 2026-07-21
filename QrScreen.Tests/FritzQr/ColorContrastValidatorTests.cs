using QrScreen.Components.FritzQr.Rendering;

namespace QrScreen.Tests.FritzQr;

public class ColorContrastValidatorTests
{
	[Fact]
	public void CalculateContrastRatio_BlackVsWhite_IsMaximum()
	{
		double ratio = ColorContrastValidator.CalculateContrastRatio("#000000", "#FFFFFF");

		Assert.Equal(21.0, ratio, precision: 1);
	}

	[Fact]
	public void CalculateContrastRatio_IdenticalColors_IsOne()
	{
		double ratio = ColorContrastValidator.CalculateContrastRatio("#336699", "#336699");

		Assert.Equal(1.0, ratio, precision: 2);
	}

	[Fact]
	public void CalculateContrastRatio_IsSymmetric()
	{
		double ratioAb = ColorContrastValidator.CalculateContrastRatio("#123456", "#abcdef");
		double ratioBa = ColorContrastValidator.CalculateContrastRatio("#abcdef", "#123456");

		Assert.Equal(ratioAb, ratioBa, precision: 6);
	}

	[Theory]
	[InlineData("#000000", "#FFFFFF", true)]
	[InlineData("#FFFFFF", "#FEFEFE", false)]
	[InlineData("rgb(0,0,0)", "rgb(255,255,255)", true)]
	[InlineData("rgb(255, 255, 255)", "rgb(240, 240, 240)", false)]
	public void HasSufficientContrast_MatchesExpectation(string colorA, string colorB, bool expected)
	{
		bool result = ColorContrastValidator.HasSufficientContrast(colorA, colorB);

		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("#abc")]
	[InlineData("#AABBCC")]
	[InlineData("rgb(1,2,3)")]
	[InlineData("rgba(1, 2, 3, 0.5)")]
	public void ParseColor_AcceptsSupportedFormats(string color)
	{
		var parsed = ColorContrastValidator.ParseColor(color);

		Assert.InRange(parsed.R, 0, 255);
		Assert.InRange(parsed.G, 0, 255);
		Assert.InRange(parsed.B, 0, 255);
	}

	[Fact]
	public void ParseColor_ShorthandHexMatchesExpandedHex()
	{
		var shorthand = ColorContrastValidator.ParseColor("#abc");
		var expanded = ColorContrastValidator.ParseColor("#aabbcc");

		Assert.Equal(expanded, shorthand);
	}

	[Theory]
	[InlineData("")]
	[InlineData("not-a-color")]
	[InlineData("#zzzzzz")]
	public void ParseColor_ThrowsForUnsupportedInput(string color)
	{
		Assert.ThrowsAny<Exception>(() => ColorContrastValidator.ParseColor(color));
	}
}
