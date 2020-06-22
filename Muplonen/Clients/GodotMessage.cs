using System;
using System.IO;
using System.Text;

namespace Muplonen.Clients
{
    /// <summary>
    /// A message received from the Godot client or for sending to the Godot client.
    /// </summary>
    public sealed class GodotMessage : IDisposable
    {
        /// <summary>
        /// Gets or sets the current position for reading in the <see cref="Buffer"/>.
        /// </summary>
        public int ReadPosition
        {
            get => (int)_binaryReader.BaseStream.Position;
            set => _binaryReader.BaseStream.Position = value;
        }

        /// <summary>
        /// Gets or sets the current position for writing in the <see cref="Buffer"/>.
        /// </summary>
        public int WritePosition
        {
            get => (int)_binaryWriter.BaseStream.Position;
            set => _binaryWriter.BaseStream.Position = value;
        }

        /// <summary>
        /// The buffer.
        /// </summary>
        public byte[] Buffer { get; } = new byte[4 * 1024];

        private readonly BinaryReader _binaryReader;
        private readonly BinaryWriter _binaryWriter;
        private static readonly Encoding _encoding = Encoding.ASCII;

        /// <summary>
        /// Creates a new <see cref="GodotMessage"/>.
        /// </summary>
        public GodotMessage()
        {
            var memoryStream = new MemoryStream(Buffer);
            _binaryReader = new BinaryReader(memoryStream, _encoding);
            _binaryWriter = new BinaryWriter(memoryStream, _encoding);
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        /// <returns>The read byte.</returns>
        public byte ReadByte() => _binaryReader.ReadByte();

        /// <summary>
        /// Reads an unsigned 16 bit integer.
        /// </summary>
        /// <returns>The read integer.</returns>
        public ushort ReadUInt16() => _binaryReader.ReadUInt16();

        /// <summary>
        /// Reads a signed 32 bit integer.
        /// </summary>
        /// <returns>The read integer.</returns>
        public int ReadInt32() => _binaryReader.ReadInt32();

        /// <summary>
        /// Reads a string.
        /// </summary>
        /// <returns>The read string.</returns>
        public string ReadString()
        {
            var length = _binaryReader.ReadInt32();
            var chars = _binaryReader.ReadChars(length);
            return new string(chars);
        }

        /// <summary>
        /// Resets the position for reading and writing to 0.
        /// </summary>
        public void ResetPosition()
        {
            _binaryReader.BaseStream.Position = 0;
            _binaryWriter.BaseStream.Position = 0;
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="number">The number to write.</param>
        public void WriteByte(byte number) => _binaryWriter.Write(number);

        /// <summary>
        /// Writes an unsigned 16 bit integer.
        /// </summary>
        /// <param name="number">The number to write.</param>
        public void WriteUInt16(ushort number) => _binaryWriter.Write(number);

        /// <summary>
        /// Writes an signed 32 bit integer.
        /// </summary>
        /// <param name="number">The number to write.</param>
        public void WriteInt32(int number) => _binaryWriter.Write(number);

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="text">The string to write.</param>
        public void WriteString(string text)
        {
            _binaryWriter.Write(text.Length);
            _binaryWriter.Write(_encoding.GetBytes(text));
        }

        /// <summary>
        /// Frees ressources used by the instance.
        /// </summary>
        public void Dispose()
        {
            _binaryReader.Dispose();
            _binaryWriter.Dispose();
        }
    }
}
