using Backend.Auth;
using Backend.Data;
using Backend.Hubs;
using Backend.Interfaces;
using Backend.Middleware;
using Backend.Models;
using Backend.Repositories;
using Backend.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mysqlOptions => {
            mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            mysqlOptions.CommandTimeout(300); // 5 minutes timeout - TEMPORARY until database indexes are added
        }
    ));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHttpClient();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<FleetRunnrOptions>(builder.Configuration.GetSection("FleetRunnr"));
builder.Services.AddScoped<FleetRunnrService>();

// PMI/Odoo Integration services
builder.Services.Configure<OdooOptions>(builder.Configuration.GetSection("Odoo"));
builder.Services.AddScoped<IPmiServices, PmiServicesRepository>();
builder.Services.AddHttpClient<IOdooXmlRpcService, OdooXmlRpcService>();

//builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<IUserServices, UserServicesRepository>();
builder.Services.AddScoped<IProductServices, ProductServicesRepository>();
builder.Services.AddScoped<IAddressServices, AddressServicesRepository>();
builder.Services.AddScoped<ICollectionServices, CollectionServicesRepository>();
builder.Services.AddScoped<IOrdersServices, OrdersServicesRepository>();
builder.Services.AddScoped<IRefundServices, RefundServiceRepository>();
builder.Services.AddScoped<IGiftCardServices, GiftCardRepository>();
builder.Services.AddScoped<IMapServices, MapServicesRepository>();
builder.Services.AddScoped<IDiscountServices, DiscountServicesRepository>();
builder.Services.AddScoped<IPriceRuleServices, PriceRuleServicesRepository>();
builder.Services.AddScoped<IOrderFilterServices, OrderFilterServicesRepository>();
builder.Services.AddScoped<IProductFilterService, ProductFilterServiceRepository>();
builder.Services.AddScoped<ICollectionFilterServices, CollectionFilterServicesRepository>();
builder.Services.AddScoped<IPurchaseOrderServices, PurchaseOrderRepository>();
builder.Services.AddScoped<IInventoryServices, InventoryServicesRepository>();
builder.Services.AddScoped<IInventoryReservationService, InventoryReservationServiceRepository>();
builder.Services.AddScoped<IBadgesService, BadgesServicesRepository>();
builder.Services.AddScoped<IPermissionServices, PermissionServicesRepository>();
builder.Services.AddScoped<IGiftCardFilterServices, GiftCardFilterServicesRepository>();
builder.Services.AddScoped<ICommentServices, CommentServicesRepository>();
builder.Services.AddScoped<ITimelineServices, TimelineServicesRepository>();
builder.Services.AddScoped<ISearchServices, SearchServicesRepository>();
builder.Services.AddScoped<IVariantAdjustmentService, VariantAdjustmentServiceRepository>();

// Multi-tenancy: per-request context populated by TenantMiddleware below.
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Local JWT issuance (replaces Cognito-issued tokens). Validation is still
// wired against Cognito below; that swap happens in a follow-up commit.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                                    "http://localhost:3000",
                                    "http://63.181.148.96",
                                    "http://3.68.5.250:5174",
                                    "http://localhost:5173",
                                    "http://63.178.93.153:3000"
                                )
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://cognito-idp.eu-north-1.amazonaws.com/eu-north-1_ZtzhnBriZ";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://cognito-idp.eu-north-1.amazonaws.com/eu-north-1_ZtzhnBriZ", // Cognito domain URL
        ValidateAudience = true,
        ValidAudience = "1i2j03gs7virgvb8gdjeig3esl", // Cognito App Client ID
        ValidateLifetime = true,
    };

    // Add logging for token validation events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully for user: " + (context.Principal?.Identity?.Name ?? "Unknown"));
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products")),
    RequestPath = "/uploads/products"
});

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var origin = context.Request.Headers["Origin"].ToString();
        string[] allowedOrigins = [
            "http://localhost:3000",
            "http://63.181.148.96",
            "http://3.68.5.250:5174",
            "http://localhost:5173",
            "http://63.178.93.153:3000"
        ];

        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
});

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseWebSockets();

app.UseAuthentication();
app.UseAuthorization();

// Multi-tenancy: resolves the current store from X-Store-Id header or JWT
// "store_id" claim, populating the scoped ITenantContext. Must run AFTER
// UseAuthentication so claims are available.
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();
