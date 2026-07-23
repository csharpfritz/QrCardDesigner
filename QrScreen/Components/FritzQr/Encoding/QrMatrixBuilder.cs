using System.Collections;
using QRCoder;
using QrScreen.Components.FritzQr.Models;

namespace QrScreen.Components.FritzQr.Encoding;

/// <summary>
/// Builds a role-classified <see cref="QrMatrix"/> from raw payload text using QRCoder's
/// spec-compliant encoder (Reed-Solomon ECC, masking, version selection). QRCoder is used
/// ONLY to obtain the raw module matrix - all visual rendering is fully custom.
/// </summary>
public static class QrMatrixBuilder
{
	// QRCoder always wraps the encoded symbol in a fixed 4-module quiet zone border
	// (see QRCodeGenerator.PlaceModules: `new QRCodeData(version, true)` + `size = ModuleMatrix.Count - 8`).
	private const int QrCoderQuietZoneModules = 4;

	public static QrMatrix Build(string data, QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.Q)
	{
		if (string.IsNullOrEmpty(data))
			throw new ArgumentException("Data must not be null or empty.", nameof(data));

		using var generator = new QRCodeGenerator();
		using var qrCodeData = generator.CreateQrCode(data, eccLevel);

		return BuildFromModuleMatrix(qrCodeData.ModuleMatrix, eccLevel);
	}

	/// <summary>
	/// Strips QRCoder's built-in quiet zone and classifies every remaining module's role.
	/// Exposed internally so tests can exercise classification without depending on QRCoder's output shape.
	/// </summary>
	internal static QrMatrix BuildFromModuleMatrix(IReadOnlyList<BitArray> moduleMatrix, QRCodeGenerator.ECCLevel eccLevel)
	{
		int fullSize = moduleMatrix.Count;
		int size = fullSize - (QrCoderQuietZoneModules * 2);
		if (size <= 0)
			throw new InvalidOperationException("Unexpected QR module matrix size.");

		int version = ((size - 21) / 4) + 1;

		var modules = new bool[size, size];
		var roles = new ModuleRole[size, size];

		for (int y = 0; y < size; y++)
		{
			var row = moduleMatrix[y + QrCoderQuietZoneModules];
			for (int x = 0; x < size; x++)
			{
				modules[x, y] = row[x + QrCoderQuietZoneModules];
				roles[x, y] = ClassifyRole(x, y, size, version);
			}
		}

		return new QrMatrix(size, version, eccLevel, modules, roles);
	}

	private static ModuleRole ClassifyRole(int x, int y, int size, int version)
	{
		if (TryGetFinderLocal(x, y, 0, 0, out var localX, out var localY) ||
			TryGetFinderLocal(x, y, size - 7, 0, out localX, out localY) ||
			TryGetFinderLocal(x, y, 0, size - 7, out localX, out localY))
		{
			if (localX == 0 || localX == 6 || localY == 0 || localY == 6)
				return ModuleRole.FinderOuter;
			if (localX is >= 2 and <= 4 && localY is >= 2 and <= 4)
				return ModuleRole.FinderInner;
		}

		if (x == 6 || y == 6)
			return ModuleRole.Timing;

		if (IsAlignmentModule(x, y, size, version))
			return ModuleRole.Alignment;

		return ModuleRole.Data;
	}

	private static bool IsAlignmentModule(int x, int y, int size, int version)
	{
		int[] positions = GetAlignmentPatternPositions(version);
		foreach (int centerX in positions)
		{
			foreach (int centerY in positions)
			{
				if (AlignmentOverlapsFinder(centerX, centerY, size))
					continue;

				if (Math.Abs(x - centerX) <= 2 && Math.Abs(y - centerY) <= 2)
					return true;
			}
		}

		return false;
	}

	private static bool AlignmentOverlapsFinder(int centerX, int centerY, int size)
	{
		return Intersects(centerX - 2, centerY - 2, 5, 5, 0, 0, 7, 7) ||
			Intersects(centerX - 2, centerY - 2, 5, 5, size - 7, 0, 7, 7) ||
			Intersects(centerX - 2, centerY - 2, 5, 5, 0, size - 7, 7, 7);
	}

	private static bool Intersects(int ax, int ay, int aw, int ah, int bx, int by, int bw, int bh)
	{
		return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
	}

	private static int[] GetAlignmentPatternPositions(int version)
	{
		if (version < 1 || version > AlignmentPatternPositions.Length)
			throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported QR version.");

		return AlignmentPatternPositions[version - 1];
	}

	private static readonly int[][] AlignmentPatternPositions =
	{
		Array.Empty<int>(),
		new[] { 6, 18 },
		new[] { 6, 22 },
		new[] { 6, 26 },
		new[] { 6, 30 },
		new[] { 6, 34 },
		new[] { 6, 22, 38 },
		new[] { 6, 24, 42 },
		new[] { 6, 26, 46 },
		new[] { 6, 28, 50 },
		new[] { 6, 30, 54 },
		new[] { 6, 32, 58 },
		new[] { 6, 34, 62 },
		new[] { 6, 26, 46, 66 },
		new[] { 6, 26, 48, 70 },
		new[] { 6, 26, 50, 74 },
		new[] { 6, 30, 54, 78 },
		new[] { 6, 30, 56, 82 },
		new[] { 6, 30, 58, 86 },
		new[] { 6, 34, 62, 90 },
		new[] { 6, 28, 50, 72, 94 },
		new[] { 6, 26, 50, 74, 98 },
		new[] { 6, 30, 54, 78, 102 },
		new[] { 6, 28, 54, 80, 106 },
		new[] { 6, 32, 58, 84, 110 },
		new[] { 6, 30, 58, 86, 114 },
		new[] { 6, 34, 62, 90, 118 },
		new[] { 6, 26, 50, 74, 98, 122 },
		new[] { 6, 30, 54, 78, 102, 126 },
		new[] { 6, 26, 52, 78, 104, 130 },
		new[] { 6, 30, 56, 82, 108, 134 },
		new[] { 6, 34, 60, 86, 112, 138 },
		new[] { 6, 30, 58, 86, 114, 142 },
		new[] { 6, 34, 62, 90, 118, 146 },
		new[] { 6, 30, 54, 78, 102, 126, 150 },
		new[] { 6, 24, 50, 76, 102, 128, 154 },
		new[] { 6, 28, 54, 80, 106, 132, 158 },
		new[] { 6, 32, 58, 84, 110, 136, 162 },
		new[] { 6, 26, 54, 82, 110, 138, 166 },
		new[] { 6, 30, 58, 86, 114, 142, 170 }
	};

	private static bool TryGetFinderLocal(int x, int y, int originX, int originY, out int localX, out int localY)
	{
		localX = x - originX;
		localY = y - originY;
		return localX >= 0 && localX < 7 && localY >= 0 && localY < 7;
	}
}
