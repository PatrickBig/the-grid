// <copyright file="MockedDbFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Shared.Models;

namespace TheGrid.Tests.Shared
{
    internal class MockedDbFixture
    {
        public void Test()
        {
            //var db = new TheGridDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<TheGridDbContext>())
            //    .WithTable
            var mockSet = Substitute.For<DbSet<Connector>>();
            var mockContext = Substitute.For<TheGridDbContext>();
            mockContext.Connectors.Returns(mockSet);
        }
    }
}
