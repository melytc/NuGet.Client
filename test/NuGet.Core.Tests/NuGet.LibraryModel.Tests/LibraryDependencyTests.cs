// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Common;
using NuGet.Versioning;
using Xunit;

namespace NuGet.LibraryModel.Tests
{
    public class LibraryDependencyTests
    {
        [Fact]
        public void LibraryDependency_Clone_Equals()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var clone = target.Clone();

            // Assert
            Assert.NotSame(target, clone);
            Assert.Equal(target, clone);
        }

        [Fact]
        public void LibraryDependency_Clone_ClonesLibraryRange()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var clone = target.Clone();
            clone.LibraryRange.Name = "SomethingElse";

            // Assert
            Assert.NotSame(target.LibraryRange, clone.LibraryRange);
            Assert.NotEqual(target.LibraryRange.Name, clone.LibraryRange.Name);
        }

        [Fact]
        public void LibraryDependency_ApplyCentralVersionInformation_NullArgumentCheck()
        {
            // Arrange
            List<LibraryDependency> packageReferences = new List<LibraryDependency>();
            Dictionary<string, CentralPackageVersion> centralPackageVersions = new Dictionary<string, CentralPackageVersion>();

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => LibraryDependency.ApplyCentralVersionInformation(null, centralPackageVersions));
            Assert.Throws<ArgumentNullException>(() => LibraryDependency.ApplyCentralVersionInformation(packageReferences, null));
        }

        [Fact]
        public void LibraryDependency_ApplyCentralVersionInformation_CPVIsMergedTpPackageVersions()
        {
            var dep1 = new LibraryDependency()
            {
                LibraryRange = new LibraryRange() { Name = "fooMerged" },
            };
            var dep2 = new LibraryDependency()
            {
                LibraryRange = new LibraryRange() { Name = "barNotMerged", VersionRange = VersionRange.Parse("1.0.0") },
            };
            var dep3 = new LibraryDependency()
            {
                LibraryRange = new LibraryRange() { Name = "bazNotMerged" },
                AutoReferenced = true
            };
            List<LibraryDependency> deps = new List<LibraryDependency>() { dep1, dep2, dep3 };

            var cpv1 = new CentralPackageVersion(dep1.Name.ToLower(), VersionRange.Parse("2.0.0"));
            var cpv2 = new CentralPackageVersion(dep2.Name.ToLower(), VersionRange.Parse("2.0.0"));
            var cpv3 = new CentralPackageVersion(dep3.Name.ToLower(), VersionRange.Parse("2.0.0"));
            Dictionary<string, CentralPackageVersion> cpvs = new Dictionary<string, CentralPackageVersion>(StringComparer.OrdinalIgnoreCase)
            { [cpv1.Name] = cpv1, [cpv2.Name] = cpv2, [cpv3.Name] = cpv3 };

            // Act
            LibraryDependency.ApplyCentralVersionInformation(deps, cpvs);

            // Assert
            Assert.True(dep1.VersionCentrallyManaged);
            Assert.False(dep2.VersionCentrallyManaged);
            Assert.False(dep3.VersionCentrallyManaged);

            Assert.Equal("[2.0.0, )", dep1.LibraryRange.VersionRange.ToNormalizedString());
            Assert.Equal("[1.0.0, )", dep2.LibraryRange.VersionRange.ToNormalizedString());
            Assert.Null(dep3.LibraryRange.VersionRange);
        }

        public LibraryDependency GetTarget()
        {
            return new LibraryDependency
            {
                IncludeType = LibraryIncludeFlags.Build | LibraryIncludeFlags.Compile,
                LibraryRange = new LibraryRange
                {
                    Name = "SomeLibrary",
                    TypeConstraint = LibraryDependencyTarget.ExternalProject | LibraryDependencyTarget.WinMD,
                    VersionRange = new VersionRange(new NuGetVersion("4.0.0-rc2"))
                },
                SuppressParent = LibraryIncludeFlags.Analyzers | LibraryIncludeFlags.ContentFiles,
                Aliases = "stuff",
            };
        }
    }

    public class LibraryDependencyPropertiesTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Test_BoolProperties(bool GeneratePathProperty, bool AutoReferenced, bool VersionCentrallyManaged)
        {
            var libraryDependency = new LibraryDependency();

            libraryDependency.GeneratePathProperty = GeneratePathProperty;
            libraryDependency.AutoReferenced = AutoReferenced;
            libraryDependency.VersionCentrallyManaged = VersionCentrallyManaged;

            Assert.Equal(GeneratePathProperty, libraryDependency.GeneratePathProperty);
            Assert.Equal(AutoReferenced, libraryDependency.AutoReferenced);
            Assert.Equal(VersionCentrallyManaged, libraryDependency.VersionCentrallyManaged);
        }

        [Theory]
        [InlineData(LibraryIncludeFlags.None)]
        [InlineData(LibraryIncludeFlags.Runtime)]
        [InlineData(LibraryIncludeFlags.Compile)]
        [InlineData(LibraryIncludeFlags.Build)]
        [InlineData(LibraryIncludeFlags.Native)]
        [InlineData(LibraryIncludeFlags.ContentFiles)]
        [InlineData(LibraryIncludeFlags.Analyzers)]
        [InlineData(LibraryIncludeFlags.BuildTransitive)]
        [InlineData(LibraryIncludeFlags.All)]
        [InlineData(LibraryIncludeFlags.Build | LibraryIncludeFlags.Compile)]
        [InlineData(LibraryIncludeFlags.Build | LibraryIncludeFlags.Runtime | LibraryIncludeFlags.ContentFiles)]
        public void Test_IncludeTypeProperty(LibraryIncludeFlags flags)
        {
            var libraryDependency = new LibraryDependency
            {
                IncludeType = flags
            };

            Assert.Equal(flags, libraryDependency.IncludeType);
        }

        [Theory]
        [InlineData(LibraryIncludeFlags.None)]
        [InlineData(LibraryIncludeFlags.Runtime)]
        [InlineData(LibraryIncludeFlags.Compile)]
        [InlineData(LibraryIncludeFlags.Build)]
        [InlineData(LibraryIncludeFlags.Native)]
        [InlineData(LibraryIncludeFlags.ContentFiles)]
        [InlineData(LibraryIncludeFlags.Analyzers)]
        [InlineData(LibraryIncludeFlags.BuildTransitive)]
        [InlineData(LibraryIncludeFlags.All)]
        [InlineData(LibraryIncludeFlags.Build | LibraryIncludeFlags.Compile)]
        [InlineData(LibraryIncludeFlags.Build | LibraryIncludeFlags.Runtime | LibraryIncludeFlags.ContentFiles)]
        public void Test_SuppressParentProperty(LibraryIncludeFlags flags)
        {
            var libraryDependency = new LibraryDependency
            {
                SuppressParent = flags
            };

            Assert.Equal(flags, libraryDependency.SuppressParent);
        }

        [Theory]
        [InlineData(LibraryDependencyReferenceType.None)]
        [InlineData(LibraryDependencyReferenceType.Transitive)]
        [InlineData(LibraryDependencyReferenceType.Direct)]
        public void Test_ReferenceTypeProperty(LibraryDependencyReferenceType flags)
        {
            var libraryDependency = new LibraryDependency
            {
                ReferenceType = flags
            };

            Assert.Equal(flags, libraryDependency.ReferenceType);
        }

        public void Test_MultiplePropertiesSet()
        {
            // Test that fields are not overlapping.
        }
    }

    public class LibraryDependencyStorageTests
    {
        private int GetFlags(LibraryDependency libraryDependency)
        {
            var type = libraryDependency.GetType();
            var privateField = type.GetField("_flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (int)privateField.GetValue(libraryDependency);
        }

        [Fact]
        public void GeneratePathProperty_WhenSetToTrue_IsStoredInLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.GeneratePathProperty = true;
            var actualFlags = GetFlags(libraryDependency) & 0b1;

            Assert.Equal(0b1, actualFlags);
        }

        [Fact]
        public void GeneratePathProperty_WhenSetToFalse_IsStoredInLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.GeneratePathProperty = false;
            var actualFlags = GetFlags(libraryDependency) & 0b1;

            Assert.Equal(0, actualFlags);
        }

        [Fact]
        public void AutoReferenced_WhenSetToTrue_IsStoredInSecondLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.AutoReferenced = true;
            var actualFlags = GetFlags(libraryDependency) & 0b10;

            Assert.Equal(0b10, actualFlags);
        }

        [Fact]
        public void AutoReferenced_WhenSetToFalse_IsStoredInSecondLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.AutoReferenced = false;
            var actualFlags = GetFlags(libraryDependency) & 0b10;

            Assert.Equal(0, actualFlags);
        }

        [Fact]
        public void VersionCentrallyManaged_WhenSetToTrue_IsStoredInThirdLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.VersionCentrallyManaged = true;
            var actualFlags = GetFlags(libraryDependency) & 0b100;

            Assert.Equal(0b100, actualFlags);
        }

        [Fact]
        public void VersionCentrallyManaged_WhenSetToFalse_IsStoredInThirdLowestBit()
        {
            var libraryDependency = new LibraryDependency();
            libraryDependency.VersionCentrallyManaged = false;
            var actualFlags = GetFlags(libraryDependency) & 0b100;

            Assert.Equal(0, actualFlags);
        }

        [Fact]
        public void IncludeType_DefaultValue_IsSetToAll()
        {
            var libraryDependency = new LibraryDependency();
            var expectedFlags = (int)LibraryIncludeFlags.All << 3;
            var actualFlags = GetFlags(libraryDependency) & ~(0b1111_1111_11 << 3);

            Assert.Equal(expectedFlags, actualFlags);
        }

        [Fact]
        public void IncludeType_WhenSet_IsStoredIn10BitsInPositions03To12()
        {
            var libraryDependency = new LibraryDependency();
            var includeType = LibraryIncludeFlags.Runtime;
            libraryDependency.IncludeType = includeType;

            var expectedFlags = (int)includeType << 3;
            Assert.Equal(expectedFlags, GetFlags(libraryDependency) & ~(0b1111_1111_11 << 3));
        }

        [Fact]
        public void SuppressParent_DefaultValue_IsSetToDefaultSuppressParent()
        {
            var libraryDependency = new LibraryDependency();
            var expectedFlags = (int)LibraryIncludeFlagUtils.DefaultSuppressParent << 13;

            Assert.Equal(expectedFlags, GetFlags(libraryDependency) & ~(0b1111_1111_11 << 13));
        }

        [Fact]
        public void SuppressParent_WhenSet_IsStoredIn10BitsInPositions13To22()
        {
            var libraryDependency = new LibraryDependency();
            var suppressParent = LibraryIncludeFlags.Build;
            libraryDependency.SuppressParent = suppressParent;

            var expectedFlags = (int)suppressParent << 13;
            Assert.Equal(expectedFlags, GetFlags(libraryDependency) & ~(0b1111_1111_11 << 13));
        }

        [Fact]
        public void ReferenceType_DefaultValue_IsSetToDirect()
        {
            var libraryDependency = new LibraryDependency();
            var expectedFlags = (int)LibraryDependencyReferenceType.Direct << 23;

            Assert.Equal(expectedFlags, GetFlags(libraryDependency) & ~(0b1111_11 << 23));
        }

        [Fact]
        public void ReferenceType_WhenSet_IsStoredIn6BitsInPositions23To28()
        {
            var libraryDependency = new LibraryDependency();
            var referenceType = LibraryDependencyReferenceType.Direct;
            libraryDependency.ReferenceType = referenceType;

            var expectedFlags = (int)referenceType << 23;
            Assert.Equal(expectedFlags, GetFlags(libraryDependency) & ~(0b1111_11 << 23));
        }

        [Fact]
        public void NoWarn_WhenClassIsCreated_IsEmptyList()
        {
            var libraryDependency = new LibraryDependency();
            Assert.Equal(new List<NuGetLogCode>(), libraryDependency.NoWarn);
        }

        [Fact]
        public void NoWarn_WhenAssigned_AccessedCountReturnsProperCount()
        {
            var libraryDependency = new LibraryDependency();
            Assert.Equal(0, libraryDependency.NoWarn.Count);
            Assert.Equal(0, libraryDependency.NoWarnCount);

            libraryDependency.NoWarn = new List<NuGetLogCode> { NuGetLogCode.NU1001, NuGetLogCode.NU1006 };
            Assert.Equal(2, libraryDependency.NoWarn.Count);
            Assert.Equal(2, libraryDependency.NoWarnCount);

            libraryDependency.NoWarn = new List<NuGetLogCode> { };
            Assert.Equal(0, libraryDependency.NoWarn.Count);
            Assert.Equal(0, libraryDependency.NoWarnCount);

            libraryDependency.NoWarn = null;
            Assert.Equal(0, libraryDependency.NoWarn.Count);
            Assert.Equal(0, libraryDependency.NoWarnCount);
        }

    }
}
