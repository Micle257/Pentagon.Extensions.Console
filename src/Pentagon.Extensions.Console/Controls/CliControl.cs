// -----------------------------------------------------------------------
//  <copyright file="CliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    public abstract class CliControl<T>
    {
        public CliLabel Label { get; set; } = CliLabels.FormField;

        public abstract T Run();

        protected virtual void WriteLabel(string text)
        {
            (Label ?? CliLabels.FormField).Write(text);
        }

        protected abstract void Write();
    }
}