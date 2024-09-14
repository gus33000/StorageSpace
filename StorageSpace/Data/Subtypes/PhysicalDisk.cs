﻿using System.Text;

namespace StorageSpace.Data.Subtypes
{
    public class PhysicalDisk
    {
        public int PhysicalDiskNumber
        {
            get;
            private set;
        }

        public byte[] CommandSerialNumber
        {
            get;
            private set;
        }

        public Guid SPACEDBGUID
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

        public byte SDBBStatus
        {
            get;
            private set;
        }

        public byte Usage
        {
            get;
            private set;
        }

        public byte MediaType
        {
            get;
            private set;
        }

        public int DiskBlockNumber
        {
            get;
            private set;
        }

        private PhysicalDisk()
        {
        }

        public static PhysicalDisk Parse(Stream stream)
        {
            using BinaryReader reader = new(stream);

            byte dataLength = reader.ReadByte();
            byte[] PhysicalDiskNumber = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] CommandSerialNumber = reader.ReadBytes(dataLength);

            Guid SPACEDBGUID = new(reader.ReadBytes(16));

            ushort PhysicalDiskNameLength = reader.ReadUInt16();
            PhysicalDiskNameLength = (ushort)((PhysicalDiskNameLength & 0xFF00) >> 8 | (PhysicalDiskNameLength & 0xFF) << 8);

            byte[] PhysicalDiskNameBuffer = new byte[PhysicalDiskNameLength * 2];
            for (int i = 0; i < PhysicalDiskNameLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                PhysicalDiskNameBuffer[i * 2] = high;
                PhysicalDiskNameBuffer[(i * 2) + 1] = low;
            }

            string PhysicalDiskName = Encoding.Unicode.GetString(PhysicalDiskNameBuffer).Replace("\0", "");

            ushort PhysicalDiskDescriptionLength = reader.ReadUInt16();
            PhysicalDiskDescriptionLength = (ushort)((PhysicalDiskDescriptionLength & 0xFF00) >> 8 | (PhysicalDiskDescriptionLength & 0xFF) << 8);

            byte[] PhysicalDiskDescriptionBuffer = new byte[PhysicalDiskDescriptionLength * 2];
            for (int i = 0; i < PhysicalDiskDescriptionLength; i++)
            {
                byte low = reader.ReadByte();
                byte high = reader.ReadByte();

                PhysicalDiskDescriptionBuffer[i * 2] = high;
                PhysicalDiskDescriptionBuffer[(i * 2) + 1] = low;
            }

            string PhysicalDiskDescription = Encoding.Unicode.GetString(PhysicalDiskDescriptionBuffer).Replace("\0", "");

            stream.Seek(1, SeekOrigin.Current);

            byte SDBBStatus = reader.ReadByte();
            byte Usage = reader.ReadByte();
            byte MediaType = reader.ReadByte();

            dataLength = reader.ReadByte();
            byte[] DiskBlockNumber = reader.ReadBytes(dataLength);

            dataLength = reader.ReadByte();
            byte[] DataValue1 = reader.ReadBytes(dataLength);

            PhysicalDisk physicalDisk = new()
            {
                PhysicalDiskNumber = BigEndianToInt(PhysicalDiskNumber),
                CommandSerialNumber = CommandSerialNumber,
                SPACEDBGUID = SPACEDBGUID,
                Name = PhysicalDiskName,
                Description = PhysicalDiskDescription,
                SDBBStatus = SDBBStatus,
                Usage = Usage,
                MediaType = MediaType,
                DiskBlockNumber = BigEndianToInt(DiskBlockNumber)
            };

            return physicalDisk;
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
            return $"PhysicalDiskNumber: {PhysicalDiskNumber}, SPACEDBGUID: {SPACEDBGUID}, PhysicalDiskName: {Name}";
        }
    }
}
