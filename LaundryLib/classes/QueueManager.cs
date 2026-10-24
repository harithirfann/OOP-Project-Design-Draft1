using System;
using System.Collections.Generic;

namespace LaundryLib
{
    public class QueueManager
    {
        public List<User> Waiting { get; } = new List<User>();

        public void Join(User user)
        {
            if (!Waiting.Contains(user))
                Waiting.Add(user);
        }

        public User Next()
        {
            if (IsEmpty()) return null;
            User first = Waiting[0];
            Waiting.RemoveAt(0);
            return first;
        }

        //1-based, 0 means not in the queue
        public int Position(User user)
        {
            int index = Waiting.IndexOf(user);
            return index < 0 ? 0 : index + 1;
        }

        public bool IsEmpty() => Waiting.Count == 0;
    }
}
