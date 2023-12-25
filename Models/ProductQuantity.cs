﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TechShopBackendDotnet.Models;

[Table("product_quantity")]
[Index("ProductId", Name = "FK_product_quantity_PRODUCT_ID")]
[MySqlCollation("utf8mb4_general_ci")]
public partial class ProductQuantity
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Required]
    [Column("COLOR")]
    [StringLength(255)]
    public string Color { get; set; }

    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [Column("SOLD")]
    public int Sold { get; set; }
}