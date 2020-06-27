using Muplonen.Math;

namespace Muplonen.World
{
    /// <summary>
    /// Entity interface.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// The entity's position.
        /// </summary>
        Vector3i Position { get; set; }
    }
}
