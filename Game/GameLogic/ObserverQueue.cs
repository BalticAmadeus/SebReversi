using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    public class ObserverQueue
    {
        public Observer Observer { get; private set; }
        public bool IsLive { get; private set; }

        public int Count
        {
            get
            {
                if (_queue != null)
                    return _queue.Count;
                else
                    return 0;
            }
        }

        private Queue<ObservedTurnInfo> _queue;

        public ObserverQueue(Observer observer)
        {
            this.Observer = observer;
            IsLive = true;
            _queue = new Queue<ObservedTurnInfo>(Settings.MaxObserverQueue);
        }

        public bool Push(ObservedTurnInfo turnInfo)
        {
            if (!IsLive)
                return false;
            if (_queue.Count >= Settings.MaxObserverQueue)
            {
                IsLive = false;
                _queue = null;
                return false;
            }
            _queue.Enqueue(turnInfo);
            return true;
        }

        public ObservedTurnInfo Pop()
        {
            if (!IsLive)
                return null;
            if (_queue.Count == 0)
                return null;
            return _queue.Dequeue();
        }
    }
}
