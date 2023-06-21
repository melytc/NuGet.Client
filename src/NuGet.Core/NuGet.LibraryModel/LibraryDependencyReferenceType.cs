// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.LibraryModel
{
    // Represents the reference type of a library dependency (None, Direct, or Transitive).
    // These values are bit-packed in flags.
    // Up to 6 bits can be used to represent new values.
    [Flags]
    public enum LibraryDependencyReferenceType : byte
    {
        None = 0,
        Transitive = 1,
        Direct = 2
    }
}
