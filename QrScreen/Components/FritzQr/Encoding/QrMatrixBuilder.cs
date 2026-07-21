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
				roles[x, y] = ClassifyRole(x, y, size);
			}
		}

		return new QrMatrix(size, version, eccLevel, modules, roles);
	}

	private static ModuleRole ClassifyRole(int x, int y, int size)
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

		return ModuleRole.Data;
	}

	private static bool TryGetFinderLocal(int x, int y, int originX, int originY, out int localX, out int localY)
	{
		localX = x - originX;
		localY = y - originY;
		return localX >= 0 && localX < 7 && localY >= 0 && localY < 7;
	}
}
