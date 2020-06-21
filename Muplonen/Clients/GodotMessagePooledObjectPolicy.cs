using Microsoft.Extensions.ObjectPool;

namespace Muplonen.Clients
{
    /// <summary>
    /// Creates and recycles <see cref="GodotMessage"/> instances for use in a pool.
    /// </summary>
    public sealed class GodotMessagePooledObjectPolicy : IPooledObjectPolicy<GodotMessage>
    {
        /// <summary>
        /// Creates a new <see cref="GodotMessage"/> instance.
        /// </summary>
        /// <returns>The created instance.</returns>
        public GodotMessage Create()
        {
            return new GodotMessage();
        }

        /// <summary>
        /// Resets a <see cref="GodotMessage"/> intance.
        /// </summary>
        /// <param name="obj">The instance for resetting.</param>
        /// <returns>Always true.</returns>
        public bool Return(GodotMessage obj)
        {
            obj.ResetPosition();
            return true;
        }
    }
}
