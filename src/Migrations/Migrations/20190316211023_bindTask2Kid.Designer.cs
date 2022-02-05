﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using server.Data;

namespace core.Migrations
{
    [DbContext(typeof(WalletContext))]
    [Migration("20190316211023_bindTask2Kid")]
    partial class bindTask2Kid
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("core.Repositories.Entities.SavingsAccountEntity", b =>
                {
                    b.Property<int>("SavingsAccountEntityId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CalculatedFunds");

                    b.Property<int>("DepositFunds");

                    b.Property<DateTime>("ReleaseDate");

                    b.Property<int>("UserId");

                    b.HasKey("SavingsAccountEntityId");

                    b.ToTable("SavingAccounts");
                });

            modelBuilder.Entity("core.Repositories.Entities.TaskEntity", b =>
                {
                    b.Property<int>("TaskId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<string>("ImgUrl");

                    b.Property<int>("Payout");

                    b.Property<int?>("SpecificUserId");

                    b.Property<int>("Status");

                    b.Property<int?>("UserId");

                    b.HasKey("TaskId");

                    b.ToTable("tasks");
                });

            modelBuilder.Entity("core.Repositories.Models.SpendingAccount", b =>
                {
                    b.Property<int>("SpendingAccountId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Balance");

                    b.HasKey("SpendingAccountId");

                    b.ToTable("SpendingAccounts");
                });

            modelBuilder.Entity("core.Repositories.Models.TransactionData", b =>
                {
                    b.Property<int>("TransactionDataId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountId");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<int>("DestAccountId");

                    b.Property<int>("DestAccountType");

                    b.Property<int>("Funds");

                    b.Property<int?>("SavingsAccountEntityId");

                    b.Property<int>("SourceAccountId");

                    b.Property<int>("SourceAccountType");

                    b.Property<int?>("SpendingAccountId");

                    b.HasKey("TransactionDataId");

                    b.HasIndex("SavingsAccountEntityId");

                    b.HasIndex("SpendingAccountId");

                    b.ToTable("AccountHistories");
                });

            modelBuilder.Entity("core.Repositories.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("SpendingAccountId");

                    b.Property<int>("FriendPoints");

                    b.Property<string>("Name");

                    b.Property<int?>("ParentId");

                    b.Property<string>("ProfileImg");

                    b.Property<int>("Role");

                    b.HasKey("UserId", "SpendingAccountId");

                    b.HasAlternateKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("core.Repositories.Models.TransactionData", b =>
                {
                    b.HasOne("core.Repositories.Entities.SavingsAccountEntity", "SavingsAccountEntity")
                        .WithMany()
                        .HasForeignKey("SavingsAccountEntityId");

                    b.HasOne("core.Repositories.Models.SpendingAccount", "SpendingAccount")
                        .WithMany()
                        .HasForeignKey("SpendingAccountId");
                });
#pragma warning restore 612, 618
        }
    }
}
