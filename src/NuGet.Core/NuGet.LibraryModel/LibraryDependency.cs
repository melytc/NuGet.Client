// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet.Common;
using NuGet.Shared;
using NuGet.Versioning;

namespace NuGet.LibraryModel
{
    public class LibraryDependency : IEquatable<LibraryDependency>
    {
        // This class has been optimized to allocate its fields in a 32-bit int property.
        // Changes to Enums or fields must consider this optimization.

        private int _flags = InitialState;

        private static readonly int InitialState =
            // Default IncludeType, shifted to position
            ((int)LibraryIncludeFlags.All << 3) |
            // Default SuppressParent, shifted to position
            ((int)LibraryIncludeFlagUtils.DefaultSuppressParent << 13) |
            // Default ReferenceType, shifted to position
            ((int)LibraryDependencyReferenceType.Direct << 23);

        public bool GeneratePathProperty
        {
            // This property is stored in the lowest bit (0b1) in position 00.
            get => (_flags & 0b1) != 0;
            set => _flags = value ? (_flags | 0b1) : (_flags & ~0b1);
        }

        /// <summary>
        /// True if the PackageReference is added by the SDK and not the user.
        /// </summary>
        public bool AutoReferenced
        {
            // This property is stored in the second lowest bit (0b10) in position 01.
            get => (_flags & 0b10) != 0;
            set => _flags = value ? (_flags | 0b10) : (_flags & ~0b10);
        }

        /// <summary>
        /// True if the dependency has the version set through CentralPackageVersionManagement file.
        /// </summary>
        public bool VersionCentrallyManaged
        {
            // This property is stored in the third lowest bit (0b100) in position 02.
            get => (_flags & 0b100) != 0;
            set => _flags = value ? (_flags | 0b100) : (_flags & ~0b100);
        }

        public LibraryIncludeFlags IncludeType
        {
            // This property is stored in 10 bits (0b1111_1111_1100_0), in positions 03 to 12.
            get => (LibraryIncludeFlags)((_flags >> 3) & 0b1111_1111_11);
            set => _flags = (_flags & ~(0b1111_1111_11 << 3)) | ((int)value << 3);
        }

        public LibraryIncludeFlags SuppressParent
        {
            // This property is stored in 10 bits (0b1111_1111_1100_0000_0000_000), in positions 13 to 22.
            get => (LibraryIncludeFlags)((_flags >> 13) & 0b1111_1111_11);
            set => _flags = (_flags & ~(0b1111_1111_11 << 13)) | ((int)value << 13);
        }

        /// <summary>
        /// Information regarding if the dependency is direct or transitive.  
        /// </summary>
        public LibraryDependencyReferenceType ReferenceType
        {
            // This property is stored in 6 bits (0b1111_1100_0000_0000_0000_0000_0000_0), in positions 23 to 28.
            get => (LibraryDependencyReferenceType)((_flags >> 23) & 0b1111_11);
            set => _flags = (_flags & ~(0b1111_11 << 23)) | ((int)value << 23);
        }

        public IList<NuGetLogCode> NoWarn
        {
            // Checks if _noWarn is null, and if it is, assign it to a new instance before returning it.
            get => _noWarn ??= new List<NuGetLogCode>();
            set => _noWarn = value;
        }

        private IList<NuGetLogCode> _noWarn;

        /// <summary>
        /// This internal field will help us avoid allocating a list when calling the count on a null.
        /// </summary>
        public int NoWarnCount => _noWarn?.Count ?? 0;

        public LibraryRange LibraryRange { get; set; }

        public string Name => LibraryRange.Name;

        public string Aliases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating a version override for any centrally defined version.
        /// </summary>
        public VersionRange VersionOverride { get; set; }

        public LibraryDependency() { }

        internal LibraryDependency(
            LibraryRange libraryRange,
            LibraryIncludeFlags includeType,
            LibraryIncludeFlags suppressParent,
            IList<NuGetLogCode> noWarn,
            bool autoReferenced,
            bool generatePathProperty,
            bool versionCentrallyManaged,
            LibraryDependencyReferenceType libraryDependencyReferenceType,
            string aliases,
            VersionRange versionOverride)
        {
            LibraryRange = libraryRange;
            IncludeType = includeType;
            SuppressParent = suppressParent;
            NoWarn = noWarn;
            AutoReferenced = autoReferenced;
            GeneratePathProperty = generatePathProperty;
            VersionCentrallyManaged = versionCentrallyManaged;
            ReferenceType = libraryDependencyReferenceType;
            Aliases = aliases;
            VersionOverride = versionOverride;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(LibraryRange);
            sb.Append(" ");
            sb.Append(LibraryIncludeFlagUtils.GetFlagString(IncludeType));
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCodeCombiner();

            hashCode.AddObject(LibraryRange);
            hashCode.AddStruct(IncludeType);
            hashCode.AddStruct(SuppressParent);
            hashCode.AddObject(AutoReferenced);
            hashCode.AddSequence(NoWarn);
            hashCode.AddObject(GeneratePathProperty);
            hashCode.AddObject(VersionCentrallyManaged);
            hashCode.AddObject(Aliases);
            hashCode.AddStruct(ReferenceType);

            return hashCode.CombinedHash;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LibraryDependency);
        }

        public bool Equals(LibraryDependency other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return AutoReferenced == other.AutoReferenced &&
                   EqualityUtility.EqualsWithNullCheck(LibraryRange, other.LibraryRange) &&
                   IncludeType == other.IncludeType &&
                   SuppressParent == other.SuppressParent &&
                   NoWarn.SequenceEqualWithNullCheck(other.NoWarn) &&
                   GeneratePathProperty == other.GeneratePathProperty &&
                   VersionCentrallyManaged == other.VersionCentrallyManaged &&
                   Aliases == other.Aliases &&
                   EqualityUtility.EqualsWithNullCheck(VersionOverride, other.VersionOverride) &&
                   ReferenceType == other.ReferenceType;
        }

        public LibraryDependency Clone()
        {
            var clonedLibraryRange = new LibraryRange(LibraryRange.Name, LibraryRange.VersionRange, LibraryRange.TypeConstraint);
            var clonedNoWarn = new List<NuGetLogCode>(NoWarn);

            return new LibraryDependency(clonedLibraryRange, IncludeType, SuppressParent, clonedNoWarn, AutoReferenced, GeneratePathProperty, VersionCentrallyManaged, ReferenceType, Aliases, VersionOverride);
        }

        /// <summary>
        /// Merge the CentralVersion information to the package reference information.
        /// </summary>
        public static void ApplyCentralVersionInformation(IList<LibraryDependency> packageReferences, IDictionary<string, CentralPackageVersion> centralPackageVersions)
        {
            if (packageReferences == null)
            {
                throw new ArgumentNullException(nameof(packageReferences));
            }
            if (centralPackageVersions == null)
            {
                throw new ArgumentNullException(nameof(centralPackageVersions));
            }
            if (centralPackageVersions.Count > 0)
            {
                foreach (LibraryDependency d in packageReferences.Where(d => !d.AutoReferenced && d.LibraryRange.VersionRange == null))
                {
                    if (d.VersionOverride != null)
                    {
                        d.LibraryRange.VersionRange = d.VersionOverride;

                        continue;
                    }

                    if (centralPackageVersions.TryGetValue(d.Name, out CentralPackageVersion centralPackageVersion))
                    {
                        d.LibraryRange.VersionRange = centralPackageVersion.VersionRange;
                    }

                    d.VersionCentrallyManaged = true;
                }
            }
        }
    }
}
