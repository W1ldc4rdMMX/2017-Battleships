﻿using System.Drawing;
using System.Reflection;

namespace ReferenceBot.Domain.Command
{
    public class Command
    {
        public Command(Code code, int x, int y)
        {
            CommandCode = code;
            Coordinate = new Point(x, y);
        }

        public Code CommandCode { get; set; }

        public Point Coordinate { get; set; }

        public override string ToString()
        {
            return $"{(int)CommandCode},{Coordinate.X},{Coordinate.Y}";
        }
    }
}