using System.Collections.Generic;

namespace Xam.Plugin.Abstractions.Utility
{
    public class FixedSizeQueue<T> : Stack<T>
    {

        public int MaximumSize { get; set; }
        public FixedSizeQueue(int maxSize) : base()
        {
            MaximumSize = maxSize;
        }

        public new void Push(T item)
        {
            // Throw away (No infinite stack pl0x)
            if (Count + 1 > MaximumSize)
                Pop();

            base.Push(item);
        }

    }
}
