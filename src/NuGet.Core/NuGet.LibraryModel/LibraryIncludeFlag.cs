// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.LibraryModel
{
    // These values are bit-packed in flags.
    // Up to 16 bits can be used to represent new values.
    // If adding more values, the class LibraryDependency needs to be revisited for its storage allocation,
    // as it currently allocates only 10 bits.
    [Flags]
    public enum LibraryIncludeFlags : ushort
    {
        None = 0,
        Runtime = 1 << 0,
        Compile = 1 << 1,
        Build = 1 << 2,
        Native = 1 << 3,
        ContentFiles = 1 << 4,
        Analyzers = 1 << 5,
        BuildTransitive = 1 << 6,
        All = Analyzers | Build | Compile | ContentFiles | Native | Runtime | BuildTransitive
    }
}
