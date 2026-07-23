using QrScreen.Components.FritzQr.Encoding;
using QrScreen.Components.FritzQr.Models;
using QRCoder;

namespace QrScreen.Tests.FritzQr;

public class QrMatrixBuilderTests
{
	[Fact]
	public void Build_ThrowsForNullOrEmptyData()
	{
		Assert.Throws<ArgumentException>(() => QrMatrixBuilder.Build(""));
		Assert.Throws<ArgumentException>(() => QrMatrixBuilder.Build(null!));
	}

	[Fact]
	public void Build_ProducesSquareMatrixMatchingVersionFormula()
	{
		var matrix = QrMatrixBuilder.Build("Hello Fritz!", QRCodeGenerator.ECCLevel.Q);

		int expectedSize = 21 + (4 * (matrix.Version - 1));
		Assert.Equal(expectedSize, matrix.Size);
		Assert.Equal(matrix.Size, matrix.Modules.GetLength(0));
		Assert.Equal(matrix.Size, matrix.Modules.GetLength(1));
	}

	[Fact]
	public void Build_ClassifiesTopLeftFinderPattern()
	{
		var matrix = QrMatrixBuilder.Build("Hello Fritz!");

		// Outer ring corners/edges of the top-left 7x7 finder pattern.
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(0, 0));
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(6, 0));
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(0, 6));
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(6, 6));
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(3, 0));

		// 3x3 inner eye.
		Assert.Equal(ModuleRole.FinderInner, matrix.GetRole(2, 2));
		Assert.Equal(ModuleRole.FinderInner, matrix.GetRole(3, 3));
		Assert.Equal(ModuleRole.FinderInner, matrix.GetRole(4, 4));

		// The separator ring between outer/inner (always light, but still classified as Data).
		Assert.Equal(ModuleRole.Data, matrix.GetRole(1, 1));

		// All finder pattern modules should be dark, except the separator ring.
		Assert.True(matrix.IsDark(0, 0));
		Assert.True(matrix.IsDark(3, 3));
		Assert.False(matrix.IsDark(1, 1));
	}

	[Fact]
	public void Build_ClassifiesTopRightAndBottomLeftFinderPatterns()
	{
		var matrix = QrMatrixBuilder.Build("Hello Fritz!");
		int size = matrix.Size;

		// Top-right finder pattern occupies columns [size-7, size-1], rows [0, 6].
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(size - 1, 0));
		Assert.Equal(ModuleRole.FinderInner, matrix.GetRole(size - 4, 3));

		// Bottom-left finder pattern occupies columns [0, 6], rows [size-7, size-1].
		Assert.Equal(ModuleRole.FinderOuter, matrix.GetRole(0, size - 1));
		Assert.Equal(ModuleRole.FinderInner, matrix.GetRole(3, size - 4));

		// Bottom-right corner has no finder pattern - must be classified as Data.
		Assert.Equal(ModuleRole.Data, matrix.GetRole(size - 1, size - 1));
	}

	[Fact]
	public void Build_ClassifiesTimingPatternModules()
	{
		var matrix = QrMatrixBuilder.Build("Hello Fritz!");

		Assert.Equal(ModuleRole.Timing, matrix.GetRole(10, 6));
		Assert.Equal(ModuleRole.Timing, matrix.GetRole(6, 10));
	}

	[Fact]
	public void Build_ClassifiesAlignmentPatternModules()
	{
		var matrix = QrMatrixBuilder.Build(new string('A', 120), QRCodeGenerator.ECCLevel.Q);
		Assert.True(matrix.Version > 1);

		int[] centers = GetAlignmentCenters(matrix.Version);
		Assert.NotEmpty(centers);

		int center = centers[^1];
		Assert.Equal(ModuleRole.Alignment, matrix.GetRole(center, center));
	}

	[Fact]
	public void Build_HasAtLeastOneDataModule()
	{
		var matrix = QrMatrixBuilder.Build("Hello Fritz!");

		bool foundData = false;
		for (int y = 0; y < matrix.Size && !foundData; y++)
		{
			for (int x = 0; x < matrix.Size; x++)
			{
				if (matrix.GetRole(x, y) == ModuleRole.Data && matrix.IsDark(x, y))
				{
					foundData = true;
					break;
				}
			}
		}

		Assert.True(foundData, "Expected at least one dark data module outside the finder patterns.");
	}

	private static int[] GetAlignmentCenters(int version) => version switch
	{
		1 => Array.Empty<int>(),
		2 => new[] { 6, 18 },
		3 => new[] { 6, 22 },
		4 => new[] { 6, 26 },
		5 => new[] { 6, 30 },
		6 => new[] { 6, 34 },
		7 => new[] { 6, 22, 38 },
		8 => new[] { 6, 24, 42 },
		9 => new[] { 6, 26, 46 },
		10 => new[] { 6, 28, 50 },
		11 => new[] { 6, 30, 54 },
		12 => new[] { 6, 32, 58 },
		13 => new[] { 6, 34, 62 },
		14 => new[] { 6, 26, 46, 66 },
		15 => new[] { 6, 26, 48, 70 },
		16 => new[] { 6, 26, 50, 74 },
		17 => new[] { 6, 30, 54, 78 },
		18 => new[] { 6, 30, 56, 82 },
		19 => new[] { 6, 30, 58, 86 },
		20 => new[] { 6, 34, 62, 90 },
		21 => new[] { 6, 28, 50, 72, 94 },
		22 => new[] { 6, 26, 50, 74, 98 },
		23 => new[] { 6, 30, 54, 78, 102 },
		24 => new[] { 6, 28, 54, 80, 106 },
		25 => new[] { 6, 32, 58, 84, 110 },
		26 => new[] { 6, 30, 58, 86, 114 },
		27 => new[] { 6, 34, 62, 90, 118 },
		28 => new[] { 6, 26, 50, 74, 98, 122 },
		29 => new[] { 6, 30, 54, 78, 102, 126 },
		30 => new[] { 6, 26, 52, 78, 104, 130 },
		31 => new[] { 6, 30, 56, 82, 108, 134 },
		32 => new[] { 6, 34, 60, 86, 112, 138 },
		33 => new[] { 6, 30, 58, 86, 114, 142 },
		34 => new[] { 6, 34, 62, 90, 118, 146 },
		35 => new[] { 6, 30, 54, 78, 102, 126, 150 },
		36 => new[] { 6, 24, 50, 76, 102, 128, 154 },
		37 => new[] { 6, 28, 54, 80, 106, 132, 158 },
		38 => new[] { 6, 32, 58, 84, 110, 136, 162 },
		39 => new[] { 6, 26, 54, 82, 110, 138, 166 },
		40 => new[] { 6, 30, 58, 86, 114, 142, 170 },
		_ => throw new ArgumentOutOfRangeException(nameof(version))
	};
}
