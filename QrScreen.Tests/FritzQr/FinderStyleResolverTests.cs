using QrScreen.Components.FritzQr.Models;
using QrScreen.Components.FritzQr.Rendering;

namespace QrScreen.Tests.FritzQr;

public class FinderStyleResolverTests
{
	[Fact]
	public void Resolve_ReturnsExplicitStylesUnchangedWhenProvided()
	{
		var explicitStyles = new FinderStyles
		{
			TopLeft = new FinderCornerStyle { OuterColor = "#111111" }
		};

		var resolved = FinderStyleResolver.Resolve(explicitStyles, markerColor: "#ff00ff", foregroundColor: "#000000");

		Assert.Same(explicitStyles, resolved);
	}

	[Fact]
	public void Resolve_UsesMarkerColorForAllCornersWhenExplicitStylesAreNull()
	{
		var resolved = FinderStyleResolver.Resolve(null, markerColor: "#ff00ff", foregroundColor: "#000000");

		Assert.Equal("#ff00ff", resolved.TopLeft.OuterColor);
		Assert.Equal("#ff00ff", resolved.TopLeft.InnerColor);
		Assert.Equal("#ff00ff", resolved.TopRight.OuterColor);
		Assert.Equal("#ff00ff", resolved.TopRight.InnerColor);
		Assert.Equal("#ff00ff", resolved.BottomLeft.OuterColor);
		Assert.Equal("#ff00ff", resolved.BottomLeft.InnerColor);
	}

	[Fact]
	public void Resolve_FallsBackToForegroundColorWhenNeitherExplicitStylesNorMarkerColorAreSet()
	{
		var resolved = FinderStyleResolver.Resolve(null, markerColor: null, foregroundColor: "rgb(128,0,255)");

		Assert.Equal("rgb(128,0,255)", resolved.TopLeft.OuterColor);
		Assert.Equal("rgb(128,0,255)", resolved.TopRight.InnerColor);
		Assert.Equal("rgb(128,0,255)", resolved.BottomLeft.OuterColor);
	}
}
