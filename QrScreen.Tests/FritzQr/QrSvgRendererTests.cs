using System.Xml.Linq;
using QRCoder;
using QrScreen.Components.FritzQr.Encoding;
using QrScreen.Components.FritzQr.Models;
using QrScreen.Components.FritzQr.Rendering;

namespace QrScreen.Tests.FritzQr;

public class QrSvgRendererTests
{
	private const int Size = 21; // smallest (version 1) QR symbol size

	private static QrMatrix BuildTestMatrix(Action<bool[,], ModuleRole[,]> configure, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.Q)
	{
		var modules = new bool[Size, Size];
		var roles = new ModuleRole[Size, Size];
		configure(modules, roles);
		return new QrMatrix(Size, 1, eccLevel, modules, roles);
	}

	[Fact]
	public void BuildElements_SkipsLightModules()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[5, 5] = false;
			roles[5, 5] = ModuleRole.Data;
		});

		var elements = QrSvgRenderer.BuildElements(matrix, new FritzQrRenderOptions());

		Assert.Empty(elements);
	}

	[Fact]
	public void BuildElements_UsesDataShapeAndForegroundColorForDataModules()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[10, 10] = true;
			roles[10, 10] = ModuleRole.Data;
		});

		var options = new FritzQrRenderOptions
		{
			ForegroundColor = "#123456",
			DataModuleShape = ModuleShape.Circle
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		var element = Assert.Single(elements);
		Assert.Equal(10, element.X);
		Assert.Equal(10, element.Y);
		Assert.Equal(ModuleShape.Circle, element.Shape);
		Assert.Equal("#123456", element.Color);
	}

	[Fact]
	public void BuildElements_UsesTimingStyleForTimingModules()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[10, 6] = true;
			roles[10, 6] = ModuleRole.Timing;
		});

		var options = new FritzQrRenderOptions
		{
			TimingModuleShape = ModuleShape.Hexagon,
			TimingModuleColor = "#654321"
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		var element = Assert.Single(elements);
		Assert.Equal(ModuleShape.Hexagon, element.Shape);
		Assert.Equal("#654321", element.Color);
	}

	[Fact]
	public void BuildElements_UsesAlignmentStyleForAlignmentModules()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[10, 10] = true;
			roles[10, 10] = ModuleRole.Alignment;
		});

		var options = new FritzQrRenderOptions
		{
			AlignmentModuleShape = ModuleShape.Star,
			AlignmentModuleColor = "#abcdef"
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		var element = Assert.Single(elements);
		Assert.Equal(ModuleShape.Star, element.Shape);
		Assert.Equal("#abcdef", element.Color);
	}

	[Fact]
	public void BuildElements_AppliesIndependentStylePerFinderCorner()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			// top-left outer
			modules[0, 0] = true;
			roles[0, 0] = ModuleRole.FinderOuter;
			// top-right inner
			modules[Size - 4, 3] = true;
			roles[Size - 4, 3] = ModuleRole.FinderInner;
			// bottom-left outer
			modules[0, Size - 1] = true;
			roles[0, Size - 1] = ModuleRole.FinderOuter;
		});

		var options = new FritzQrRenderOptions
		{
			FinderStyles = new FinderStyles
			{
				TopLeft = new FinderCornerStyle { OuterShape = ModuleShape.Square, OuterColor = "#111111" },
				TopRight = new FinderCornerStyle { InnerShape = ModuleShape.Circle, InnerColor = "#222222" },
				BottomLeft = new FinderCornerStyle { OuterShape = ModuleShape.RoundedSquare, OuterColor = "#333333" }
			}
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		var topLeft = Assert.Single(elements, e => e.X == 0 && e.Y == 0);
		Assert.Equal(ModuleShape.Square, topLeft.Shape);
		Assert.Equal("#111111", topLeft.Color);

		var topRight = Assert.Single(elements, e => e.X == Size - 4 && e.Y == 3);
		Assert.Equal(ModuleShape.Circle, topRight.Shape);
		Assert.Equal("#222222", topRight.Color);

		var bottomLeft = Assert.Single(elements, e => e.X == 0 && e.Y == Size - 1);
		Assert.Equal(ModuleShape.RoundedSquare, bottomLeft.Shape);
		Assert.Equal("#333333", bottomLeft.Color);
	}

	[Fact]
	public void BuildElements_AllowsExpandedFinderMarkerShapes()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[0, 0] = true;
			roles[0, 0] = ModuleRole.FinderOuter;
		});

		var options = new FritzQrRenderOptions
		{
			FinderStyles = new FinderStyles
			{
				TopLeft = new FinderCornerStyle { OuterShape = ModuleShape.Pentagon, OuterColor = "#111111" }
			}
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		var element = Assert.Single(elements);
		Assert.Equal(ModuleShape.Pentagon, element.Shape);
		Assert.Equal("#111111", element.Color);
	}

	[Fact]
	public void Render_WrapsElementsInSvgWithQuietZoneOffsetAndBackground()
	{
		var matrix = BuildTestMatrix((modules, roles) =>
		{
			modules[0, 0] = true;
			roles[0, 0] = ModuleRole.Data;
		});

		var options = new FritzQrRenderOptions { BackgroundColor = "#ffffff", QuietZoneModules = 4 };

		string svg = QrSvgRenderer.Render(matrix, options);

		Assert.StartsWith("<svg", svg);
		Assert.Contains($"viewBox=\"0 0 {Size + 8} {Size + 8}\"", svg);
		Assert.Contains("fill=\"#ffffff\"", svg);
		Assert.Contains("translate(4,4)", svg);
	}

	[Fact]
	public void Render_ProducesWellFormedXml()
	{
		var matrix = QrMatrixBuilder.Build("https://example.com/fritz");
		string svg = QrSvgRenderer.Render(matrix, new FritzQrRenderOptions());

		var doc = XDocument.Parse(svg); // throws if malformed

		Assert.Equal("svg", doc.Root!.Name.LocalName);
	}

	[Fact]
	public void BuildElements_ExcludesDataModulesUnderTheLogoKnockout()
	{
		var matrix = QrMatrixBuilder.Build("https://example.com/fritz", QRCodeGenerator.ECCLevel.H);
		var options = new FritzQrRenderOptions
		{
			Logo = new LogoOptions { ImageBytes = new byte[] { 1, 2, 3 }, SizePercent = 30 }
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);
		var placement = LogoLayoutCalculator.Calculate(matrix.Size, matrix.EccLevel, options.Logo);

		Assert.DoesNotContain(elements, e =>
			matrix.GetRole(e.X, e.Y) == ModuleRole.Data &&
			e.X + 1 > placement.KnockoutX && e.X < placement.KnockoutX + placement.KnockoutSize &&
			e.Y + 1 > placement.KnockoutY && e.Y < placement.KnockoutY + placement.KnockoutSize);
	}

	[Fact]
	public void BuildElements_NeverExcludesFinderPatternModules()
	{
		// A logo requesting the max size shouldn't be able to eat into the (far away) finder patterns.
		var matrix = QrMatrixBuilder.Build("https://example.com/fritz", QRCodeGenerator.ECCLevel.H);
		var options = new FritzQrRenderOptions
		{
			Logo = new LogoOptions { ImageBytes = new byte[] { 1, 2, 3 }, SizePercent = 100 }
		};

		var elements = QrSvgRenderer.BuildElements(matrix, options);

		Assert.Contains(elements, e => e.X == 0 && e.Y == 0); // top-left finder pattern corner
	}

	[Fact]
	public void Render_EmbedsBase64ImageAndKnockoutRectWhenLogoPresent()
	{
		var matrix = QrMatrixBuilder.Build("https://example.com/fritz", QRCodeGenerator.ECCLevel.H);
		byte[] imageBytes = { 0xDE, 0xAD, 0xBE, 0xEF };
		var options = new FritzQrRenderOptions
		{
			Logo = new LogoOptions { ImageBytes = imageBytes, MimeType = "image/png", SizePercent = 20 }
		};

		string svg = QrSvgRenderer.Render(matrix, options);
		string expectedBase64 = Convert.ToBase64String(imageBytes);

		Assert.Contains($"data:image/png;base64,{expectedBase64}", svg);
		Assert.Contains("<image", svg);

		var doc = XDocument.Parse(svg);
		Assert.Equal("svg", doc.Root!.Name.LocalName);
	}

	[Fact]
	public void Render_FallsBackToPngMimeTypeForDisallowedValues()
	{
		var matrix = QrMatrixBuilder.Build("https://example.com/fritz", QRCodeGenerator.ECCLevel.H);
		var options = new FritzQrRenderOptions
		{
			Logo = new LogoOptions { ImageBytes = new byte[] { 1 }, MimeType = "text/html; evil=1" }
		};

		string svg = QrSvgRenderer.Render(matrix, options);

		Assert.Contains("data:image/png;base64,", svg);
		Assert.DoesNotContain("text/html", svg);
	}
}
