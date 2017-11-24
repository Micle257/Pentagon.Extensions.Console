// -----------------------------------------------------------------------
//  <copyright file="WriteObject.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Buffers
{
    using System.Collections.Generic;
    using Structures;

    public class WriteObject
    {
        readonly bool _vertical;

        public WriteObject(string data, BufferPoint startPoint, ConsoleColour color, int elevation, bool vertical = false)
        {
            _vertical = vertical;
            Data = data;
            StartPoint = startPoint;
            Color = color;
            Elevation = elevation;
            Status = WriteStatus.NotWritten;
        }

        public WriteObject(Box box, ConsoleColour color, int elevation, char ch = ' ', bool vertical = false) : this(new string(ch, box.Width), box.Point, color, elevation, vertical) { }

        public IList<(char character, BufferPoint point)> Characters => GetCharacters();

        public Box Box => new Box(StartPoint, 1, Data.Length);

        public string Data { get; }

        public int Elevation { get; }

        public BufferPoint StartPoint { get; }

        public ConsoleColour Color { get; }

        public bool CanWrite => Status == WriteStatus.NotWritten && !string.IsNullOrEmpty(Data) && StartPoint.IsValid;

        public bool CanRemove => Status == WriteStatus.Write;

        public WriteStatus Status { get; private set; }

        public void Write()
        {
            if (CanWrite)
                Status = WriteStatus.Write;
        }

        public void Remove()
        {
            if (CanRemove)
                Status = WriteStatus.Removed;
        }

        IList<(char character, BufferPoint point)> GetCharacters()
        {
            var list = new List<(char character, BufferPoint point)>();

            var x = 0;

            foreach (var c in Data)
            {
                list.Add((c, StartPoint.WithOffset(_vertical ? 0 : x, !_vertical ? 0 : x)));
                x++;
            }

            return list;
        }
    }
}