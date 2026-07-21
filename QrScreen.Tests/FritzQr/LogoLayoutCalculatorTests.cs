using QRCoder;
using QrScreen.Components.FritzQr.Models;
using QrScreen.Components.FritzQr.Rendering;

namespace QrScreen.Tests.FritzQr;

public class LogoLayoutCalculatorTests
{
	[Theory]
	[InlineData(QRCodeGenerator.ECCLevel.L, 15)]
	[InlineData(QRCodeGenerator.ECCLevel.M, 20)]
	[InlineData(QRCodeGenerator.ECCLevel.Q, 25)]
	[InlineData(QRCodeGenerator.ECCLevel.H, 30)]
	public void GetMaxSizePercent_ReturnsExpectedCapPerEccLevel(QRCodeGenerator.ECCLevel eccLevel, double expectedMax)
	{
		Assert.Equal(expectedMax, LogoLayoutCalculator.GetMaxSizePercent(eccLevel));
	}

	[Fact]
	public void Calculate_ClampsRequestedSizeToEccMaximum()
	{
		var options = new LogoOptions { ImageBytes = new byte[] { 1 }, SizePercent = 90 };

		var placement = LogoLayoutCalculator.Calculate(100, QRCodeGenerator.ECCLevel.M, options);

		// M caps at 20% => knockout should be 20 modules wide for a 100-module symbol.
		Assert.Equal(20, placement.KnockoutSize, precision: 6);
	}

	[Fact]
	public void Calculate_CentersKnockoutAndLogoInTheMatrix()
	{
		var options = new LogoOptions { ImageBytes = new byte[] { 1 }, SizePercent = 20, PaddingPercent = 0 };

		var placement = LogoLayoutCalculator.Calculate(100, QRCodeGenerator.ECCLevel.H, options);

		Assert.Equal(40, placement.KnockoutX, precision: 6); // (100 - 20) / 2
		Assert.Equal(40, placement.KnockoutY, precision: 6);
		Assert.Equal(20, placement.KnockoutSize, precision: 6);
		// No padding requested, so the logo should exactly fill the knockout.
		Assert.Equal(placement.KnockoutX, placement.LogoX, precision: 6);
		Assert.Equal(placement.KnockoutSize, placement.LogoSize, precision: 6);
	}

	[Fact]
	public void Calculate_PaddingShrinksLogoWithinKnockout()
	{
		var options = new LogoOptions { ImageBytes = new byte[] { 1 }, SizePercent = 20, PaddingPercent = 10 };

		var placement = LogoLayoutCalculator.Calculate(100, QRCodeGenerator.ECCLevel.H, options);

		// 10% padding on each side of a 20-module knockout = 2 modules per side.
		Assert.Equal(16, placement.LogoSize, precision: 6);
		Assert.True(placement.LogoX > placement.KnockoutX);
	}
}
