// -----------------------------------------------------------------------
//  <copyright file="CliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System;
    using System.IO;

    public abstract class CliControl<T>
    {
        public CliLabel Label { get; set; } = CliLabels.FormField;

        public abstract T Run();

        protected abstract void Write();
    }
}