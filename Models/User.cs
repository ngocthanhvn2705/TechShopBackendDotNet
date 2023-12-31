﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechShopBackendDotnet.Models;

[Table("user")]
[MySqlCollation("utf8mb4_general_ci")]
public partial class User
{
    [Key]
    [Column("EMAIL")]
    public string Email { get; set; }

    [Required]
    [Column("NAME")]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    [Column("PASSWORD")]
    [StringLength(255)]
    public string Password { get; set; }

    [Required]
    [Column("PHONE")]
    [StringLength(255)]
    public string Phone { get; set; }

    [Required]
    [Column("GENDER")]
    [StringLength(50)]
    public string Gender { get; set; }

    [Column("BIRTHDAY")]
    public DateOnly Birthday { get; set; }

    [Required]
    [Column("ADDRESS")]
    [StringLength(255)]
    public string Address { get; set; }

    [Required]
    [Column("WARD")]
    [StringLength(255)]
    public string Ward { get; set; }

    [Required]
    [Column("DISTRICT")]
    [StringLength(255)]
    public string District { get; set; }

    [Required]
    [Column("CITY")]
    [StringLength(255)]
    public string City { get; set; }

    [Column("IMAGE", TypeName = "mediumblob")]
    public byte[] Image { get; set; }

    [Required]
    [Column("ROLE")]
    [StringLength(255)]
    public string Role { get; set; }

    [Required]
    [Column("STATUS")]
    [StringLength(255)]
    public string Status { get; set; }
}