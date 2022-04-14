using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Bidtellect.Serialization.Tests
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void RoundTrip()
        {
            byte[] message;

            var bytes = new byte[] { 0x55, 0x32, 0xFF };

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.Write(true);
                    bitWriter.WriteBase2(32, 8);
                    bitWriter.WriteFib(21);
                    bitWriter.Write(bytes);
                    bitWriter.WriteString("Test!\0overrun");
                }

                message = stream.ToArray();
            }

            var bitReader = new BitReader(message);

            Assert.AreEqual(true, bitReader.ReadBit());
            Assert.AreEqual(32, (int)bitReader.ReadBase2(8));
            Assert.AreEqual(21, bitReader.ReadFib());
            Assert.IsTrue(bytes.SequenceEqual(bitReader.ReadBytes(bytes.Length)));
            Assert.AreEqual("Test!", bitReader.ReadString());
        }

        [TestMethod]
        public void BitRoundTripTest()
        {
            byte[] bytes;

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.Write(true);
                    bitWriter.Write(false);
                    bitWriter.Write(true);
                }

                bytes = stream.ToArray();
            }

            var reader = new BitReader(bytes);

            Assert.AreEqual(true, reader.ReadBit());
            Assert.AreEqual(false, reader.ReadBit());
            Assert.AreEqual(true, reader.ReadBit());
        }

        [TestMethod]
        public void ByteRoundTripTest()
        {
            byte[] bytes;

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.Write(0x22);
                    bitWriter.Write(0x72);
                    bitWriter.Write(0x6C);
                }

                bytes = stream.ToArray();
            }

            var reader = new BitReader(bytes);

            Assert.AreEqual(0x22, reader.ReadByte());
            Assert.AreEqual(0x72, reader.ReadByte());
            Assert.AreEqual(0x6C, reader.ReadByte());
        }

        [TestMethod]
        public void Base2RoundTripTest()
        {
            byte[] bytes;

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.WriteBase2(5, 4);
                    bitWriter.WriteBase2(200, 8);
                    bitWriter.WriteBase2(int.MaxValue, 32);
                    bitWriter.WriteBase2(long.MaxValue, 64);
                }

                bytes = stream.ToArray();
            }

            var reader = new BitReader(bytes);

            Assert.AreEqual(5, (int)reader.ReadBase2(4));
            Assert.AreEqual(200, (int)reader.ReadBase2(8));
            Assert.AreEqual(int.MaxValue, (int)reader.ReadBase2(32));
            Assert.AreEqual(long.MaxValue, (long)reader.ReadBase2(64));
        }

        [TestMethod]
        public void FibRoundTripTest()
        {
            byte[] bytes;

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.WriteFib(int.MaxValue-1);
                    bitWriter.WriteFib(13);
                }

                bytes = stream.ToArray();
            }

            var reader = new BitReader(bytes);

            Assert.AreEqual(int.MaxValue-1, (int)reader.ReadFib());
            Assert.AreEqual(13, (int)reader.ReadFib());
        }

        [TestMethod]
        public void StringRoundTripTest()
        {
            byte[] bytes;

            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    bitWriter.WriteString("Hello world!");
                    bitWriter.WriteString("Unicode Support ✅");
                    bitWriter.WriteString("Does not write pass null.\0 Skip this part");
                }

                bytes = stream.ToArray();
            }

            var reader = new BitReader(bytes);

            Assert.AreEqual("Hello world!", reader.ReadString());
            Assert.AreEqual("Unicode Support ✅", reader.ReadString());
            Assert.AreEqual("Does not write pass null.", reader.ReadString());
        }

        protected byte[] GetBytes(Action<BitWriter> action)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                using (var binaryWriter = new System.IO.BinaryWriter(stream))
                using (var bitWriter = new BitWriter(binaryWriter))
                {
                    action.Invoke(bitWriter);
                }

                return stream.ToArray();
            }
        }
    }
}
