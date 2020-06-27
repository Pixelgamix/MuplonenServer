using Muplonen.DataAccess;
using Muplonen.Math;
using Muplonen.World;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public class PlayerSession : IPlayerSession
    {
        /// <inheritdoc/>
        public GodotClientConnection Connection { get; }

        /// <inheritdoc/>
        public PlayerAccount? PlayerAccount
        {
            get => _playerAccount;
            set => SetProperty(ref _playerAccount, value);
        }
        private PlayerAccount? _playerAccount;

        /// <inheritdoc/>
        public PlayerCharacter? PlayerCharacter
        {
            get => _playerCharacter;
            set => SetProperty(ref _playerCharacter, value);
        }
        private PlayerCharacter? _playerCharacter;

        /// <inheritdoc/>
        public IRoomInstance? RoomInstance
        {
            get => _roomInstance;
            set => SetProperty(ref _roomInstance, value);
        }
        private IRoomInstance? _roomInstance;

        /// <inheritdoc />
        public Vector3i Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
        private Vector3i _position;

        /// <inheritdoc/>
        public Guid SessionId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <inheritdoc/>
        public event EventHandler? SessionEnded;

        /// <summary>
        /// Creates a new <see cref="PlayerSession"/> instance.
        /// </summary>
        /// <param name="connection">The connection to the Godot client.</param>
        public PlayerSession(GodotClientConnection connection)
        {
            Connection = connection;
            Connection.ConnectionClosed += Connection_ConnectionClosed;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SessionEnded?.Invoke(this, EventArgs.Empty);
            Connection.ConnectionClosed -= Connection_ConnectionClosed;
            Connection.Dispose();
        }

        /// <summary>
        /// Trigges the <see cref="SessionEnded"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connection_ConnectionClosed(object? sender, EventArgs e)
        {
            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets theproperty to the new value, if the new value is different from the property's
        /// current value and triggers the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="currentValue">Reference to the current value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void SetProperty<T>(ref T currentValue, T newValue, [CallerMemberName] String propertyName = "")
        {
            if (!Equals(currentValue, newValue))
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
                currentValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
