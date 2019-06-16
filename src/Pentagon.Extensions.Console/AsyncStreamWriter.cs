// -----------------------------------------------------------------------
//  <copyright file="AsyncStreamWriter.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class AsyncStreamWriter : TextWriter
    {
        readonly Stream stream;

        public AsyncStreamWriter(Stream stream, Encoding encoding)
        {
            this.stream = stream;
            Encoding = encoding;
        }

        public override Encoding Encoding { get; }

        public static void InjectAsConsoleOut()
        {
            Console.SetOut(new AsyncStreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding));
        }

        public override void Write(char[] value, int index, int count)
        {
            var textAsBytes = Encoding.GetBytes(value, index, count);

            Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, textAsBytes, 0, textAsBytes.Length, null);
        }

        public override void Write(char value)
        {
            Write(new[] {value});
        }
    }
}