// -----------------------------------------------------------------------
//  <copyright file="BufferCell.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Structures
{
    public struct BufferCell
    {
        public BufferCell(char character, BufferPoint point, CliConsoleColor color, int elevation)
        {
            Character = character;
            Point = point;
            Color = color;
            Elevation = elevation;
        }

        public char Character { get; }

        public BufferPoint Point { get; }

        public CliConsoleColor Color { get; }
        public int Elevation { get; }
    }
}