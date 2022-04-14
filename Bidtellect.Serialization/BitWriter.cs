using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Bidtellect.Serialization
{
    /// <summary>
    /// Writes individual bits sequentially using a <c>System.IO.BinaryWriter</c>.
    /// </summary>
    public class BitWriter : IDisposable
    {
        protected BinaryWriter binaryWriter;

        protected const byte DefaultMask = 0b1000_0000;

        protected byte byteBuffer;
        protected byte bitMask;

        /// <summary>
        /// Gets the current position (in bits, starting at 0).
        /// </summary>
        public int Position { get; protected set; }

        /// <summary>
        /// Initializes a new instance of <c>BitWriter</c>.
        /// </summary>
        /// <param name="binaryWriter">
        /// The binary writer used to write the bits.
        /// </param>
        public BitWriter(BinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;

            Reset();
        }

        /// <summary>
        /// Writes a boolean value where
        /// <i>true</i> is represented as <c>1</c> and
        /// <i>false</i> is represented as <c>0</c>.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(bool value)
        {
            if (value)
            {
                byteBuffer |= bitMask;
            }

            AdvancePosition();
        }

        /// <summary>
        /// Writes a byte value.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(byte value)
        {
            WriteBase2(value, 8);
        }

        /// <summary>
        /// Writes multiple byte values from the given array.
        /// </summary>
        /// <param name="value">An array of bytes from which to get the bytes to be written.</param>
        /// <param name="offset">An offset (0-based) from which to begin writting the bytes.</param>
        /// <param name="length">The number of bytes to be written.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <c>offset</c> or <c>length</c> values are out of range.
        /// </exception>
        public void Write(byte[] value, int offset, int length)
        {
            if (offset < 0 || offset > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length + offset > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            for (var i = offset; i < length; i += 1)
            {
                Write(value[i]);
            }
        }

        /// <summary>
        /// Writes multiple byte values from the given array.
        /// </summary>
        /// <param name="value">An array of bytes from which to get the bytes to be written.</param>
        public void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        /// <summary>
        /// Writes a series of boolean values where
        /// <i>true</i> is represented as <c>1</c> and
        /// <i>false</i> is represented as <c>0</c>.
        /// </summary>
        /// <param name="values">An enumerable collection of values to write.</param>
        public void Write(IEnumerable<bool> values)
        {
            foreach (var value in values)
            {
                Write(value);
            }
        }

        /// <summary>
        /// Writes an unsigned integer using <i>base2</i> encoding.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="length">The number of bits used to encode the value.</param>
        public void WriteBase2(int value, int length)
        {
            WriteBase2((uint)value, length);
        }

        /// <summary>
        /// Writes an unsigned integer using <i>base2</i> encoding.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="length">The number of bits used to encode the value.</param>
        public void WriteBase2(uint value, int length)
        {
            WriteBase2((ulong)value, length);
        }

        /// <summary>
        /// Writes an unsigned long integer using <i>base2</i> encoding.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="length">The number of bits used to encode the value.</param>
        public void WriteBase2(long value, int length)
        {
            WriteBase2((ulong)value, length);
        }

        /// <summary>
        /// Writes an unsigned long integer using <i>base2</i> encoding.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="length">The number of bits used to encode the value.</param>
        public void WriteBase2(ulong value, int length)
        {
            var mask = 1UL << length - 1;

            while (mask > 0)
            {
                Write((value & mask) > 0);

                mask >>= 1;
            }
        }

        /// <inheritdoc cref="BitWriter.WriteFib(uint)"/>
        public void WriteFib(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            WriteFib((uint)value);
        }

        /// <summary>
        /// Writes an unsigned integer in Fibonacci encoding.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteFib(uint value)
        {
            var fib = ComputeFib(value);

            var lastBit = false;

            for (var i = 0; ; i += 1)
            {
                var bit = (fib & 1UL << i) > 0;

                Write(bit);

                if (lastBit && bit)
                {
                    break;
                }
                else
                {
                    lastBit = bit;
                }
            }
        }

        public void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            for (int i = 0; i < bytes.Length; i += 1)
            {
                Write(bytes[i]);

                // If the value string contains the NUL character, then stop.
                if (bytes[i] == 0)
                {
                    return;
                }
            }

            // Write a NUL character to denote the end of the string.
            Write(0);
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying binary writer.
        /// </summary>
        public void Flush()
        {
            binaryWriter.Write(byteBuffer);
        }

        public void Dispose()
        {
            if (bitMask != DefaultMask)
            {
                Flush();
            }
        }

        /// <summary>
        /// Resets the <c>bitMask</c> and clears the <c>byteBuffer</c>.
        /// </summary>
        protected void Reset()
        {
            bitMask = DefaultMask;
            byteBuffer = 0;
        }

        /// <summary>
        /// Moves the position to the next bit; flushing and reseting if needed.
        /// </summary>
        protected void AdvancePosition()
        {
            bitMask >>= 1;

            if (bitMask <= 0)
            {
                Flush();
                Reset();
            }

            Position += 1;
        }

        protected static ulong ComputeFib(ulong value)
        {
            var a = 0UL;
            var b = 1UL;

            var n = 0;

            while (true)
            {
                var sum = a + b;

                if (sum > value)
                {
                    break;
                }

                a = b;
                b = sum;
                n += 1;
            }

            var bitField = 1UL << n;

            while (value > 0)
            {
                ulong difference;

                n -= 1;

                if (value >= b)
                {
                    bitField |= 1UL << n;

                    value -= b;

                    difference = b - a;
                    b = a;
                    a = difference;

                    n -= 1;
                }

                difference = b - a;
                b = a;
                a = difference;
            }

            return bitField;
        }
    }
}
