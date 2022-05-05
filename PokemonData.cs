using System.Buffers.Binary;

namespace PCDWC4Converter;

public static class PokemonData
{
    private static readonly byte[] _blockPositions =
    {
        0, 1, 2, 3,
        0, 1, 3, 2,
        0, 2, 1, 3,
        0, 3, 1, 2,
        0, 2, 3, 1,
        0, 3, 2, 1,
        1, 0, 2, 3,
        1, 0, 3, 2,
        2, 0, 1, 3,
        3, 0, 1, 2,
        2, 0, 3, 1,
        3, 0, 2, 1,
        1, 2, 0, 3,
        1, 3, 0, 2,
        2, 1, 0, 3,
        3, 1, 0, 2,
        2, 3, 0, 1,
        3, 2, 0, 1,
        1, 2, 3, 0,
        1, 3, 2, 0,
        2, 1, 3, 0,
        3, 1, 2, 0,
        2, 3, 1, 0,
        3, 2, 1, 0
    };
    
    private static readonly byte[] _blockPositionInvert =
    {
        0, 1, 2, 4, 3, 5, 6, 7, 12, 18, 13, 19, 8, 10, 14, 20, 16, 22, 9, 11, 15, 21, 17, 23
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

    private static void Shuffle(Span<byte> data, uint shiftValue)
    {
        // Offset by first block
        data = data.Slice(8, 32 * 4);

        Span<byte> originalData = stackalloc byte[32 * 4];
        data.CopyTo(originalData);
        
        for (var i = 0; i < 4; i++)
        {
            // Unshuffle the data
            originalData.Slice(32 * _blockPositions[i + shiftValue * 4], 32).CopyTo(data);
            
            // Increment to next block
            data = data[32..];
        }
    }

    public static void DecryptData(Span<byte> pkmData)
    {
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(pkmData[..4]);
        var shiftValue = ((pid & 0x3E000) >> 0xD) % 24;
        
        XorCrypt(pkmData);
        Shuffle(pkmData, shiftValue);
    }

    public static void EncryptData(Span<byte> pkmData)
    {
        var pid = BinaryPrimitives.ReadUInt32LittleEndian(pkmData[..4]);
        var shiftValue = ((pid & 0x3E000) >> 0xD) % 24;
        
        Shuffle(pkmData, _blockPositionInvert[shiftValue]);
        XorCrypt(pkmData);
    }
}
