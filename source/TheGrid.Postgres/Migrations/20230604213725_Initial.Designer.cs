﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Models;

#nullable disable

namespace TheGrid.Postgres.Migrations
{
    [DbContext(typeof(TheGridDbContext))]
    [Migration("20230604213725_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "hstore");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TheGrid.Models.DataSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Dictionary<string, string>>("ExecutorParameters")
                        .IsRequired()
                        .HasColumnType("hstore");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("OrganizationId")
                        .HasColumnType("integer");

                    b.Property<string>("QueryRunnerId")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("DataSources");
                });

            modelBuilder.Entity("TheGrid.Models.Query", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Dictionary<string, QueryResultColumn>>("Columns")
                        .HasColumnType("jsonb");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DataSourceId")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("LastErrorMessage")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<Dictionary<string, object>>("Parameters")
                        .HasColumnType("jsonb");

                    b.Property<int>("ResultState")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ResultsRefreshed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.HasIndex("DataSourceId");

                    b.ToTable("Queries");
                });

            modelBuilder.Entity("TheGrid.Models.QueryResultRow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Dictionary<string, object>>("Data")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("QueryId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("QueryId");

                    b.ToTable("QueryResultRows");
                });

            modelBuilder.Entity("TheGrid.QueryRunners.Models.Organization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("TheGrid.Models.DataSource", b =>
                {
                    b.HasOne("TheGrid.QueryRunners.Models.Organization", "Organization")
                        .WithMany("DataSources")
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("TheGrid.Models.Query", b =>
                {
                    b.HasOne("TheGrid.Models.DataSource", "DataSource")
                        .WithMany()
                        .HasForeignKey("DataSourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DataSource");
                });

            modelBuilder.Entity("TheGrid.Models.QueryResultRow", b =>
                {
                    b.HasOne("TheGrid.Models.Query", "Query")
                        .WithMany()
                        .HasForeignKey("QueryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Query");
                });

            modelBuilder.Entity("TheGrid.QueryRunners.Models.Organization", b =>
                {
                    b.Navigation("DataSources");
                });
#pragma warning restore 612, 618
        }
    }
}
