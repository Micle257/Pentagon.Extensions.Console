namespace Pentagon.Extensions.Console {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsolePresentation.Controls;
    using JetBrains.Annotations;

    public class ConsoleTextTracker : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            ConsoleWriter.Wrote -= OnWrote;
        }

        public void ClearAll()
        {
            var init = Cursor.Current;

            foreach (var text in _texts.ToList())
            {
                Cursor.SetCurrent(text.Coord);

                ConsoleHelper.WriteSpace(text.Data.Length);
            }

            init.Apply();
        }

        public ConsoleTextTracker()
        {
            ConsoleWriter.Wrote += OnWrote;
        }

        [NotNull]
        readonly List<Text> _texts = new List<Text>();

        void OnWrote(object sender, Text text)
        {
            _texts.Add(text);
        }
    }
}