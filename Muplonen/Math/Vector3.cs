namespace Muplonen.Math
{
    /// <summary>
    /// Integer vector.
    /// </summary>
    public struct Vector3i
    {
        /// <summary>
        /// Vector with all parts set to 0.
        /// </summary>
        public static readonly Vector3i Zero = new Vector3i();

        /// <summary>
        /// X part of the vector.
        /// </summary>
        public short X;

        /// <summary>
        /// Y part of the vector.
        /// </summary>
        public short Y;

        /// <summary>
        /// Z part of the vector.
        /// </summary>
        public short Z;

        /// <summary>
        /// Creates a new <see cref="Vector3i"/> instance with the specified parts.
        /// </summary>
        /// <param name="x">X-Part.</param>
        /// <param name="y">Y-Part.</param>
        /// <param name="z">Z-Part.</param>
        public Vector3i(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
