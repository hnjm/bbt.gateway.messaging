﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using bbt.gateway.messaging;

namespace bbt.gateway.messaging.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211019131601_reset2")]
    partial class reset2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("bbt.gateway.messaging.Models.Header", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Branch")
                        .HasColumnType("TEXT");

                    b.Property<string>("BusinessLine")
                        .HasColumnType("TEXT");

                    b.Property<int>("ContentType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EmailTemplatePrefix")
                        .HasColumnType("TEXT");

                    b.Property<string>("EmailTemplateSuffix")
                        .HasColumnType("TEXT");

                    b.Property<string>("SmsPrefix")
                        .HasColumnType("TEXT");

                    b.Property<string>("SmsSender")
                        .HasColumnType("TEXT");

                    b.Property<string>("SmsSuffix")
                        .HasColumnType("TEXT");

                    b.Property<string>("SmsTemplatePrefix")
                        .HasColumnType("TEXT");

                    b.Property<string>("SmsTemplateSuffix")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Headers");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpBlackListEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PhoneConfigurationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ResolvedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidTo")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneConfigurationId");

                    b.ToTable("OtpBlackListEntries");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpBlackListEntryLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("BlackListEntryId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ParameterMaster")
                        .HasColumnType("TEXT");

                    b.Property<string>("ParameterSlave")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BlackListEntryId");

                    b.ToTable("OtpBlackListEntryLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpOperatorException", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Operator")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReplaceWith")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidTo")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("OtpOperatorExceptions");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfiguration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Operator")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PhoneConfigurations");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfigurationLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ParameterMaster")
                        .HasColumnType("TEXT");

                    b.Property<string>("ParameterSlave")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PhoneId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneId");

                    b.ToTable("PhoneConfigurationLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendOtpRequestLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PhoneId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneId");

                    b.ToTable("SendOtpRequestLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendOtpResponseLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Operator")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OperatorResponseCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OperatorResponseMessage")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("SendOtpRequestLogId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SendOtpRequestLogId");

                    b.ToTable("SendOtpResponseLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendSmsLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Operator")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OperatorResponseCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OperatorResponseMessage")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PhoneId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneId");

                    b.ToTable("SendSmsLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpBlackListEntry", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "PhoneConfiguration")
                        .WithMany("BlacklistEntries")
                        .HasForeignKey("PhoneConfigurationId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Phone", "Phone", b1 =>
                        {
                            b1.Property<Guid>("OtpBlackListEntryId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("CountryCode")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Number")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Prefix")
                                .HasColumnType("INTEGER");

                            b1.HasKey("OtpBlackListEntryId");

                            b1.ToTable("OtpBlackListEntries");

                            b1.WithOwner()
                                .HasForeignKey("OtpBlackListEntryId");
                        });

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("OtpBlackListEntryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("OtpBlackListEntryId");

                            b1.ToTable("OtpBlackListEntries");

                            b1.WithOwner()
                                .HasForeignKey("OtpBlackListEntryId");
                        });

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "ResolvedBy", b1 =>
                        {
                            b1.Property<Guid>("OtpBlackListEntryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("OtpBlackListEntryId");

                            b1.ToTable("OtpBlackListEntries");

                            b1.WithOwner()
                                .HasForeignKey("OtpBlackListEntryId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("Phone");

                    b.Navigation("PhoneConfiguration");

                    b.Navigation("ResolvedBy");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpBlackListEntryLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.OtpBlackListEntry", "BlackListEntry")
                        .WithMany("Logs")
                        .HasForeignKey("BlackListEntryId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("OtpBlackListEntryLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("OtpBlackListEntryLogId");

                            b1.ToTable("OtpBlackListEntryLog");

                            b1.WithOwner()
                                .HasForeignKey("OtpBlackListEntryLogId");
                        });

                    b.Navigation("BlackListEntry");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpOperatorException", b =>
                {
                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("OtpOperatorExceptionId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("OtpOperatorExceptionId");

                            b1.ToTable("OtpOperatorExceptions");

                            b1.WithOwner()
                                .HasForeignKey("OtpOperatorExceptionId");
                        });

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfiguration", b =>
                {
                    b.OwnsOne("bbt.gateway.messaging.Models.Phone", "Phone", b1 =>
                        {
                            b1.Property<Guid>("PhoneConfigurationId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("CountryCode")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Number")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Prefix")
                                .HasColumnType("INTEGER");

                            b1.HasKey("PhoneConfigurationId");

                            b1.ToTable("PhoneConfigurations");

                            b1.WithOwner()
                                .HasForeignKey("PhoneConfigurationId");
                        });

                    b.Navigation("Phone");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfigurationLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "Phone")
                        .WithMany("Logs")
                        .HasForeignKey("PhoneId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("PhoneConfigurationLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("PhoneConfigurationLogId");

                            b1.ToTable("PhoneConfigurationLog");

                            b1.WithOwner()
                                .HasForeignKey("PhoneConfigurationLogId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("Phone");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendOtpRequestLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "Phone")
                        .WithMany("OtpLogs")
                        .HasForeignKey("PhoneId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("SendOtpRequestLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("SendOtpRequestLogId");

                            b1.ToTable("SendOtpRequestLog");

                            b1.WithOwner()
                                .HasForeignKey("SendOtpRequestLogId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("Phone");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendOtpResponseLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.SendOtpRequestLog", null)
                        .WithMany("ResponseLogs")
                        .HasForeignKey("SendOtpRequestLogId");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendSmsLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "Phone")
                        .WithMany("SmsLogs")
                        .HasForeignKey("PhoneId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("SendSmsLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("SendSmsLogId");

                            b1.ToTable("SendSmsLog");

                            b1.WithOwner()
                                .HasForeignKey("SendSmsLogId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("Phone");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpBlackListEntry", b =>
                {
                    b.Navigation("Logs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfiguration", b =>
                {
                    b.Navigation("BlacklistEntries");

                    b.Navigation("Logs");

                    b.Navigation("OtpLogs");

                    b.Navigation("SmsLogs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SendOtpRequestLog", b =>
                {
                    b.Navigation("ResponseLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
