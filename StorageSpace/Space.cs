using StorageSpace.Data.Subtypes;

namespace StorageSpace
{
    public class Space : Stream
    {
        private readonly Stream Stream;
        private readonly long OriginalSeekPosition;
        private readonly long length;

        private readonly bool IsAncientFormat = false;

        private long blockSize => IsAncientFormat ? 0x10000000 : 0x100000;

        private readonly Dictionary<long, long> blockTable;
        private readonly List<SlabAllocation> slabAllocations = [];
        private readonly long TotalBlocks;

        private long currentPosition = 0;

        internal Space(Stream Stream, long storeIndex, Pool storageSpace, long OriginalSeekPosition, bool IsAncientFormat)
        {
            this.IsAncientFormat = IsAncientFormat;
            this.OriginalSeekPosition = OriginalSeekPosition;
            this.Stream = Stream;

            foreach (Volume volume in storageSpace.SDBBVolumes)
            {
                if (volume.VolumeNumber != storeIndex)
                {
                    continue;
                }

                TotalBlocks = volume.VolumeBlockNumber;
            }

            foreach (SlabAllocation slabAllocation in storageSpace.SDBBSlabAllocation)
            {
                if (slabAllocation.VolumeID != storeIndex)
                {
                    continue;
                }

                slabAllocations.Add(slabAllocation);
            }

            (length, blockTable) = BuildBlockTable();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        private (long, Dictionary<long, long>) BuildBlockTable()
        {
            Dictionary<long, long> blockTable = [];

            long blockSize = IsAncientFormat ? this.blockSize : 1;

            foreach (SlabAllocation slabAllocation in slabAllocations)
            {
                long virtualDiskBlockNumber = slabAllocation.VolumeBlockNumber;
                long physicalDiskBlockNumber = slabAllocation.PhysicalDiskBlockNumber;

                blockTable.Add(virtualDiskBlockNumber, physicalDiskBlockNumber);
            }

            return (TotalBlocks * blockSize, blockTable);
        }

        private long GetBlockDataIndex(long realBlockOffset)
        {
            if (blockTable.TryGetValue(realBlockOffset, out long value))
            {
                return value;
            }

            return -1; // Invalid
        }

        public override long Position
        {
            get => currentPosition;
            set
            {
                if (currentPosition < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                // Workaround for malformed MBRs
                /*if (currentPosition > Length)
                {
                    throw new EndOfStreamException();
                }*/

                currentPosition = value;
            }
        }

        public override void Flush()
        {
            // Nothing to do here
        }

        private long ImageGetStoreDataBlockOffset(long physicalDiskBlockNumber) => IsAncientFormat ? ((long)physicalDiskBlockNumber + 2) * blockSize : physicalDiskBlockNumber * blockSize + 0x2000 + 0x4000000;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // Workaround for malformed MBRs
            if (Position >= Length)
            {
                return count;
            }

            long readBytes = count;

            if (Position + readBytes > Length)
            {
                readBytes = (int)(Length - Position);
            }

            // The number of bytes that do not line up with the size of blocks (blockSize) at the beginning
            long overflowBlockStartByteCount = Position % blockSize;

            // The number of bytes that do not line up with the size of blocks (blockSize) at the end
            long overflowBlockEndByteCount = (Position + readBytes) % blockSize;

            // The position to start reading from, aligned to the size of blocks (blockSize)
            long noOverflowBlockStartByteCount = Position - overflowBlockStartByteCount;

            // The number of extra bytes to read at the start
            long extraStartBytes = blockSize - overflowBlockStartByteCount;

            // The number of extra bytes to read at the end
            long extraEndBytes = blockSize - overflowBlockEndByteCount;

            // The position to end reading from, aligned to the size of blocks (blockSize) (excluding)
            long noOverflowBlockEndByteCount = Position + readBytes + extraEndBytes;

            // The first block we have to read
            long startBlockIndex = noOverflowBlockStartByteCount / blockSize;

            // The last block we have to read (excluding)
            long endBlockIndex = noOverflowBlockEndByteCount / blockSize;

            // Go through every block one by one
            for (long currentBlock = startBlockIndex; currentBlock < endBlockIndex; currentBlock++)
            {
                bool isFirstBlock = currentBlock == startBlockIndex;
                bool isLastBlock = currentBlock == endBlockIndex - 1;

                long bytesToRead = blockSize;
                long bufferDestination = extraStartBytes + (currentBlock - startBlockIndex - 1) * blockSize;

                if (isFirstBlock)
                {
                    bytesToRead = extraStartBytes;
                    bufferDestination = 0;
                }

                if (isLastBlock)
                {
                    bytesToRead -= extraEndBytes;
                }

                long virtualBlockIndex = GetBlockDataIndex(currentBlock);

                if (virtualBlockIndex != -1)
                {
                    // The block exists
                    long physicalDiskLocation = ImageGetStoreDataBlockOffset(virtualBlockIndex);

                    if (isFirstBlock)
                    {
                        physicalDiskLocation += overflowBlockStartByteCount;
                    }

                    Stream.Seek(OriginalSeekPosition + physicalDiskLocation, SeekOrigin.Begin);
                    Stream.Read(buffer, offset + (int)bufferDestination, (int)bytesToRead);
                }
                else
                {
                    // The block does not exist in the pool, fill the area with 00s instead
                    Array.Fill<byte>(buffer, 0, offset + (int)bufferDestination, (int)bytesToRead);
                }
            }

            Position += readBytes;

            if (Position == Length)
            {
                // Workaround for malformed MBRs
                //return 0;
            }

            return (int)readBytes;
        }

        public void CopyTo(Stream DestinationStream, Action<ulong, ulong> ProgressCallBack)
        {
            long OriginalDestinationStreamPosition = DestinationStream.Position;

            ulong totalBytes = (uint)slabAllocations.Count * (ulong)blockSize * 2u;

            ProgressCallBack?.Invoke(0, totalBytes);

            ulong currentWrittenBytes = 0;

            for (int i = 0; i < slabAllocations.Count; i++)
            {
                SlabAllocation slabAllocation = slabAllocations[i];
                long virtualDiskBlockNumber = slabAllocation.VolumeBlockNumber;
                long physicalDiskBlockNumber = slabAllocation.PhysicalDiskBlockNumber;

                long virtualPosition = virtualDiskBlockNumber * blockSize;
                long physicalPosition = ImageGetStoreDataBlockOffset(physicalDiskBlockNumber);

                Stream.Seek(OriginalSeekPosition + physicalPosition, SeekOrigin.Begin);
                DestinationStream.Seek(OriginalDestinationStreamPosition + virtualPosition, SeekOrigin.Begin);

                // Block Size is a multiple of 4096
                ulong chunkSize = (ulong)blockSize;
                for (ulong j = 0; j < (ulong)blockSize; j += chunkSize)
                {
                    byte[] buffer = new byte[chunkSize];

                    Stream.Read(buffer);
                    currentWrittenBytes += chunkSize;
                    ProgressCallBack?.Invoke(currentWrittenBytes, totalBytes);

                    DestinationStream.Write(buffer);
                    currentWrittenBytes += chunkSize;
                    ProgressCallBack?.Invoke(currentWrittenBytes, totalBytes);
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        Position = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        Position += offset;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        Position = Length + offset;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(nameof(origin));
                    }
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Stream.Dispose();
        }
    }
}
