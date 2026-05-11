using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class MyDbContext : DbContext
    {
        private readonly ITenantContext? _tenantContext;

        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public MyDbContext(DbContextOptions<MyDbContext> options, ITenantContext tenantContext) : base(options)
        {
            _tenantContext = tenantContext;
        }

        /// <summary>
        /// Re-evaluated by EF Core on every query against an IStoreScoped entity.
        /// Returning null disables filtering (used for unauthenticated routes
        /// and design-time tools).
        /// </summary>
        public int? CurrentStoreId => _tenantContext?.StoreId;

        public DbSet<StoreModel> Stores { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<CustomerModel> Customers { get; set; }
        public DbSet<AddressesModel> Addresses { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; } 
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<VariantModel> Variants { get; set; }
        public DbSet<Options> Options { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<CollectionModel> Collection { get; set; }
        public DbSet<RulesModel> Rules { get; set; }
        public DbSet<CollectionImageModel> CollectionImages { get; set; }
        public DbSet<CollectionProduct> CollectionProducts { get; set; }
        public DbSet<OrdersModel> Orders { get; set; }
        public DbSet<LineItemsModel> LineItems { get; set; }
        public DbSet<TaxLinesModel> TaxLines { get; set; }
        public DbSet<PriceSetModel> priceSet { get; set; }
        public DbSet<SubTotalPriceModel> subtotalPrice { get; set; }
        public DbSet<CurrentTotalPriceModel> currentTotalPrice { get; set; }
        public DbSet<TotalDiscountModel> totalDiscount { get; set; }
        public DbSet<TotalTaxModel> totalTax { get; set; }
        public DbSet<TotalLineModel> totalLine { get; set; }
        public DbSet<TotalShippingModel> totalShipping { get; set; }
        public DbSet<FulfillmentsModel> fulfillments { get; set; }
        public DbSet<DiscountCodeModel> discount_code { get; set; }
        public DbSet<NoteAttributesModel> note_attributes { get; set; }
        public DbSet<DiscountApplicationsModel> discount_applications { get; set; }
        public DbSet<BillingAddressModel> billing_address { get; set; }
        public DbSet<ClientDetailsModel> client_details { get; set; }
        public DbSet<ShippingAddressModel> ShippingAddress { get; set; }
        public DbSet<RefundModel> Refund { get; set; }
        public DbSet<TransactionsModel> transactions { get; set; }
        public DbSet<RefundLineItemsModel> refund_line_items { get; set; }
        public DbSet<GiftCardModel> gift_card { get; set; }
        public DbSet<DistrictModel> district { get; set; }
        public DbSet<CityModel> city { get; set; }
        public DbSet<DiscountModel> discount_codes { get; set; }
        public DbSet<PriceRuleModel> price_rule { get; set; }
        public DbSet<EntitlementPurchaseModel> entitlement_purchase { get; set; }
        public DbSet<EntitlementQuantityModel> entitlement_quantity { get; set; }
        public DbSet<OrderFilterModel> order_filter { get; set; }
        public DbSet<OrderSortBy> order_sort_by { get; set; }
        public DbSet<ProductSortByModel> product_sort_by { get; set; }
        public DbSet<CustomerSortByModel> customer_sort_by { get; set; }
        public DbSet<InventorySortByModel> inventory_sort_by { get; set; }
        public DbSet<CollectionSortByModel> collection_sort_by { get; set; }
        public DbSet<GiftCardSortByModel> giftCard_sort_by { get; set; }
        public DbSet<ProductFilterModel> product_filter { get; set; }
        public DbSet<CollectionFilterModel> collection_filter { get; set; }
        public DbSet<GiftCardFilterModel> giftcard_filter { get; set; }
        public DbSet<SupplierModel> supplier { get; set; }
        public DbSet<ShippingLineModel> ShippingLines { get; set; }
        public DbSet<CartItem> cart { get; set; }
        public DbSet<BadgesModel> badges { get; set; }
        public DbSet<PermissionsModel> permissions { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<TimelineEventModel> TimelineEvents { get; set; }
        public DbSet<VariantAdjustmentHistory> VariantAdjustmentHistory { get; set; }
        public DbSet<InventoryTransactionLog> InventoryTransactionLog { get; set; }

        // PMI Integration tables
        public DbSet<PmiCustomer> PmiCustomers { get; set; }
        public DbSet<PmiOrder> PmiOrders { get; set; }
        public DbSet<PmiOrderedProduct> PmiOrderedProducts { get; set; }
        public DbSet<PmiError> PmiErrors { get; set; }
        public DbSet<PmiOrderedMachine> PmiOrderedMachines { get; set; }
        public DbSet<PmiProduct> PmiProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("YourConnectionString", new MySqlServerVersion(new Version(8, 0, 28)),
                    options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity names to lowercase table names (Linux is case-sensitive)
            modelBuilder.Entity<StoreModel>(entity =>
            {
                entity.ToTable("stores");
                entity.HasIndex(s => s.Slug).IsUnique().HasDatabaseName("UX_Stores_Slug");
                entity.HasIndex(s => s.Domain).HasDatabaseName("IX_Stores_Domain");
            });
            modelBuilder.Entity<UserModel>().ToTable("users");
            modelBuilder.Entity<CustomerModel>().ToTable("customers");
            modelBuilder.Entity<AddressesModel>().ToTable("addresses");
            modelBuilder.Entity<EmployeeModel>().ToTable("employees");
            modelBuilder.Entity<ProductModel>().ToTable("products");
            modelBuilder.Entity<VariantModel>().ToTable("variants");
            modelBuilder.Entity<Options>().ToTable("options");
            modelBuilder.Entity<ProductImages>().ToTable("productimages");
            modelBuilder.Entity<CollectionModel>().ToTable("smart_collections");
            modelBuilder.Entity<RulesModel>().ToTable("collection_rules");
            modelBuilder.Entity<CollectionImageModel>().ToTable("collection_images");
            modelBuilder.Entity<CollectionProduct>().ToTable("collection_products");
            modelBuilder.Entity<OrdersModel>().ToTable("orders");
            modelBuilder.Entity<LineItemsModel>().ToTable("lineitems");
            modelBuilder.Entity<TaxLinesModel>().ToTable("taxlines");
            modelBuilder.Entity<PriceSetModel>().ToTable("price_set");
            modelBuilder.Entity<SubTotalPriceModel>().ToTable("current_subtotal_price");
            modelBuilder.Entity<CurrentTotalPriceModel>().ToTable("current_total_price_set");
            modelBuilder.Entity<TotalDiscountModel>().ToTable("total_discount_set");
            modelBuilder.Entity<TotalTaxModel>().ToTable("total_tax_set");
            modelBuilder.Entity<TotalLineModel>().ToTable("total_line_items_price_set");
            modelBuilder.Entity<TotalShippingModel>().ToTable("total_shipping_price_set");
            modelBuilder.Entity<FulfillmentsModel>().ToTable("fulfillments");
            modelBuilder.Entity<DiscountCodeModel>().ToTable("discount_codes");
            modelBuilder.Entity<NoteAttributesModel>().ToTable("note_attributes");
            modelBuilder.Entity<DiscountApplicationsModel>().ToTable("discount_applications");
            modelBuilder.Entity<BillingAddressModel>().ToTable("billing_address");
            modelBuilder.Entity<ClientDetailsModel>().ToTable("client_details");
            modelBuilder.Entity<ShippingAddressModel>().ToTable("shipping_address");
            modelBuilder.Entity<RefundModel>().ToTable("refund");
            modelBuilder.Entity<TransactionsModel>().ToTable("transactions");
            modelBuilder.Entity<RefundLineItemsModel>().ToTable("refund_line_items");
            modelBuilder.Entity<GiftCardModel>().ToTable("gift_card");
            modelBuilder.Entity<DistrictModel>().ToTable("district");
            modelBuilder.Entity<CityModel>().ToTable("cities");
            // DiscountModel uses [Table("discount_code")] attribute
            modelBuilder.Entity<PriceRuleModel>().ToTable("price_rules");
            modelBuilder.Entity<EntitlementPurchaseModel>().ToTable("entitlement_purchase");
            modelBuilder.Entity<EntitlementQuantityModel>().ToTable("entitlement_quantity_ratio");
            modelBuilder.Entity<OrderFilterModel>().ToTable("order_filter");
            modelBuilder.Entity<OrderSortBy>().ToTable("order_sort_by");
            modelBuilder.Entity<ProductSortByModel>().ToTable("product_sort_by");
            modelBuilder.Entity<CustomerSortByModel>().ToTable("customer_sort_by");
            modelBuilder.Entity<InventorySortByModel>().ToTable("inventory_sort_by");
            modelBuilder.Entity<CollectionSortByModel>().ToTable("collection_sort_by");
            modelBuilder.Entity<GiftCardSortByModel>().ToTable("giftcard_sort_by");
            modelBuilder.Entity<ProductFilterModel>().ToTable("product_filter");
            modelBuilder.Entity<CollectionFilterModel>().ToTable("collection_filter");
            modelBuilder.Entity<GiftCardFilterModel>().ToTable("giftcard_filter");
            modelBuilder.Entity<SupplierModel>().ToTable("supplier");
            modelBuilder.Entity<ShippingLineModel>().ToTable("shipping_lines");
            modelBuilder.Entity<CartItem>().ToTable("cart");
            modelBuilder.Entity<BadgesModel>().ToTable("badges");
            modelBuilder.Entity<PermissionsModel>().ToTable("permissions");
            modelBuilder.Entity<CommentModel>().ToTable("comments");
            modelBuilder.Entity<TimelineEventModel>().ToTable("timeline_events");
            modelBuilder.Entity<VariantAdjustmentHistory>().ToTable("variant_adjustment_history");

            // PMI Integration table mappings
            modelBuilder.Entity<PmiCustomer>().ToTable("pmicustomers");
            modelBuilder.Entity<PmiOrder>().ToTable("pmiorders");
            modelBuilder.Entity<PmiOrderedProduct>().ToTable("pmiorderedproduct");
            modelBuilder.Entity<PmiError>().ToTable("pmierrors");
            modelBuilder.Entity<PmiOrderedMachine>().ToTable("pmiorderedmachines");
            modelBuilder.Entity<PmiProduct>().ToTable("pmiproducts");

            // Configure CustomerModel
            modelBuilder.Entity<CustomerModel>(entity =>
            {
                // Map the C# property names to the actual column names in the database
                entity.Property(entity=> entity.EmailSmsOptIn).HasColumnName("email_sms_opt_in");
                entity.Property(entity => entity.Newsletter).HasColumnName("news_letter");
                entity.Property(entity=> entity.CreatedAt).HasColumnName("created_at");
                entity.Property(entity=> entity.UpdatedAt).HasColumnName("updated_at");
                entity.Property(entity => entity.OrdersCount).HasColumnName("orders_count");
                entity.Property(entity => entity.TotalSpent).HasColumnName("total_spent");
                entity.Property(entity => entity.LastOrderName).HasColumnName("last_order_name");
                entity.Property(entity => entity.LastOrderId).HasColumnName("last_order_id");

            });

            modelBuilder.Entity<AddressesModel>(entity =>
            {
                entity.Property(entity => entity.CountryCode).HasColumnName("country_code");
                entity.Property(entity => entity.ProvinceCode).HasColumnName("province_code");
                entity.Property(entity => entity.CountryName).HasColumnName("country_name");
                entity.Property(entity => entity.CustomerId).HasColumnName("customer_id");
                entity.Property(entity => entity.Default).HasColumnName("default_address");
            });

            modelBuilder.Entity<EmployeeModel>(entity =>
            {
                entity.Property(entity => entity.AccessControl).HasColumnName("access_control");
            });

            modelBuilder.Entity<ProductImages>(entity =>
            {
                entity.Property(entity => entity.variant_id).HasColumnName("variantId");
            });

            modelBuilder.Entity<ProductDTO>(entity =>
            {
                // Remove any JSON converter and ensure Status is a plain string
                entity.Property(e => e.status)
                    .HasConversion<string>(); // This tells EF Core to treat it as plain text

                entity.Property(e => e.product_status)
                    .HasConversion<string>(); // Similarly for product_status
            });

            // Configure VariantModel properties
            modelBuilder.Entity<VariantModel>(entity =>
            {
                // Configure auto-increment for id
                entity.HasKey(e => e.id);
                entity.Property(e => e.id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.inventory_policy)
                    .HasConversion<string>();

                entity.Property(e => e.option1)
                    .HasConversion<string>();
            });

            modelBuilder.Entity<UserModel>()
                .HasOne<CustomerModel>(user => user.Customer)
                .WithOne(customer => customer.User)
                .HasForeignKey<CustomerModel>(customer => customer.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerModel>()
                .HasMany<AddressesModel>(customer => customer.Addresses)
                .WithOne(address => address.Customer)
                .HasForeignKey(address => address.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasOne<EmployeeModel>(user => user.Employee)
                .WithOne(employee => employee.User)
                .HasForeignKey<EmployeeModel>(employee => employee.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductModel>()
                .HasMany(product => product.Variants)
                .WithOne()
                .HasForeignKey(variant => variant.product_id);

            modelBuilder.Entity<ProductModel>()
                .HasMany(product => product.Options)
                .WithOne()
                .HasForeignKey(option => option.product_id);
            
            modelBuilder.Entity<ProductModel>()
                .HasMany(product => product.ProductImages)
                .WithOne()
                .HasForeignKey (productimage => productimage.product_id);

            // Add indexes for ProductModel to improve query performance
            // Note: For MySQL TEXT columns, we only index the first 255 characters

            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.created_at)
                .HasDatabaseName("IX_Products_CreatedAt");

            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.updated_at)
                .HasDatabaseName("IX_Products_UpdatedAt");

            // Add index on Variants for inventory queries
            modelBuilder.Entity<VariantModel>()
                .HasIndex(v => new { v.product_id, v.inventory_quantity })
                .HasDatabaseName("IX_Variants_ProductId_Inventory");

            modelBuilder.Entity<VariantModel>()
                .HasMany(variant => variant.ProductImages)
                .WithOne()
                .HasForeignKey(variantimage => variantimage.variant_id);

            modelBuilder.Entity<CollectionModel>()
                .HasMany(collection => collection.Rules)
                .WithOne()
                .HasForeignKey(rules => rules.smart_collection_id);

            modelBuilder.Entity<CollectionModel>()
                .HasMany(collection => collection.CollectionImages)
                .WithOne()
                .HasForeignKey(images => images.smart_collection_id);

            modelBuilder.Entity<CollectionModel>()
                .HasMany(collection => collection.Products)
                .WithOne()
                .HasForeignKey(product => product.smart_collection_id);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.LineItems)
                .WithOne()
                .HasForeignKey(lineItem => lineItem.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.taxLines)
                .WithOne()
                .HasForeignKey(tax => tax.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.priceSet)
                .WithOne()
                .HasForeignKey(price => price.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.subtotal_price_set)
                .WithOne()
                .HasForeignKey(subtotal => subtotal.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.CurrentTotalPrice)
                .WithOne()
                .HasForeignKey(totalPrice => totalPrice.orderid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.TotalDiscount)
                .WithOne()
                .HasForeignKey(totalDiscount => totalDiscount.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.TotalTax)
                .WithOne()
                .HasForeignKey(totalTax => totalTax.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.LineModels)
                .WithOne()
                .HasForeignKey(lineModel => lineModel.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.totalShipping)
                .WithOne()
                .HasForeignKey(totalShipping => totalShipping.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.fulfillment)
                .WithOne()
                .HasForeignKey(fulfillment => fulfillment.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.discount_code)
                .WithOne()
                .HasForeignKey(discount_code => discount_code.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.note_attributes)
                .WithOne()
                .HasForeignKey(note_attributes => note_attributes.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.discount_applications)
                .WithOne()
                .HasForeignKey(discount_applications => discount_applications.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.billing_address)
                .WithOne()
                .HasForeignKey(billing_address => billing_address.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.ShippingLines)
                .WithOne()
                .HasForeignKey(shipping_lines => shipping_lines.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.client_details)
                .WithOne()
                .HasForeignKey(client_details => client_details.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.ShippingAddress)
                .WithOne()
                .HasForeignKey(shipping => shipping.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.Refunds)
                .WithOne()
                .HasForeignKey(refund => refund.orderid);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.transactions)
                .WithOne()
                .HasForeignKey(transactions => transactions.orderid);

            modelBuilder.Entity<DistrictModel>()
                .HasMany(district => district.city)
                .WithOne()
                .HasForeignKey(city => city.district_id);

            // Transaction → RefundModel: configure from the child side to prevent
            // EF Core convention creating a duplicate shadow FK "RefundModelid"
            modelBuilder.Entity<TransactionsModel>()
                .HasOne<RefundModel>()
                .WithMany(refund => refund.Transaction)
                .HasForeignKey(transaction => transaction.refund_id)
                .IsRequired(false);

            // RefundLine ↔ RefundModel: single bidirectional configuration
            modelBuilder.Entity<RefundLineItemsModel>()
                .HasOne(refundLine => refundLine.Refund)
                .WithMany(refund => refund.RefundLine)
                .HasForeignKey(refundLine => refundLine.refund_id);

            // RefundLine → LineItem: single configuration
            modelBuilder.Entity<RefundLineItemsModel>()
                .HasOne(refundLine => refundLine.LineItem)
                .WithMany(lineItem => lineItem.RefundLineItems)
                .HasForeignKey(refundLine => refundLine.line_item_id);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.LineItems)
                .WithOne()
                .HasForeignKey(lineItem => lineItem.orderid);

            modelBuilder.Entity<PriceRuleModel>()
                .HasMany(price => price.discount)
                .WithOne()
                .HasForeignKey(discount => discount.price_rule_id);

            modelBuilder.Entity<PriceRuleModel>()
                .HasMany(price => price.entitlement_purchase)
                .WithOne()
                .HasForeignKey(purchase => purchase.price_rule_id);

            modelBuilder.Entity<PriceRuleModel>()
                .HasMany(price => price.entitlement_quantity)
                .WithOne()
                .HasForeignKey(quantity => quantity.price_rule_id);

            modelBuilder.Entity<OrdersModel>()
                .HasMany(order => order.Comments)
                .WithOne(comment => comment.Order)
                .HasForeignKey(comment => comment.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasMany(user => user.Comments)
                .WithOne(comment => comment.User)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TimelineEvent relationships
            modelBuilder.Entity<TimelineEventModel>(entity =>
            {
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(timeline => timeline.Order)
                    .WithMany()
                    .HasForeignKey(timeline => timeline.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(timeline => timeline.User)
                    .WithMany()
                    .HasForeignKey(timeline => timeline.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure VariantAdjustmentHistory relationships
            modelBuilder.Entity<VariantAdjustmentHistory>(entity =>
            {
                entity.HasIndex(e => e.VariantId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ActivityType);

                entity.HasOne(adj => adj.Variant)
                    .WithMany()
                    .HasForeignKey(adj => adj.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(adj => adj.Product)
                    .WithMany()
                    .HasForeignKey(adj => adj.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            //modelBuilder.Entity<Carrier>()
            //    .HasMany(carrier => carrier.ShippingMethods)
            //    .WithOne()
            //    .HasForeignKey(shipping => shipping.carrier_uuid);

            //modelBuilder.Entity<ShippingMethod>()
            //    .HasMany(shipping => shipping.BaseFee)
            //    .WithOne()
            //    .HasForeignKey(fee => fee.shipping_method_uuid);

            // PMI Integration relationships
            modelBuilder.Entity<PmiOrder>(entity =>
            {
                entity.HasKey(e => e.OrderReference);
                entity.Property(e => e.OrderReference).HasColumnName("orderReference");
                entity.Property(e => e.OrderNumber).HasColumnName("orderNumber");

                // DB columns are varchar(45) but C# types are DateTime?/long?
                // Add value conversions to handle the type mismatch
                entity.Property(e => e.DateDelivered)
                    .HasColumnName("dateDelivered")
                    .HasColumnType("varchar(45)")
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        v => !string.IsNullOrEmpty(v) ? DateTime.Parse(v) : (DateTime?)null
                    );

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customerId")
                    .HasColumnType("varchar(45)")
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                        v => !string.IsNullOrEmpty(v) ? long.Parse(v) : (long?)null
                    );

                entity.Property(e => e.DateCreated)
                    .HasColumnName("dateCreated")
                    .HasColumnType("varchar(45)")
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        v => !string.IsNullOrEmpty(v) ? DateTime.Parse(v) : (DateTime?)null
                    );

                entity.Property(e => e.ErrorId).HasColumnName("errorId");
                entity.Property(e => e.Anonymous).HasColumnName("anonymous");

                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(o => o.Error)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(o => o.ErrorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PmiOrderedProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).HasColumnName("orderId");
                // DB: productId is varchar(45), C#: long
                entity.Property(e => e.ProductId)
                    .HasColumnName("productId")
                    .HasColumnType("varchar(45)")
                    .HasConversion(
                        v => v.ToString(),
                        v => string.IsNullOrEmpty(v) ? 0L : long.Parse(v)
                    );
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                // DB: price is int, C#: decimal?
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("int")
                    .HasConversion(
                        v => v.HasValue ? (int)v.Value : (int?)null,
                        v => v.HasValue ? (decimal)v.Value : (decimal?)null
                    );

                entity.HasOne(p => p.Order)
                    .WithMany(o => o.OrderedProducts)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PmiOrderedMachine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).HasColumnName("orderId");
                entity.Property(e => e.SerialNum).HasColumnName("serialNum");

                entity.HasOne(m => m.Order)
                    .WithMany(o => o.OrderedMachines)
                    .HasForeignKey(m => m.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PmiCustomer>(entity =>
            {
                entity.HasKey(e => e.Id);
                // DB: id is varchar(45), C#: long
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("varchar(45)")
                    .HasConversion(
                        v => v.ToString(),
                        v => string.IsNullOrEmpty(v) ? 0L : long.Parse(v)
                    );
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.LastName).HasColumnName("lastName");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Email).HasColumnName("email");
            });

            modelBuilder.Entity<PmiError>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Error).HasColumnName("error");
            });

            modelBuilder.Entity<PmiProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                // DB: id is varchar(55), C#: long
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("varchar(55)")
                    .HasConversion(
                        v => v.ToString(),
                        v => string.IsNullOrEmpty(v) ? 0L : long.Parse(v)
                    );
                entity.Property(e => e.Name).HasColumnName("name");
                // DB: price is double, C#: decimal?
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("double")
                    .HasConversion(
                        v => v.HasValue ? (double)v.Value : (double?)null,
                        v => v.HasValue ? (decimal)v.Value : (decimal?)null
                    );
            });

            // === Multi-tenancy: Store FK relationships ===
            // Every IStoreScoped entity has a non-nullable StoreId referencing stores.Id.
            // OnDelete=Restrict so a store can't be deleted while it owns data.
            ConfigureStoreScope<CustomerModel>(modelBuilder, "IX_customers_store_id");
            ConfigureStoreScope<EmployeeModel>(modelBuilder, "IX_employees_store_id");
            ConfigureStoreScope<ProductModel>(modelBuilder, "IX_products_store_id");
            ConfigureStoreScope<CollectionModel>(modelBuilder, "IX_smart_collections_store_id");
            ConfigureStoreScope<OrdersModel>(modelBuilder, "IX_orders_store_id");
            ConfigureStoreScope<RefundModel>(modelBuilder, "IX_refund_store_id");
            ConfigureStoreScope<GiftCardModel>(modelBuilder, "IX_gift_card_store_id");
            ConfigureStoreScope<DiscountModel>(modelBuilder, "IX_discount_code_store_id");
            ConfigureStoreScope<PriceRuleModel>(modelBuilder, "IX_price_rules_store_id");
            ConfigureStoreScope<CartItem>(modelBuilder, "IX_cart_store_id");
            ConfigureStoreScope<BadgesModel>(modelBuilder, "IX_badges_store_id");
            ConfigureStoreScope<CommentModel>(modelBuilder, "IX_comments_store_id");
            ConfigureStoreScope<TimelineEventModel>(modelBuilder, "IX_timeline_events_store_id");
            ConfigureStoreScope<VariantAdjustmentHistory>(modelBuilder, "IX_variant_adjustment_history_store_id");
            ConfigureStoreScope<InventoryTransactionLog>(modelBuilder, "IX_inventory_transaction_log_store_id");
            ConfigureStoreScope<SupplierModel>(modelBuilder, "IX_supplier_store_id");
            ConfigureStoreScope<PmiCustomer>(modelBuilder, "IX_pmicustomers_store_id");
            ConfigureStoreScope<PmiOrder>(modelBuilder, "IX_pmiorders_store_id");
            ConfigureStoreScope<PmiProduct>(modelBuilder, "IX_pmiproducts_store_id");
        }

        private void ConfigureStoreScope<TEntity>(ModelBuilder modelBuilder, string indexName)
            where TEntity : class, IStoreScoped
        {
            modelBuilder.Entity<TEntity>()
                .HasOne<StoreModel>()
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TEntity>()
                .HasIndex(e => e.StoreId)
                .HasDatabaseName(indexName);

            // Auto-scope every read against this entity to the current tenant.
            // CurrentStoreId == null disables the filter for unauthenticated routes
            // and design-time tools, preserving legacy behavior where applicable.
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => CurrentStoreId == null || e.StoreId == CurrentStoreId);
        }

        public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyTenantOnInsert();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyTenantOnInsert();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// For every IStoreScoped entity being inserted with an unset StoreId (0),
        /// stamp it with the current tenant. Callers that pass an explicit StoreId
        /// keep that value (used by admin tools that operate across stores).
        /// </summary>
        private void ApplyTenantOnInsert()
        {
            var storeId = CurrentStoreId;
            if (storeId is null) return;

            foreach (var entry in ChangeTracker.Entries<IStoreScoped>())
            {
                if (entry.State == EntityState.Added && entry.Entity.StoreId <= 0)
                {
                    entry.Entity.StoreId = storeId.Value;
                }
            }
        }

    }
}
