﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;
using Saitynai.Backend.Services;

#nullable disable

namespace Saitynai.Backend.Migrations
{
    [DbContext(typeof(SaitynaiDbContext))]
    [Migration("20241219011938_TagEmbedding")]
    partial class TagEmbedding
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("saitynai")
                .HasAnnotation("ProductVersion", "9.0.0-rc.1.24451.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "vector");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Embedding", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Vector>("Value")
                        .HasColumnType("vector")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("pk_embeddings");

                    b.ToTable("embeddings", "saitynai");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Note", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<Guid?>("EmbeddingId")
                        .HasColumnType("uuid")
                        .HasColumnName("embedding_id");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.HasKey("Id")
                        .HasName("pk_notes");

                    b.HasIndex("EmbeddingId")
                        .HasDatabaseName("ix_notes_embedding_id");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_notes_owner_id");

                    b.ToTable("notes", "saitynai");

                    b.HasData(
                        new
                        {
                            Id = new Guid("b0c5301d-4a02-427d-bb10-2a23b281d2fc"),
                            Content = "Seeded note.",
                            OwnerId = new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5")
                        });
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.NoteTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("NoteId")
                        .HasColumnType("uuid")
                        .HasColumnName("note_id");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid")
                        .HasColumnName("tag_id");

                    b.HasKey("Id")
                        .HasName("pk_note_tags");

                    b.HasIndex("NoteId")
                        .HasDatabaseName("ix_note_tags_note_id");

                    b.HasIndex("TagId")
                        .HasDatabaseName("ix_note_tags_tag_id");

                    b.ToTable("note_tags", "saitynai");

                    b.HasData(
                        new
                        {
                            Id = new Guid("9acad4bd-aa3e-42e5-8496-c18b1ba2b2c9"),
                            NoteId = new Guid("b0c5301d-4a02-427d-bb10-2a23b281d2fc"),
                            TagId = new Guid("51532d76-1926-4adb-9173-85485876ea42")
                        },
                        new
                        {
                            Id = new Guid("61724840-f4f5-4286-9b73-d56dd059b1dc"),
                            NoteId = new Guid("b0c5301d-4a02-427d-bb10-2a23b281d2fc"),
                            TagId = new Guid("331226db-d78d-4dd4-aaab-fb9c2e92d84a")
                        });
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Vector>("Embedding")
                        .HasColumnType("vector")
                        .HasColumnName("embedding");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid")
                        .HasColumnName("group_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("ix_tags_group_id");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_tags_owner_id");

                    b.HasIndex("Name", "GroupId", "OwnerId")
                        .IsUnique()
                        .HasDatabaseName("ix_tags_name_group_id_owner_id");

                    b.ToTable("tags", "saitynai");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.TagGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Guid?>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid")
                        .HasColumnName("parent_id");

                    b.HasKey("Id")
                        .HasName("pk_tag_groups");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_tag_groups_owner_id");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_tag_groups_parent_id");

                    b.HasIndex("Name", "ParentId", "OwnerId")
                        .IsUnique()
                        .HasDatabaseName("ix_tag_groups_name_parent_id_owner_id");

                    b.ToTable("tag_groups", "saitynai");

                    b.HasData(
                        new
                        {
                            Id = new Guid("7e9a83c8-2bac-418b-af6b-8ddc2ec34ae7"),
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Seeded tag group",
                            OwnerId = new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5")
                        });
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean")
                        .HasColumnName("is_admin");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasDatabaseName("ix_users_username");

                    b.ToTable("users", "saitynai");

                    b.HasData(
                        new
                        {
                            Id = new Guid("85891986-ba23-499e-8c02-59bee76a574e"),
                            Email = "adminas.adminavicius@email.com",
                            IsAdmin = true,
                            Password = "admin123",
                            Username = "admin"
                        },
                        new
                        {
                            Id = new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5"),
                            Email = "user.useris@email.com",
                            IsAdmin = false,
                            Password = "user123",
                            Username = "user"
                        });
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Note", b =>
                {
                    b.HasOne("Saitynai.Backend.Contracts.Models.Embedding", "Embedding")
                        .WithMany()
                        .HasForeignKey("EmbeddingId")
                        .HasConstraintName("fk_notes_embeddings_embedding_id");

                    b.HasOne("Saitynai.Backend.Contracts.Models.User", "Owner")
                        .WithMany("Contents")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_notes_users_owner_id");

                    b.Navigation("Embedding");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.NoteTag", b =>
                {
                    b.HasOne("Saitynai.Backend.Contracts.Models.Note", "Note")
                        .WithMany("Tags")
                        .HasForeignKey("NoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_note_tags_notes_note_id");

                    b.HasOne("Saitynai.Backend.Contracts.Models.Tag", "Tag")
                        .WithMany("Contents")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_note_tags_tags_tag_id");

                    b.Navigation("Note");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Tag", b =>
                {
                    b.HasOne("Saitynai.Backend.Contracts.Models.TagGroup", "Group")
                        .WithMany("Tags")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tags_tag_groups_group_id");

                    b.HasOne("Saitynai.Backend.Contracts.Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .HasConstraintName("fk_tags_users_owner_id");

                    b.Navigation("Group");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.TagGroup", b =>
                {
                    b.HasOne("Saitynai.Backend.Contracts.Models.User", "Owner")
                        .WithMany("TagGroups")
                        .HasForeignKey("OwnerId")
                        .HasConstraintName("fk_tag_groups_users_owner_id");

                    b.HasOne("Saitynai.Backend.Contracts.Models.TagGroup", "Parent")
                        .WithMany("ChildrenGroups")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_tag_groups_tag_groups_parent_id");

                    b.Navigation("Owner");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Note", b =>
                {
                    b.Navigation("Tags");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.Tag", b =>
                {
                    b.Navigation("Contents");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.TagGroup", b =>
                {
                    b.Navigation("ChildrenGroups");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("Saitynai.Backend.Contracts.Models.User", b =>
                {
                    b.Navigation("Contents");

                    b.Navigation("TagGroups");
                });
#pragma warning restore 612, 618
        }
    }
}
