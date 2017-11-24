// -----------------------------------------------------------------------
//  <copyright file="WindowSelector.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Buffers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Collections;
    using EventArguments;
    using Registration;

    [Register(RegisterType.Singleton, typeof(ISelectable<ConsoleWindow>))]
    public class WindowSelector : IContainer<ConsoleWindow>, ISelectable<ConsoleWindow>
    {
        readonly List<ConsoleWindow> _objects = new List<ConsoleWindow>();

        public event SelectedEventHandler<ConsoleWindow> Selected;

        public int Count { get; }
        public ConsoleWindow Current { get; private set; }

        public ConsoleWindow this[int index] => _objects[index];

        public int this[ConsoleWindow value] => _objects.IndexOf(value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ConsoleWindow> GetEnumerator() => _objects.GetEnumerator();

        public void AddItem(ConsoleWindow item)
        {
            _objects.Add(item);
            if (Current == null)
                Select(item);
        }

        public void Select(ConsoleWindow obj)
        {
            if (this[obj] == -1)
                return;

            Current = obj;
            Selected?.Invoke(this, new SelectedEventArgs<ConsoleWindow>(obj, this[obj]));
        }

        public void SelectNext()
        {
            throw new NotSupportedException();
        }

        public void SelectPrevious()
        {
            throw new NotSupportedException();
        }
    }
}