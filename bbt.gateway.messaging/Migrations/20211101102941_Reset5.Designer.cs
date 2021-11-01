﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using bbt.gateway.messaging;

#nullable disable

namespace bbt.gateway.messaging.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211101102941_Reset5")]
    partial class Reset5
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0-rc.2.21480.5");

            modelBuilder.Entity("bbt.gateway.messaging.Models.BlackListEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PhoneConfigurationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ResolvedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ValidTo")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneConfigurationId");

                    b.ToTable("BlackListEntries");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.BlackListEntryLog", b =>
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

                    b.ToTable("BlackListEntryLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.Header", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Branch")
                        .HasColumnType("INTEGER");

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

                    b.HasData(
                        new
                        {
                            Id = new Guid("7fd4c8cd-1dbf-4bb9-9cbf-4fadb0e4918a"),
                            ContentType = 0,
                            EmailTemplatePrefix = "generic",
                            SmsPrefix = "Dear Honey,",
                            SmsSender = "BATMAN",
                            SmsSuffix = ":)",
                            SmsTemplatePrefix = "generic"
                        },
                        new
                        {
                            Id = new Guid("d2991d8f-69e6-4dba-bb39-a5c40cdb3a25"),
                            Branch = 2000,
                            ContentType = 0,
                            EmailTemplatePrefix = "on",
                            SmsPrefix = "OBEY:",
                            SmsSender = "ZEUS",
                            SmsTemplatePrefix = "on"
                        });
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.Operator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ControlDaysForOtp")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseIvnWhenDeactive")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Operators");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ControlDaysForOtp = 60,
                            Status = 1,
                            Type = 1,
                            UseIvnWhenDeactive = false
                        },
                        new
                        {
                            Id = 2,
                            ControlDaysForOtp = 60,
                            Status = 1,
                            Type = 2,
                            UseIvnWhenDeactive = false
                        },
                        new
                        {
                            Id = 3,
                            ControlDaysForOtp = 60,
                            Status = 1,
                            Type = 3,
                            UseIvnWhenDeactive = false
                        },
                        new
                        {
                            Id = 4,
                            ControlDaysForOtp = 60,
                            Status = 1,
                            Type = 4,
                            UseIvnWhenDeactive = false
                        },
                        new
                        {
                            Id = 5,
                            ControlDaysForOtp = 60,
                            Status = 1,
                            Type = 5,
                            UseIvnWhenDeactive = false
                        });
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpRequestLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PhoneConfigurationId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneConfigurationId");

                    b.ToTable("OtpRequestLogs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpResponseLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Operator")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("OtpRequestLogId")
                        .HasColumnType("TEXT");

                    b.Property<int>("ResponseCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ResponseMessage")
                        .HasColumnType("TEXT");

                    b.Property<string>("StatusQueryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Topic")
                        .HasColumnType("TEXT");

                    b.Property<int>("TrackingStatus")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OtpRequestLogId");

                    b.ToTable("OtpResponseLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpTrackingLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Detail")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("LogId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("OtpResponseLogId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("QueriedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OtpResponseLogId");

                    b.ToTable("OtpTrackingLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfiguration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("$id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CustomerNo")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Operator")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("$id")
                        .IsUnique()
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

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

                    b.Property<Guid?>("PhoneId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RelatedId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneId");

                    b.ToTable("PhoneConfigurationLog");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.SmsLog", b =>
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

                    b.Property<Guid?>("PhoneConfigurationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PhoneConfigurationId");

                    b.ToTable("SmsLogs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.BlackListEntry", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "PhoneConfiguration")
                        .WithMany("BlacklistEntries")
                        .HasForeignKey("PhoneConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("BlackListEntryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("BlackListEntryId");

                            b1.ToTable("BlackListEntries");

                            b1.WithOwner()
                                .HasForeignKey("BlackListEntryId");
                        });

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "ResolvedBy", b1 =>
                        {
                            b1.Property<Guid>("BlackListEntryId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("BlackListEntryId");

                            b1.ToTable("BlackListEntries");

                            b1.WithOwner()
                                .HasForeignKey("BlackListEntryId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("PhoneConfiguration");

                    b.Navigation("ResolvedBy");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.BlackListEntryLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.BlackListEntry", "BlackListEntry")
                        .WithMany("Logs")
                        .HasForeignKey("BlackListEntryId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("BlackListEntryLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("BlackListEntryLogId");

                            b1.ToTable("BlackListEntryLog");

                            b1.WithOwner()
                                .HasForeignKey("BlackListEntryLogId");
                        });

                    b.Navigation("BlackListEntry");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpRequestLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "PhoneConfiguration")
                        .WithMany("OtpLogs")
                        .HasForeignKey("PhoneConfigurationId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("OtpRequestLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("OtpRequestLogId");

                            b1.ToTable("OtpRequestLogs");

                            b1.WithOwner()
                                .HasForeignKey("OtpRequestLogId");
                        });

                    b.OwnsOne("bbt.gateway.messaging.Models.Phone", "Phone", b1 =>
                        {
                            b1.Property<Guid>("OtpRequestLogId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("CountryCode")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Number")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Prefix")
                                .HasColumnType("INTEGER");

                            b1.HasKey("OtpRequestLogId");

                            b1.ToTable("OtpRequestLogs");

                            b1.WithOwner()
                                .HasForeignKey("OtpRequestLogId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("Phone");

                    b.Navigation("PhoneConfiguration");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpResponseLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.OtpRequestLog", null)
                        .WithMany("ResponseLogs")
                        .HasForeignKey("OtpRequestLogId");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpTrackingLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.OtpResponseLog", null)
                        .WithMany("TrackingLogs")
                        .HasForeignKey("OtpResponseLogId");
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

            modelBuilder.Entity("bbt.gateway.messaging.Models.SmsLog", b =>
                {
                    b.HasOne("bbt.gateway.messaging.Models.PhoneConfiguration", "PhoneConfiguration")
                        .WithMany("SmsLogs")
                        .HasForeignKey("PhoneConfigurationId");

                    b.OwnsOne("bbt.gateway.messaging.Models.Process", "CreatedBy", b1 =>
                        {
                            b1.Property<Guid>("SmsLogId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Action")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Identity")
                                .HasColumnType("TEXT");

                            b1.Property<string>("ItemId")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.HasKey("SmsLogId");

                            b1.ToTable("SmsLogs");

                            b1.WithOwner()
                                .HasForeignKey("SmsLogId");
                        });

                    b.Navigation("CreatedBy");

                    b.Navigation("PhoneConfiguration");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.BlackListEntry", b =>
                {
                    b.Navigation("Logs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpRequestLog", b =>
                {
                    b.Navigation("ResponseLogs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.OtpResponseLog", b =>
                {
                    b.Navigation("TrackingLogs");
                });

            modelBuilder.Entity("bbt.gateway.messaging.Models.PhoneConfiguration", b =>
                {
                    b.Navigation("BlacklistEntries");

                    b.Navigation("Logs");

                    b.Navigation("OtpLogs");

                    b.Navigation("SmsLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
