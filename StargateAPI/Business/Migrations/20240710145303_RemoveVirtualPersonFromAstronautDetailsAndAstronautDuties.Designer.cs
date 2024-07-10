﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StargateAPI.Business.Data;

#nullable disable

namespace StargateAPI.Migrations
{
    [DbContext(typeof(StargateContext))]
    [Migration("20240710145303_RemoveVirtualPersonFromAstronautDetailsAndAstronautDuties")]
    partial class RemoveVirtualPersonFromAstronautDetailsAndAstronautDuties
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("StargateAPI.Business.Data.AstronautDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("CareerEndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CareerStartDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CurrentDutyTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CurrentRank")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PersonId")
                        .IsUnique();

                    b.ToTable("AstronautDetail");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CareerStartDate = new DateTime(2024, 7, 10, 9, 53, 3, 630, DateTimeKind.Local).AddTicks(6934),
                            CurrentDutyTitle = "Commander",
                            CurrentRank = "1LT",
                            PersonId = 1
                        },
                        new
                        {
                            Id = 2,
                            CareerStartDate = new DateTime(1957, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            CurrentDutyTitle = "Pilot",
                            CurrentRank = "Senior Lieutenant",
                            PersonId = 3
                        });
                });

            modelBuilder.Entity("StargateAPI.Business.Data.AstronautDuty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DutyEndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DutyStartDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("DutyTitle")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Rank")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("AstronautDuty");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DutyStartDate = new DateTime(2024, 7, 10, 9, 53, 3, 630, DateTimeKind.Local).AddTicks(7052),
                            DutyTitle = "Commander",
                            PersonId = 1,
                            Rank = "1LT"
                        },
                        new
                        {
                            Id = 2,
                            DutyStartDate = new DateTime(1960, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DutyTitle = "Pilot",
                            PersonId = 3,
                            Rank = "Senior Lieutenant"
                        });
                });

            modelBuilder.Entity("StargateAPI.Business.Data.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Person");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "John Doe"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Jane Doe"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Yuri"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Roger"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Charlie"
                        },
                        new
                        {
                            Id = 6,
                            Name = "Fred"
                        },
                        new
                        {
                            Id = 7,
                            Name = "Tom"
                        },
                        new
                        {
                            Id = 8,
                            Name = "Richard"
                        },
                        new
                        {
                            Id = 9,
                            Name = "Harry"
                        },
                        new
                        {
                            Id = 10,
                            Name = "Mary"
                        },
                        new
                        {
                            Id = 11,
                            Name = "Martha"
                        },
                        new
                        {
                            Id = 12,
                            Name = "Sierra"
                        },
                        new
                        {
                            Id = 13,
                            Name = "Francine"
                        });
                });

            modelBuilder.Entity("StargateAPI.Business.Data.AstronautDetail", b =>
                {
                    b.HasOne("StargateAPI.Business.Data.Person", "Person")
                        .WithOne("AstronautDetail")
                        .HasForeignKey("StargateAPI.Business.Data.AstronautDetail", "PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("StargateAPI.Business.Data.AstronautDuty", b =>
                {
                    b.HasOne("StargateAPI.Business.Data.Person", "Person")
                        .WithMany("AstronautDuties")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("StargateAPI.Business.Data.Person", b =>
                {
                    b.Navigation("AstronautDetail");

                    b.Navigation("AstronautDuties");
                });
#pragma warning restore 612, 618
        }
    }
}