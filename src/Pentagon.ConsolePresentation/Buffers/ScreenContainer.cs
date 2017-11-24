// -----------------------------------------------------------------------
//  <copyright file="ScreenContainer.cs">
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
    using JetBrains.Annotations;

    public class ScreenContainer : ISelectable<IScreen>
    {
        /// <summary> Inner collection representing this instance. </summary>
        [NotNull]
        readonly List<IScreen> _objects = new List<IScreen>();

        /// <inheritdoc />
        public event SelectedEventHandler<IScreen> Selected;

        /// <inheritdoc />
        public int Count => _objects.Count;

        /// <inheritdoc />
        public IScreen Current { get; private set; }

        /// <inheritdoc />
        public IScreen this[int index] => _objects[index];

        /// <inheritdoc />
        public int this[IScreen value] => _objects.IndexOf(value);

        /// <inheritdoc />
        void IContainer<IScreen>.AddItem(IScreen item)
        {
            _objects.Add(item);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IScreen> GetEnumerator() => _objects.GetEnumerator();

        /// <inheritdoc />
        public void Select(IScreen obj)
        {
            if (obj == Current)
                return;

            if (this[obj] == -1)
                _objects.Add(obj);

            Current = obj;
            foreach (var screen in this)
                screen.IsActive = false;
            Current.IsActive = true;
            Selected?.Invoke(this, new SelectedEventArgs<IScreen>(obj, this[obj]));
        }

        /// <inheritdoc />
        public void SelectNext()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SelectPrevious()
        {
            throw new NotImplementedException();
        }
    }
}