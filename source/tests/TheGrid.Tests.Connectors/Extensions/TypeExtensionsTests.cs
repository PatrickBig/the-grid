// <copyright file="TypeExtensionsTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json;
using TheGrid.Connectors.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Tests.Connectors.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TypeExtensions"/> class.
    /// </summary>
    public class TypeExtensionsTests
    {
        /// <summary>
        /// Tests that each mapped CLR type resolves to its expected <see cref="QueryResultColumnType"/>.
        /// </summary>
        /// <param name="type">The CLR type to map.</param>
        /// <param name="expected">The expected <see cref="QueryResultColumnType"/>.</param>
        [Theory]
        [InlineData(typeof(short), QueryResultColumnType.Integer)]
        [InlineData(typeof(ushort), QueryResultColumnType.Integer)]
        [InlineData(typeof(int), QueryResultColumnType.Integer)]
        [InlineData(typeof(uint), QueryResultColumnType.Long)]
        [InlineData(typeof(long), QueryResultColumnType.Long)]
        [InlineData(typeof(decimal), QueryResultColumnType.Decimal)]
        [InlineData(typeof(TimeSpan), QueryResultColumnType.Time)]
        [InlineData(typeof(DateTime), QueryResultColumnType.DateTime)]
        [InlineData(typeof(bool), QueryResultColumnType.Boolean)]
        [InlineData(typeof(string), QueryResultColumnType.Text)]
        [InlineData(typeof(Guid), QueryResultColumnType.Guid)]
        [InlineData(typeof(byte[]), QueryResultColumnType.Binary)]
        [InlineData(typeof(JsonElement), QueryResultColumnType.Json)]
        [InlineData(typeof(JsonDocument), QueryResultColumnType.Json)]
        public void GetQueryResultColumnTypeForType_MappedTypes_Test(Type type, QueryResultColumnType expected)
        {
            // Act
            var result = type.GetQueryResultColumnTypeForType();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that an unmapped CLR type resolves to <see cref="QueryResultColumnType.Unknown"/> instead of silently becoming <see cref="QueryResultColumnType.Text"/>.
        /// </summary>
        [Fact]
        public void GetQueryResultColumnTypeForType_UnmappedType_ReturnsUnknown_Test()
        {
            // Act
            var result = typeof(double).GetQueryResultColumnTypeForType();

            // Assert
            Assert.Equal(QueryResultColumnType.Unknown, result);
        }

        /// <summary>
        /// Tests that <see cref="Nullable{T}"/> types are unwrapped before mapping.
        /// </summary>
        [Fact]
        public void GetQueryResultColumnTypeForType_NullableType_UnwrapsUnderlyingType_Test()
        {
            // Act
            var result = typeof(int?).GetQueryResultColumnTypeForType();

            // Assert
            Assert.Equal(QueryResultColumnType.Integer, result);
        }

        /// <summary>
        /// Tests that <see cref="TheGrid.Shared.Models.QueryResultColumnType"/> and <see cref="TheGrid.Models.QueryResultColumnType"/>
        /// declare identical member names so Mapster's unconfigured <c>.Adapt&lt;&gt;()</c> conversion between them stays correct.
        /// </summary>
        [Fact]
        public void QueryResultColumnType_SharedAndModelsEnums_HaveIdenticalMemberNames_Test()
        {
            // Arrange
            var sharedNames = Enum.GetNames(typeof(TheGrid.Shared.Models.QueryResultColumnType));
            var modelsNames = Enum.GetNames(typeof(TheGrid.Models.QueryResultColumnType));

            // Assert
            Assert.Equal(sharedNames, modelsNames);
        }

        /// <summary>
        /// Tests that the underlying integer values of the original 7 <see cref="QueryResultColumnType"/> members are unchanged,
        /// since <see cref="TheGrid.Models.Column.Type"/> is persisted by EF Core as its raw underlying <see cref="int"/>.
        /// </summary>
        [Fact]
        public void QueryResultColumnType_OriginalMemberOrdinalValues_AreUnchanged_Test()
        {
            Assert.Equal(0, (int)QueryResultColumnType.Text);
            Assert.Equal(1, (int)QueryResultColumnType.Boolean);
            Assert.Equal(2, (int)QueryResultColumnType.Integer);
            Assert.Equal(3, (int)QueryResultColumnType.Long);
            Assert.Equal(4, (int)QueryResultColumnType.Decimal);
            Assert.Equal(5, (int)QueryResultColumnType.DateTime);
            Assert.Equal(6, (int)QueryResultColumnType.Time);
        }
    }
}
