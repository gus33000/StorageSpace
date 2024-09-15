using System.Text;
using System.Text.RegularExpressions;

namespace StorageSpace.Data.Subtypes
{
    public partial class Volume
    {
        public int VolumeNumber
        {
            get;
            private set;
        }

        public int CommandSerialNumber
        {
            get;
            private set;
        }

        public Guid VolumeGUID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public int VolumeBlockNumber
        {
            get;
            private set;
        }

        public byte ProvisioningType
        {
            get;
            private set;
        }

        public byte ResiliencySettingName
        {
            get;
            private set;
        }

        public byte NumberOfCopies
        {
            get;
            private set;
        }

        public byte NumberOfClusters
        {
            get;
            private set;
        }

        private Volume()
        {
        }

        public static Volume ParseV1(Stream stream)
        {
            using BinaryReader reader = new(stream, Encoding.Default, true);

            byte dataLength = reader.ReadByte();
            byte[] VolumeNumber = reader.ReadBytes(dataLength);

            Guid VolumeGUID = new(reader.ReadBytes(16));

            ushort VolumeNameLength = reader.ReadUInt16();
            VolumeNameLength = (ushort)((VolumeNameLength & 0xFF00) >> 8 | (VolumeNameLength & 0xFF) << 8);

            byte[] VolumeNameBuffer = new byte[VolumeNameLength * 2];
            for (int i = 0; i < VolumeNameLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                VolumeNameBuffer[i * 2] = high;
                VolumeNameBuffer[(i * 2) + 1] = low;
            }

            string VolumeName = Encoding.Unicode.GetString(VolumeNameBuffer).Replace("\0", "");

            ushort VolumeDescriptionLength = reader.ReadUInt16();
            VolumeDescriptionLength = (ushort)((VolumeDescriptionLength & 0xFF00) >> 8 | (VolumeDescriptionLength & 0xFF) << 8);

            byte[] VolumeDescriptionBuffer = new byte[VolumeDescriptionLength * 2];
            for (int i = 0; i < VolumeDescriptionLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                VolumeDescriptionBuffer[i * 2] = high;
                VolumeDescriptionBuffer[(i * 2) + 1] = low;
            }

            string VolumeDescription = Encoding.Unicode.GetString(VolumeDescriptionBuffer).Replace("\0", "");

            stream.Seek(3, SeekOrigin.Current);

            dataLength = reader.ReadByte();
            byte[] VolumeBlockNumber = reader.ReadBytes(dataLength);

            int ParsedVolumeBlockNumber = BigEndianToInt(VolumeBlockNumber.Take(dataLength).ToArray());

            dataLength = reader.ReadByte();
            byte[] DataValue2 = reader.ReadBytes(dataLength);

            byte ProvisioningType = reader.ReadByte();

            stream.Seek(9, SeekOrigin.Current);

            byte ResiliencySettingName = reader.ReadByte();

            dataLength = reader.ReadByte();
            byte[] DataValue3 = reader.ReadBytes(dataLength);

            stream.Seek(1, SeekOrigin.Current);

            byte NumberOfCopies = reader.ReadByte();

            stream.Seek(3, SeekOrigin.Current);

            byte NumberOfClusters = reader.ReadByte();

            // Unknown

            Volume volume = new()
            {
                VolumeNumber = BigEndianToInt(VolumeNumber),
                CommandSerialNumber = 0,
                VolumeGUID = VolumeGUID,
                Name = VolumeName,
                Description = VolumeDescription,
                VolumeBlockNumber = ParsedVolumeBlockNumber,
                ProvisioningType = ProvisioningType,
                ResiliencySettingName = ResiliencySettingName,
                NumberOfCopies = NumberOfCopies,
                NumberOfClusters = NumberOfClusters
            };

            return volume;
        }

        public static Volume ParseV2(Stream stream)
        {
            using BinaryReader reader = new(stream, Encoding.Default, true);

            byte dataLength = reader.ReadByte();
            byte[] VolumeNumber = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] CommandSerialNumber = reader.ReadBytes(dataLength);

            Guid VolumeGUID = new(reader.ReadBytes(16));

            ushort VolumeNameLength = reader.ReadUInt16();
            VolumeNameLength = (ushort)((VolumeNameLength & 0xFF00) >> 8 | (VolumeNameLength & 0xFF) << 8);

            byte[] VolumeNameBuffer = new byte[VolumeNameLength * 2];
            for (int i = 0; i < VolumeNameLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                VolumeNameBuffer[i * 2] = high;
                VolumeNameBuffer[(i * 2) + 1] = low;
            }

            string VolumeName = Encoding.Unicode.GetString(VolumeNameBuffer).Replace("\0", "");

            ushort VolumeDescriptionLength = reader.ReadUInt16();
            VolumeDescriptionLength = (ushort)((VolumeDescriptionLength & 0xFF00) >> 8 | (VolumeDescriptionLength & 0xFF) << 8);

            byte[] VolumeDescriptionBuffer = new byte[VolumeDescriptionLength * 2];
            for (int i = 0; i < VolumeDescriptionLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                VolumeDescriptionBuffer[i * 2] = high;
                VolumeDescriptionBuffer[(i * 2) + 1] = low;
            }

            string VolumeDescription = Encoding.Unicode.GetString(VolumeDescriptionBuffer).Replace("\0", "");

            stream.Seek(3, SeekOrigin.Current);

            dataLength = reader.ReadByte();
            byte[] VolumeBlockNumber = reader.ReadBytes(dataLength);

            int ParsedVolumeBlockNumber = BigEndianToInt(VolumeBlockNumber.Take(dataLength).ToArray());

            dataLength = reader.ReadByte();
            byte[] DataValue2 = reader.ReadBytes(dataLength);

            byte ProvisioningType = reader.ReadByte();

            stream.Seek(9, SeekOrigin.Current);

            byte ResiliencySettingName = reader.ReadByte();

            dataLength = reader.ReadByte();
            byte[] DataValue3 = reader.ReadBytes(dataLength);

            stream.Seek(1, SeekOrigin.Current);

            byte NumberOfCopies = reader.ReadByte();

            stream.Seek(3, SeekOrigin.Current);

            byte NumberOfClusters = reader.ReadByte();

            // Unknown

            Volume volume = new()
            {
                VolumeNumber = BigEndianToInt(VolumeNumber),
                CommandSerialNumber = BigEndianToInt(CommandSerialNumber),
                VolumeGUID = VolumeGUID,
                Name = VolumeName,
                Description = VolumeDescription,
                VolumeBlockNumber = ParsedVolumeBlockNumber,
                ProvisioningType = ProvisioningType,
                ResiliencySettingName = ResiliencySettingName,
                NumberOfCopies = NumberOfCopies,
                NumberOfClusters = NumberOfClusters
            };

            return volume;
        }

        private static bool? IsAncientPoolVolumeSpec = null;

        public static Volume Parse(Stream stream)
        {
            if (IsAncientPoolVolumeSpec.HasValue)
            {
                try
                {
                    if (IsAncientPoolVolumeSpec.Value)
                    {
                        return ParseV1(stream);
                    }
                    else
                    {
                        return ParseV2(stream);
                    }
                }
                catch
                {
                    IsAncientPoolVolumeSpec = null;
                }
            }

            long ogSeek = stream.Position;

            bool V1ParsingSuccess = false;
            bool V2ParsingSuccess = false;

            Volume? V1 = null;
            Volume? V2 = null;

            try
            {
                V1 = ParseV1(stream);
                V1ParsingSuccess = true;
            }
            catch { }

            stream.Seek(ogSeek, SeekOrigin.Begin);

            try
            {
                V2 = ParseV2(stream);
                V2ParsingSuccess = true;
            }
            catch { }

            if (V1ParsingSuccess && !V2ParsingSuccess)
            {
                IsAncientPoolVolumeSpec = true;
                return V1!;
            }

            if (V2ParsingSuccess && !V1ParsingSuccess)
            {
                IsAncientPoolVolumeSpec = false;
                return V2!;
            }

            if (!V1ParsingSuccess && !V2ParsingSuccess)
            {
                throw new InvalidDataException("Unable to parse Volume SDBB");
            }

            // Both versions work, do some extra checks

            string V1Name = V1!.Name;
            string V2Name = V2!.Name;

            Regex regex = CommonDiskAsciiCharacters();

            int V1NameMatchCount = regex.Matches(V1Name).Count;
            int V2NameMatchCount = regex.Matches(V2Name).Count;

            if (V1NameMatchCount < V2NameMatchCount)
            {
                // Do not set flag as we are frankly unsure here.
                return V1;
            }
            else
            {
                // Do not set flag as we are frankly unsure here.
                return V2;
            }
        }

        private static int BigEndianToInt(byte[] buf)
        {
            int val = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                val *= 0x100;
                val += buf[i];
            }
            return val;
        }

        public override string ToString()
        {
            return $"VolumeNumber: {VolumeNumber}, VolumeGUID: {VolumeGUID}, VolumeName: {Name}";
        }

        [GeneratedRegex("[a-zA-Z0-9_\\.]*")]
        private static partial Regex CommonDiskAsciiCharacters();
    }
}
