using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    /// <summary>
    /// Implementation is NOT synchronized. We rely on SessionManager for proper locking.
    /// </summary>
    public class ReplayDetector
    {
        private class ReplayToken
        {
            public int SessionId;
            public int SequenceNumber;
            public DateTime UseTime;

            public ReplayToken(int sessionId, int sequenceNumber)
            {
                SessionId = sessionId;
                SequenceNumber = sequenceNumber;
                UseTime = DateTime.Now;
            }

            public override bool Equals(object obj)
            {
                ReplayToken that = obj as ReplayToken;
                if (that == null)
                    return false;
                return this.SessionId == that.SessionId
                    && this.SequenceNumber == that.SequenceNumber;
            }

            public override int GetHashCode()
            {
                return SessionId.GetHashCode() + SequenceNumber.GetHashCode();
            }
        }

        private Queue<ReplayToken> _queue;
        private HashSet<ReplayToken> _set;

        public ReplayDetector()
        {
            _queue = new Queue<ReplayToken>();
            _set = new HashSet<ReplayToken>();
        }

        public void CheckAndStore(int sessionId, int sequenceNumber)
        {
            ReplayToken token = new ReplayToken(sessionId, sequenceNumber);
            if (_set.Contains(token))
                throw new AuthException("Session message sequence replay detected.");
            _set.Add(token);
            _queue.Enqueue(token);
            CleanOldTokens();
        }

        private void CleanOldTokens()
        {
            DateTime staleBarrier = DateTime.Now.AddSeconds(-Settings.ReplayDetectionWindowSeconds);
            while (_queue.Count > 0 && _queue.Peek().UseTime < staleBarrier)
            {
                ReplayToken token = _queue.Dequeue();
                _set.Remove(token);
            }
        }
    }
}
