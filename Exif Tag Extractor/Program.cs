using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Exif_Tag_Extractor
{
    /// <summary>
    /// Equivalent to two LONGs (8 bytes total). Value returns the cotient.
    /// </summary>
    struct Rational
    {
        public long Numerator { get; set; }
        public long Denominator { get; set; }
        public double Value { get { return Numerator / Denominator; } }
    }

    /// <summary>
    /// Byte ordering. II is equivalent to Little Endian, MM to BigEndian. 
    /// This is used to file bytes read only. After file content is read, the sorts the bytes, if needed, and works internally with Little Endiand ordering.
    /// </summary>
    internal enum Endianness
    {
        LittleEndian = 73 * 256 + 73,
        BigEndian = 77 * 256 + 77,
        II = LittleEndian,
        MM = BigEndian,
    }

    /// <summary>
    /// The identification for the internal types defined for Tiff tagas values.
    /// </summary>
    public enum ValueElementType
    {
        BYTE = 1,       // An 8-bit unsigned integer.
        ASCII = 2,      // An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL.
        SHORT = 3,      // A 16-bit (2-byte) unsigned integer.
        LONG = 4,       // A 32-bit (4-byte) unsigned integer.
        RATIONAL = 5,   // Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator.
        UNDEFINED = 7,  // An 8-bit byte that can take any value depending on the field definition.
        SLONG = 9,      // A 32-bit (4-byte) signed integer (2's complement notation).
        SRATIONAL = 10, // Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator.
    }

    /// <summary>
    /// Represents the basic type of a tag
    /// </summary>
    public class TagValueType
    {
        public ValueElementType Id { get; set; }
        public int BytesLength { get; set; }
        public Type DestinationType { get; set; }

        public TagValueType(ValueElementType type, int length, Type destinationType)
        {
            Id = type;
            BytesLength = length;
            DestinationType = destinationType;
        }
    }

    /// <summary>
    /// Represents a Tiff tag.
    /// </summary>
    public class Tag
    {
        public Tag()
        {
            TagValueBytes = new List<byte>();
        }

        /// <summary>
        /// Tiff Tag Id
        /// </summary>
        public TagTypeCode Id { get; set; }
        /// <summary>
        /// Type of the value elements
        /// </summary>
        public ValueElementType ValueElementsType { get; set; }
        /// <summary>
        /// Number of the element in the tag
        /// </summary>
        public int ValueElementsCount { get; set; }
        /// <summary>
        /// The offset to the value. If the number of elements is less or equal to four bytes, stores the value of the tag
        /// </summary>
        public int ValueOffset { get; set; }
        /// <summary>
        /// Indicates if the ValueOffset stores de value of the tag or a pointer to the value position
        /// </summary>
        public bool IsValueOffsetPointer { get; set; }
        /// <summary>
        /// The value extracted for the tag
        /// </summary>
        public List<byte> TagValueBytes { get; set; }
    }

    internal class TagTypesDictionary : Dictionary<ValueElementType, TagValueType>
    {
        internal TagTypesDictionary()
            : base()
        {
            this.Add(ValueElementType.BYTE, new TagValueType(ValueElementType.BYTE, 1, typeof(byte)));
            this.Add(ValueElementType.ASCII, new TagValueType(ValueElementType.ASCII, 1, typeof(string)));
            this.Add(ValueElementType.SHORT, new TagValueType(ValueElementType.SHORT, 2, typeof(short)));
            this.Add(ValueElementType.LONG, new TagValueType(ValueElementType.LONG, 4, typeof(int)));
            this.Add(ValueElementType.RATIONAL, new TagValueType(ValueElementType.RATIONAL, 8, typeof(Rational)));
            this.Add(ValueElementType.UNDEFINED, new TagValueType(ValueElementType.UNDEFINED, 1, typeof(byte)));
            this.Add(ValueElementType.SLONG, new TagValueType(ValueElementType.SLONG, 4, typeof(int)));
            this.Add(ValueElementType.SRATIONAL, new TagValueType(ValueElementType.SRATIONAL, 8, typeof(int)));
        }
    }

    enum EndiannesCheckBytes
    {
        littleEndian = 42,
        bigEndian = 10752
    }

    class TiffHeader
    {
        static public byte[] HeaderMark
        {
            get { return new byte[6] { 69, 120, 105, 102, 0, 0 }; }
        }

        public Endianness ByteOrder { get; set; }
        public int Index { get; set; }
        public int IfdOffset { get; set; }
        public int TagsNumber { get; set; }
    }

    class TiffTagCollection : Dictionary<TagTypeCode, Tag>
    {
        public TiffHeader Header { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string dir = @"C:\TEMP\Copia de TEMP\";
            FileInfo fi = new FileInfo(dir + "DSCN3462.JPG");

            List<byte> bytes = GetFileBytes(fi, 0, 2048);

            TiffHeader header = new TiffHeader();

            int tiffHeaderIndex;
            int tiffFileMarkIndex = LocateExifFileMarkIndex(bytes, out tiffHeaderIndex);

            header.Index = tiffHeaderIndex;
            header.ByteOrder = GetEndiannes(bytes, header.Index);

            bool endiannessChecked = CheckEndianness(bytes, header.ByteOrder, header.Index);

            header.IfdOffset = GetFirstTagOffset(bytes, header.ByteOrder, header.Index);
            header.TagsNumber = GetTagsNumber(bytes, header.ByteOrder, header.Index, header.IfdOffset);

            int firstTagIdx = GetFirstTagIdx(header.Index, header.IfdOffset);

            byte[][] tagsBytes = ExtractTagsBytes(bytes, header.TagsNumber, firstTagIdx);

            TiffTagCollection tags = ParseTagsBytes(header.ByteOrder, tagsBytes);

            LoadTagValues(ref tags, tiffHeaderIndex, bytes);

            foreach (Tag tag in tags.Values)
            {
                Console.WriteLine("{0}: {1}", tag.Id, Encoding.ASCII.GetString(tag.TagValueBytes.ToArray()));
            }
        }

        private static void LoadTagValues(ref TiffTagCollection tags, int headerIndex, List<byte> bytes)
        {
            foreach (Tag tag in tags.Values)
            {
                if (!tag.IsValueOffsetPointer)
                {
                    continue;
                }
                Tag tagReference = tag;

                LoadTagValue(ref tagReference, headerIndex, bytes);
            }
        }

        private static void LoadTagValue(ref Tag tag, int headerIndex, List<byte> bytes)
        {
            int position = headerIndex + tag.ValueOffset;
            int count = new TagTypesDictionary()[tag.ValueElementsType].BytesLength * tag.ValueElementsCount;

            tag.TagValueBytes.AddRange(ChopByteArray(bytes.ToArray(), position, count));
        }

        private static TiffTagCollection ParseTagsBytes(Endianness endianness, byte[][] tagsBytes)
        {
            TiffTagCollection result = new TiffTagCollection();
            Tag tag;

            foreach (byte[] bytes in tagsBytes)
            {
                tag = ParseTagBytes(endianness, bytes);
                result.Add(tag.Id, tag);
            }

            return result;
        }

        private static Tag ParseTagBytes(Endianness e, byte[] bytes)
        {
            byte[] tags = SortTagBytes(e, bytes);

            Tag tag = new Tag();
            tag.Id = GetValue<TagTypeCode>(ChopByteArray(bytes, 0, 2));
            tag.ValueElementsType = GetValue<ValueElementType>(ChopByteArray(bytes, 2, 2));
            tag.ValueElementsCount = GetValue<int>(ChopByteArray(bytes, 4, 4));

            if (tag.ValueElementsCount * new TagTypesDictionary()[tag.ValueElementsType].BytesLength > 4)
            {
                tag.IsValueOffsetPointer = true;
                tag.ValueOffset = GetValue<int>(ChopByteArray(bytes, 8, 4));
            }
            else
            {
                tag.IsValueOffsetPointer = false;
                tag.TagValueBytes = new List<byte>(ChopByteArray(bytes, 8, 4));
            }

            return tag;
        }

        private static byte[] SortTagBytes(Endianness e, byte[] bytes)
        {
            if (e == Endianness.BigEndian)
            {
                byte[] result = new byte[12];
                ShiftBytes(ref result, bytes, 0, 2);
                ShiftBytes(ref result, bytes, 2, 2);
                ShiftBytes(ref result, bytes, 4, 4);
                ShiftBytes(ref result, bytes, 8, 4);
                return result;
            }
            else
            {
                return bytes;
            }
        }

        private static void ShiftBytes(ref byte[] result, byte[] bytes, int start, int count)
        {
            byte[] temp = (byte[])ChopByteArray(bytes, start, count).Reverse();
            temp.CopyTo(result, start);
        }

        private static byte[][] ExtractTagsBytes(List<byte> bytes, int tagsNumber, int firstTagIdx)
        {
            byte[][] result = new byte[tagsNumber][];
            byte[] tagBytes;

            for (int i = 0; i < tagsNumber; ++i)
            {
                tagBytes = new byte[12];
                bytes.CopyTo(firstTagIdx + i * 12, tagBytes, 0, 12);
                result[i] = tagBytes;
            }

            return result;
        }

        private static int GetFirstTagIdx(int tiffHeaderIdx, int firstTagOffset)
        {
            return tiffHeaderIdx + firstTagOffset + 8 + 2;
        }

        private static int GetTagsNumber(List<byte> bytes, Endianness endianness, int tiffHeaderIdx, int firstTagOffset)
        {

            byte[] tagsNumberBytes = ChopByteArray(bytes.ToArray(), tiffHeaderIdx + firstTagOffset + 8, 2);
            if (endianness == Endianness.BigEndian)
            {
                tagsNumberBytes.Reverse();
            }

            int result = BitConverter.ToInt16(tagsNumberBytes, 0);
            return result;
        }

        private static int GetFirstTagOffset(List<byte> bytes, Endianness endianness, int tiffHeaderIdx)
        {
            byte[] tagsNumberBytes = ChopByteArray(bytes.ToArray(), tiffHeaderIdx + 6, 2);
            if (endianness == Endianness.BigEndian)
            {
                tagsNumberBytes.Reverse();
            }

            int result = BitConverter.ToInt16(tagsNumberBytes, 0);
            return result;
        }

        private static bool CheckEndianness(List<byte> bytes, Endianness endianness, int tiffHeaderIdx)
        {
            byte[] checkBytes = ChopByteArray(bytes.ToArray(), tiffHeaderIdx + 2, 2);

            if (endianness == Endianness.LittleEndian)
            {
                if (EndiannesCheckBytes.littleEndian == (EndiannesCheckBytes)BitConverter.ToInt16(checkBytes, 0))
                {
                    return true;
                }
            }
            else if (endianness == Endianness.BigEndian)
            {
                if (EndiannesCheckBytes.bigEndian == (EndiannesCheckBytes)BitConverter.ToInt16(checkBytes, 0))
                {
                    return true;
                }
            }
            return false;
        }

        private static void ExtractFilesHeader(string dir)
        {
            byte[] test = new byte[4] { 34, 1, 0, 0 };
            short conv = BitConverter.ToInt16(test, 0);

            FileInfo[] fis = new DirectoryInfo(dir).GetFiles("*.jpg");

            foreach (FileInfo fi in fis)
            {
                ExtractFileHeader(dir, fi);
            }
        }

        private static int LocateExifFileMarkIndex(List<byte> lb, out int tiffHeaderStartIndex)
        {
            for (int i = 0; i < lb.Count - 6; ++i)
            {
                if (lb[i] == TiffHeader.HeaderMark[0] &&
                    lb[i + 1] == TiffHeader.HeaderMark[1] &&
                    lb[i + 2] == TiffHeader.HeaderMark[2] &&
                    lb[i + 3] == TiffHeader.HeaderMark[3] &&
                    lb[i + 4] == TiffHeader.HeaderMark[4] &&
                    lb[i + 5] == TiffHeader.HeaderMark[5])
                {
                    tiffHeaderStartIndex = i + 6;
                    return i;
                }
                ++i;
            }
            tiffHeaderStartIndex = -1;
            return -1;
        }

        private static void ExtractFileHeader(string dir, FileInfo fi)
        {
            List<byte> lb = GetFileBytes(fi, 0, 2048);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (byte b in lb)
            {
                sb.AppendLine(string.Format("{2} {0} ({1})", UTF8Encoding.UTF8.GetString(new byte[1] { b }), b, i++));
            }

            File.WriteAllText(dir + fi.Name + ".txt", sb.ToString());
        }

        private static List<byte> GetFileBytes(FileInfo fi, long startIndex, long bytes)
        {
            byte[] ba = new byte[bytes];
            using (FileStream fs = fi.Open(FileMode.Open))
            {
                fs.Read(ba, 0, ba.Length);
            }
            List<byte> lb = new List<byte>();
            lb.AddRange(ba);
            return lb;
        }

        private static Endianness GetEndiannes(List<byte> bytes, int tiffHeaderIdx)
        {
            byte[] endiannessBytes = ChopByteArray(bytes.ToArray(), tiffHeaderIdx, 2);

            int endiannessValue = (int)BitConverter.ToInt16(endiannessBytes, 0);

            return (Endianness)endiannessValue;
        }

        private static byte[] ChopByteArray(byte[] array, int position, int count)
        {
            if (array.Length < position + count)
            {
                throw new IndexOutOfRangeException("The number of elements exeeds the total elements of the array");
            }

            byte[] result = new byte[count];

            for (int i = 0; i < count; ++i)
            {
                result[i] = array[position + i];
            }

            return result;
        }

        private static T GetValue<T>(byte[] array)
        {
            object result = new object();
            string typeName = typeof(T).ToString().ToUpper().Substring(typeof(T).ToString().LastIndexOf('.') + 1);
            switch (typeName)
            {
                case "STRING":
                    result = GetStringValue(array);
                    break;
                case "INT16":
                    result = GetShortValue(array);
                    break;
                case "INT32":
                case "VALUEELEMENTTYPE":
                case "TAGTYPECODE":
                    result = GetIntValue(array);
                    break;
                case "TIFFTAGVALUETYPE":
                    result = GetTagValueType(array);
                    break;
                case "RATIONAL":
                    result = GetRationalValue(array);
                    break;
                default:
                    throw new InvalidCastException("Cast not supported by the library.");
                    break;
            };
            return (T)result;
        }

        private static string GetStringValue(byte[] array) //where T : TiffTagValueType
        {
            return Encoding.ASCII.GetString(array);
        }

        private static short GetShortValue(byte[] array)
        {
            return BitConverter.ToInt16(array, 0);
        }

        private static int GetIntValue(byte[] array)
        {
            return BitConverter.ToInt16(array, 0);
        }

        private static Rational GetRationalValue(byte[] array)
        {
            Rational result = new Rational();

            result.Numerator = BitConverter.ToInt32(array, 0);
            result.Denominator = BitConverter.ToInt32(array, 0);

            return result;
        }

        private static ValueElementType GetTagValueType(byte[] array)
        {
            return (ValueElementType)BitConverter.ToInt16(ChopByteArray(array, 0, 2), 0);
        }
    }

    public enum TagTypeCode
    {
        ImageWidth = 256,
        ImageLength = 257,
        BitsPerSample = 258,
        Compression = 259,
        PhotometricInterpretation = 262,
        Orientation = 274,
        SamplesPerPixel = 277,
        PlanarConfiguration = 284,
        YCbCrSubSampling = 530,
        YCbCrPositioning = 531,
        XResolution = 282,
        YResolution = 283,
        ResolutionUnit = 296,
        StripOffsets = 273,
        RowsPerStrip = 278,
        StripByteCounts = 279,
        JPEGInterchangeFormat = 513,
        JPEGInterchangeFormatLength = 514,
        TransferFunction = 301,
        WhitePoint = 318,
        PrimaryChromaticities = 319,
        YCbCrCoefficients = 529,
        ReferenceBlackWhite = 532,
        DateTime = 306,
        ImageDescription = 270,
        Make = 271,
        Model = 272,
        Software = 305,
        Artist = 315,
        Copyright = 33432,
    }
}
