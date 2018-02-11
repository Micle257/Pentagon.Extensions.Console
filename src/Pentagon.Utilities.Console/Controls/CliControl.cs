// -----------------------------------------------------------------------
//  <copyright file="CliControl.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Controls
{
    public abstract class CliControl<T>
    {
        public abstract T Run();
        protected abstract void Write();
    }
}