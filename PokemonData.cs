using System.Buffers.Binary;

namespace PCDWC4Converter;

public static class PokemonData
{
    private static readonly int[][] _blockPositions =
    {
        new[] { 0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3 },
        new[] { 1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2 },
        new[] { 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1 },
        new[] { 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 },
    };

    private static void XorCrypt(Span<byte> data)
    {
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(data[..4]);
        var initialSeed = BinaryPrimitives.ReadUInt16LittleEndian(data[6..8]);

        var rng = new LCRng(initialSeed);

        for (var i = 8; i < 236; i += 2)
        {
            if (i == 136)
            {
                rng = new LCRng(pid);
            }

            var dataBlock = data[i..];
            var value = BinaryPrimitives.ReadUInt16LittleEndian(dataBlock);
            BinaryPrimitives.WriteUInt16LittleEndian(dataBlock, (ushort)(value ^ rng.NextH()));
        }
    }

    private static void Unshuffle(Span<byte> data)
    {
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(data[..4]);
        var shiftValue = ((pid & 0x3E000) >> 0xD) % 24;

        // Offset by first block
        data = data.Slice(8, 32 * 4);

        Span<byte> originalData = stackalloc byte[32 * 4];
        data.CopyTo(originalData);
        
        for (var i = 0; i < 4; i++)
        {
            // Unshuffle the data
            originalData.Slice(32 * _blockPositions[i][shiftValue], 32).CopyTo(data);
            
            // Increment to next block
            data = data[32..];
        }
    }
    
    private static void Shuffle(Span<byte> data)
    {
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(data[..4]);
        var shiftValue = ((pid & 0x3E000) >> 0xD) % 24;

        // Offset by first block
        data = data.Slice(8, 32 * 4);

        Span<byte> originalData = stackalloc byte[32 * 4];
        data.CopyTo(originalData);
        
        for (var i = 0; i < 4; i++)
        {
            // Shuffle the data
            originalData[..32].CopyTo(data.Slice(_blockPositions[i][shiftValue], 32));
            
            // Increment to next block
            originalData = originalData[32..];
        }
    }

    public static void DecryptData(Span<byte> pkmData)
    {
        XorCrypt(pkmData);
        Unshuffle(pkmData);
    }

    public static void EncryptData(Span<byte> pkmData)
    {
        XorCrypt(pkmData);
        Shuffle(pkmData);
    }
}
