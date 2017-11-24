// -----------------------------------------------------------------------
//  <copyright file="ObjectFilterBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console
{
    using System;
    using Buffers;
    using Enums;
    using Structures;

    class ObjectFilterBuilder
    {
        WriteStatus _status = WriteStatus.Write;
        RelativeElevationType _elevationType = RelativeElevationType.All;
        int _elevation;
        Func<char, bool> _characterPredicate = c => true;
        Box _box;

        public ObjectFilterBuilder WithStatus(WriteStatus status)
        {
            _status = status;
            return this;
        }

        public ObjectFilterBuilder WithElevationFilter(RelativeElevationType elevationType, int elevation)
        {
            _elevationType = elevationType;
            _elevation = elevation;
            return this;
        }

        public ObjectFilterBuilder WithCharacterFilter(Func<char, bool> characterPredicate)
        {
            _characterPredicate = characterPredicate;
            return this;
        }

        public ObjectFilterBuilder WithBox(Box box)
        {
            _box = box;
            return this;
        }
    }
}