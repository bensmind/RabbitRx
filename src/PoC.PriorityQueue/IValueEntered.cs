using System;


namespace PoC.PriorityQueue
{
    public interface IValueEntered
    {
        string Value { get; }
        int Priority { get; set; }
    }
}
