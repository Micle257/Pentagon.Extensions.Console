namespace Pentagon.Utilities.Console.Buffers {
    using System.Collections.Generic;
    using Structures;

    public interface IScreenCellCache
    {
        IDictionary<BufferPoint, SortedList<int, BufferCell>> Cache { get; }
        void AddOrReplaceCell(BufferCell cell);
        void RemoveCell(BufferPoint point, int elevation);
    }
}