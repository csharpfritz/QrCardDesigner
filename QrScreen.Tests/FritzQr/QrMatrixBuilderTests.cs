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
}
