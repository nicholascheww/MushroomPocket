﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MushroomPocket;

#nullable disable

namespace MushroomPocket.Migrations
{
    [DbContext(typeof(MushroomPocketContext))]
    partial class MushroomPocketContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.5");

            modelBuilder.Entity("MushroomPocket.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("atk")
                        .HasColumnType("INTEGER");

                    b.Property<int>("exp")
                        .HasColumnType("INTEGER");

                    b.Property<int>("hp")
                        .HasColumnType("INTEGER");

                    b.Property<string>("skill")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Characters");
                });
#pragma warning restore 612, 618
        }
    }
}
