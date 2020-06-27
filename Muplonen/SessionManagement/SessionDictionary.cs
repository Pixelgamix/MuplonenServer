using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// Dictionary of active player sessions.
    /// </summary>
    public class SessionDictionary : IDisposable
    {
        private ConcurrentDictionary<Guid, IPlayerSession> _session2PlayerSession = new ConcurrentDictionary<Guid, IPlayerSession>();
        private ConcurrentDictionary<Guid, IPlayerSession> _account2PlayerSession = new ConcurrentDictionary<Guid, IPlayerSession>();
        private ConcurrentDictionary<Guid, IPlayerSession> _character2PlayerSession = new ConcurrentDictionary<Guid, IPlayerSession>();

        /// <summary>
        /// All sessions including unauthenticated, logged in and in-game sessions.
        /// </summary>
        public IEnumerable<IPlayerSession> AllSessions { get => _session2PlayerSession.Values; }

        /// <summary>
        /// Unauthenticated sessions.
        /// </summary>
        public IEnumerable<IPlayerSession> Unauthenticated { get => _session2PlayerSession.Values.Where(session => session.PlayerAccount == null); }

        /// <summary>
        /// Authenticated (=logged in) sessions.
        /// </summary>
        public IEnumerable<IPlayerSession> Authenticated { get => _account2PlayerSession.Values; }

        /// <summary>
        /// In-game (=character selected) sessions.
        /// </summary>
        public IEnumerable<IPlayerSession> InGame { get => _character2PlayerSession.Values; }

        /// <summary>
        /// Event that is triggered when a session got added.
        /// </summary>
        public event EventHandler<IPlayerSession>? SessionAdded;

        /// <summary>
        /// Event that is triggered when a session got removed.
        /// </summary>
        public event EventHandler<IPlayerSession>? SessionRemoved;

        /// <summary>
        /// Tries to fetch the <see cref="IPlayerSession"/> with the specified session id.
        /// </summary>
        /// <param name="characterId">The session id.</param>
        /// <param name="playerSession">The session or null.</param>
        /// <returns>true if the session was returned.</returns>
        public bool TryGetBySessionId(Guid sessionId, out IPlayerSession? playerSession) => _session2PlayerSession.TryGetValue(sessionId, out playerSession);

        /// <summary>
        /// Tries to fetch the <see cref="IPlayerSession"/> with the specified account id.
        /// </summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="playerSession">The session or null.</param>
        /// <returns>true if the session was returned.</returns>
        public bool TryGetByAccountId(Guid accountId, out IPlayerSession? playerSession) => _account2PlayerSession.TryGetValue(accountId, out playerSession);

        /// <summary>
        /// Tries to fetch the <see cref="IPlayerSession"/> with the specified character id.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <param name="playerSession">The session or null.</param>
        /// <returns>true if the session was returned.</returns>
        public bool TryGetByCharacterId(Guid characterId, out IPlayerSession? playerSession) => _character2PlayerSession.TryGetValue(characterId, out playerSession);

        /// <summary>
        /// Adds a session to the dictionary.
        /// </summary>
        /// <param name="playerSession">The session to add.</param>
        /// <returns>true if the session was added. Otherwise false.</returns>
        public bool TryAddSession(IPlayerSession playerSession)
        {
            if (!_session2PlayerSession.TryAdd(playerSession.SessionId, playerSession))
                return false;

            if (playerSession.PlayerAccount != null)
                _account2PlayerSession.TryAdd(playerSession.PlayerAccount.Id, playerSession);

            if (playerSession.PlayerCharacter != null)
                _character2PlayerSession.TryAdd(playerSession.PlayerCharacter.Id, playerSession);

            playerSession.PropertyChanged += PlayerSession_PropertyChanged;
            playerSession.PropertyChanging += PlayerSession_PropertyChanging;

            SessionAdded?.Invoke(this, playerSession);

            return true;
        }

        /// <summary>
        /// Removes a session from the dictionary.
        /// </summary>
        /// <param name="playerSession">The session to remove.</param>
        public void RemoveSession(IPlayerSession playerSession)
        {
            if (playerSession.PlayerCharacter != null)
                _character2PlayerSession.TryRemove(playerSession.PlayerCharacter.Id, out _);

            if (playerSession.PlayerAccount != null)
                _account2PlayerSession.TryRemove(playerSession.PlayerAccount.Id, out _);

            _session2PlayerSession.TryRemove(playerSession.SessionId, out _);

            playerSession.PropertyChanged -= PlayerSession_PropertyChanged;
            playerSession.PropertyChanging -= PlayerSession_PropertyChanging;

            SessionRemoved?.Invoke(this, playerSession);
        }

        /// <summary>
        /// Removes all sessions from the dictionary.
        /// </summary>
        public void Clear()
        {
            foreach (var session in _session2PlayerSession.Values)
                RemoveSession(session);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Updates the player session's registrations in the dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerSession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var session = (IPlayerSession)sender;

            if (e.PropertyName == nameof(session.PlayerAccount) && session.PlayerAccount != null)
                _account2PlayerSession.TryAdd(session.PlayerAccount.Id, session);

            else if (e.PropertyName == nameof(session.PlayerCharacter) && session.PlayerCharacter != null)
                _character2PlayerSession.TryAdd(session.PlayerCharacter.Id, session);
        }

        /// <summary>
        /// Updates the player session's registrations in the dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerSession_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var session = (IPlayerSession)sender;

            if (e.PropertyName == nameof(session.PlayerAccount) && session.PlayerAccount != null)
                _account2PlayerSession.TryRemove(session.PlayerAccount.Id, out _);

            else if (e.PropertyName == nameof(session.PlayerCharacter) && session.PlayerCharacter != null)
                _character2PlayerSession.TryRemove(session.PlayerCharacter.Id, out _);
        }
    }
}
