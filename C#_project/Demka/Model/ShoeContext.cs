using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Demka.Model;

public partial class ShoeContext : DbContext
{
    public static ShoeContext _context;
    public static ShoeContext Context
    {
        get {
            if (_context == null) {
                _context = new ShoeContext();
            }
            return _context;
        }
    }
    public ShoeContext()
    {
    }

    public ShoeContext(DbContextOptions<ShoeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<ItemType> ItemTypes { get; set; }

    public virtual DbSet<Manufacture> Manufactures { get; set; }

    public virtual DbSet<OrderImport> OrderImports { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PickupPoint> PickupPoints { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<UserImport> UserImports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=shoe;user=root;password=admin", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.39-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ArticuleItem).HasName("PRIMARY");

            entity.ToTable("items");

            entity.HasIndex(e => e.CategoryItem, "fk_category_item_categories_idx");

            entity.HasIndex(e => e.IdManufactureItem, "fk_manufacture_manufactures_idx");

            entity.HasIndex(e => e.IdSupplierItem, "fk_supplier_suppliers_idx");

            entity.HasIndex(e => e.TypeItem, "fk_type_item_types_idx");

            entity.Property(e => e.ArticuleItem)
                .HasMaxLength(6)
                .HasColumnName("articule_item");
            entity.Property(e => e.CategoryItem).HasColumnName("category_item");
            entity.Property(e => e.CountInStorageItem).HasColumnName("count_in_storage_item");
            entity.Property(e => e.DescriptionItem).HasColumnName("description_item");
            entity.Property(e => e.DiscountItem).HasColumnName("discount_item");
            entity.Property(e => e.IdManufactureItem).HasColumnName("id_manufacture_item");
            entity.Property(e => e.IdSupplierItem).HasColumnName("id_supplier_item");
            entity.Property(e => e.MeasurementItem)
                .HasColumnType("enum('шт.')")
                .HasColumnName("measurement_item");
            entity.Property(e => e.PhotoItem).HasColumnName("photo_item");
            entity.Property(e => e.PriceItem)
                .HasPrecision(7, 2)
                .HasColumnName("price_item");
            entity.Property(e => e.TypeItem).HasColumnName("type_item");

            entity.HasOne(d => d.CategoryItemNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.CategoryItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_category_item_categories");

            entity.HasOne(d => d.IdManufactureItemNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdManufactureItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_manufacture_manufactures");

            entity.HasOne(d => d.IdSupplierItemNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdSupplierItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_supplier_suppliers");

            entity.HasOne(d => d.TypeItemNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.TypeItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_type_item_types");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.IdItemCategory).HasName("PRIMARY");

            entity.ToTable("item_categories");

            entity.Property(e => e.IdItemCategory).HasColumnName("id_item_category");
            entity.Property(e => e.NameItemCategory)
                .HasMaxLength(100)
                .HasColumnName("name_item_category");
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.HasKey(e => e.IdItemType).HasName("PRIMARY");

            entity.ToTable("item_types");

            entity.Property(e => e.IdItemType).HasColumnName("id_item_type");
            entity.Property(e => e.NameItemType)
                .HasMaxLength(100)
                .HasColumnName("name_item_type");
        });

        modelBuilder.Entity<Manufacture>(entity =>
        {
            entity.HasKey(e => e.IdManufacture).HasName("PRIMARY");

            entity.ToTable("manufactures");

            entity.Property(e => e.IdManufacture).HasColumnName("id_manufacture");
            entity.Property(e => e.NameManufacture)
                .HasMaxLength(100)
                .HasColumnName("name_manufacture");
        });

        modelBuilder.Entity<OrderImport>(entity =>
        {
            entity.HasKey(e => e.IdOrderImport).HasName("PRIMARY");

            entity.ToTable("order_imports");

            entity.HasIndex(e => e.IdClientOrderImport, "fk_id_client_user_imports_idx");

            entity.HasIndex(e => e.IdPickupPointOrderImport, "fk_id_pickup_points_idx");

            entity.Property(e => e.IdOrderImport).HasColumnName("id_order_import");
            entity.Property(e => e.CodeOrderImport)
                .HasMaxLength(3)
                .HasColumnName("code_order_import");
            entity.Property(e => e.DateOfDeliveryOrderImport).HasColumnName("date_of_delivery_order_import");
            entity.Property(e => e.DateOrderImport).HasColumnName("date_order_import");
            entity.Property(e => e.IdClientOrderImport).HasColumnName("id_client_order_import");
            entity.Property(e => e.IdPickupPointOrderImport).HasColumnName("id_pickup_point_order_import");
            entity.Property(e => e.StatusOrderImport)
                .HasColumnType("enum('Завершен','Новый')")
                .HasColumnName("status_order_import");

            entity.HasOne(d => d.IdClientOrderImportNavigation).WithMany(p => p.OrderImports)
                .HasForeignKey(d => d.IdClientOrderImport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_id_client_user_imports");

            entity.HasOne(d => d.IdPickupPointOrderImportNavigation).WithMany(p => p.OrderImports)
                .HasForeignKey(d => d.IdPickupPointOrderImport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_id_pickup_points");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.IdOrderItem, e.ArticulItem })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("order_items");

            entity.HasIndex(e => e.ArticulItem, "fk_articul_item_items_idx");

            entity.Property(e => e.IdOrderItem).HasColumnName("id_order_item");
            entity.Property(e => e.ArticulItem)
                .HasMaxLength(6)
                .HasColumnName("articul_item");
            entity.Property(e => e.CountItem).HasColumnName("count_item");

            entity.HasOne(d => d.ArticulItemNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ArticulItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_articul_item_items");

            entity.HasOne(d => d.IdOrderItemNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdOrderItem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_id_order_order_imports");
        });

        modelBuilder.Entity<PickupPoint>(entity =>
        {
            entity.HasKey(e => e.IdPickupPoint).HasName("PRIMARY");

            entity.ToTable("pickup_points");

            entity.Property(e => e.IdPickupPoint).HasColumnName("id_pickup_point");
            entity.Property(e => e.CityPickupPoint).HasColumnName("city_pickup_point");
            entity.Property(e => e.DistrictPickupPoint).HasColumnName("district_pickup_point");
            entity.Property(e => e.IndexPickupPoint).HasColumnName("index_pickup_point");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier).HasName("PRIMARY");

            entity.ToTable("suppliers");

            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.NameSupplier)
                .HasMaxLength(100)
                .HasColumnName("name_supplier");
        });

        modelBuilder.Entity<UserImport>(entity =>
        {
            entity.HasKey(e => e.IdUserImport).HasName("PRIMARY");

            entity.ToTable("user_imports");

            entity.Property(e => e.IdUserImport).HasColumnName("id_user_import");
            entity.Property(e => e.LastnameUserImport)
                .HasMaxLength(100)
                .HasColumnName("lastname_user_import");
            entity.Property(e => e.LoginUserImport)
                .HasMaxLength(100)
                .HasColumnName("login_user_import");
            entity.Property(e => e.NameUserImport)
                .HasMaxLength(100)
                .HasColumnName("name_user_import");
            entity.Property(e => e.PasswordUserImport)
                .HasMaxLength(45)
                .HasColumnName("password_user_import");
            entity.Property(e => e.RoleUserImport)
                .HasColumnType("enum('Администратор','Менеджер','Авторизированный клиент')")
                .HasColumnName("role_user_import");
            entity.Property(e => e.SurnameUserImport)
                .HasMaxLength(100)
                .HasColumnName("surname_user_import");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
