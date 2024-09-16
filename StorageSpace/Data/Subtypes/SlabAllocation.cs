namespace StorageSpace.Data.Subtypes
{
    public class SlabAllocation
    {
        public long VolumeID
        {
            get;
            private set;
        }

        public long VolumeBlockNumber
        {
            get;
            private set;
        }

        public long ParitySequenceNumber
        {
            get;
            private set;
        }

        public long MirrorSequenceNumber
        {
            get;
            private set;
        }

        public long PhysicalDiskID
        {
            get;
            private set;
        }

        public long PhysicalDiskBlockNumber
        {
            get;
            private set;
        }

        private static long BigEndianToInt(byte[] buf)
        {
            long val = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                val *= 0x100;
                val += buf[i];
            }
            return val;
        }
        private SlabAllocation()
        {
        }

        public static SlabAllocation Parse(Stream stream)
        {
            using BinaryReader reader = new(stream);

            byte dataLength = reader.ReadByte();
            byte[] DataValue1 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] DataValue2 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] DataValue3 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] DataValue4 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] DataValue5 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            long VolumeID = BigEndianToInt(reader.ReadBytes(dataLength));

            dataLength = reader.ReadByte();
            long VolumeBlockNumber = BigEndianToInt(reader.ReadBytes(dataLength));

            dataLength = reader.ReadByte();
            long ParitySequenceNumber = BigEndianToInt(reader.ReadBytes(dataLength));

            dataLength = reader.ReadByte();
            long MirrorSequenceNumber = BigEndianToInt(reader.ReadBytes(dataLength));

            dataLength = reader.ReadByte();
            byte[] DataValue6 = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            long PhysicalDiskID = BigEndianToInt(reader.ReadBytes(dataLength));

            dataLength = reader.ReadByte();
            long PhysicalDiskBlockNumber = BigEndianToInt(reader.ReadBytes(dataLength));

            SlabAllocation slabAllocation = new()
            {
                VolumeID = VolumeID,
                VolumeBlockNumber = VolumeBlockNumber,
                ParitySequenceNumber = ParitySequenceNumber,
                MirrorSequenceNumber = MirrorSequenceNumber,
                PhysicalDiskID = PhysicalDiskID,
                PhysicalDiskBlockNumber = PhysicalDiskBlockNumber
            };

            return slabAllocation;
        }

        public override string ToString()
        {
            return $"VolumeID: {VolumeID}, VolumeBlockNumber: {VolumeBlockNumber}, PhysicalDiskBlockNumber: {PhysicalDiskBlockNumber}";
        }
    }
}
