using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PosApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Aron Mini Mart POS API",
        Version     = "v1",
        Description = "REST API for the Aron Mini Mart Point of Sale system. " +
                      "Provides menu items, categories, and order management endpoints " +
                      "consumed by the Android POS app.",
        Contact = new OpenApiContact
        {
            Name  = "Aron Mini Mart",
            Email = "admin@aronminimart.com"
        }
    });

    // Include XML comments for endpoint descriptions
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    options.DocInclusionPredicate((_, _) => true);
});

// ─── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ─── Middleware ────────────────────────────────────────────────────────────────
app.UseCors("AllowAll");

// Swagger available in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aron POS API v1");
    options.RoutePrefix = "swagger";          // http://localhost:5000/swagger
    options.DocumentTitle = "Aron POS API";
    options.DefaultModelsExpandDepth(-1);     // hide schemas section by default
    options.DisplayRequestDuration();
});

// Removed UseHttpsRedirection — Android emulator uses plain HTTP on port 5000
app.UseAuthorization();
app.MapControllers();

// Redirect root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// ─── Initialize Database ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();

        // Check both MenuCategories (from Admin app) and Orders (from POS API)
        cmd.CommandText = @"
            SELECT 
                SUM(CASE WHEN TABLE_NAME = 'MenuCategories' THEN 1 ELSE 0 END) AS HasMenu,
                SUM(CASE WHEN TABLE_NAME = 'Orders'         THEN 1 ELSE 0 END) AS HasOrders,
                SUM(CASE WHEN TABLE_NAME = 'OrderItems'     THEN 1 ELSE 0 END) AS HasOrderItems
            FROM INFORMATION_SCHEMA.TABLES";

        int hasMenu = 0, hasOrders = 0, hasOrderItems = 0;
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                hasMenu       = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                hasOrders     = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                hasOrderItems = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            }
        }

        await conn.CloseAsync();

        logger.LogInformation(
            "DB check — MenuCategories:{HasMenu} Orders:{HasOrders} OrderItems:{HasOrderItems}",
            hasMenu, hasOrders, hasOrderItems);

        if (hasOrders == 0 || hasOrderItems == 0)
        {
            logger.LogInformation("Orders/OrderItems tables missing. Creating them now...");

            var createConn = context.Database.GetDbConnection();
            if (createConn.State != System.Data.ConnectionState.Open)
                await createConn.OpenAsync();

            // Create Orders table if missing
            if (hasOrders == 0)
            {
                using var createCmd = createConn.CreateCommand();
                createCmd.CommandText = @"
                    CREATE TABLE [Orders] (
                        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [OrderNumber] NVARCHAR(MAX)     NOT NULL,
                        [OrderDate]   DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
                        [SubTotal]    DECIMAL(10,2)     NOT NULL,
                        [CashGiven]   DECIMAL(10,2)     NOT NULL,
                        [Change]      DECIMAL(10,2)     NOT NULL,
                        [Status]      NVARCHAR(MAX)     NOT NULL DEFAULT 'Completed',
                        [CashierName] NVARCHAR(MAX)     NULL
                    )";
                await createCmd.ExecuteNonQueryAsync();
                logger.LogInformation("Orders table created.");
            }

            // Create OrderItems table if missing
            if (hasOrderItems == 0)
            {
                using var createCmd = createConn.CreateCommand();
                createCmd.CommandText = @"
                    CREATE TABLE [OrderItems] (
                        [Id]         INT IDENTITY(1,1)  NOT NULL PRIMARY KEY,
                        [OrderId]    INT                NOT NULL,
                        [MenuItemId] INT                NOT NULL,
                        [ItemName]   NVARCHAR(MAX)      NOT NULL,
                        [UnitPrice]  DECIMAL(10,2)      NOT NULL,
                        [Quantity]   INT                NOT NULL,
                        [LineTotal]  DECIMAL(10,2)      NOT NULL,
                        CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId])
                            REFERENCES [Orders]([Id]) ON DELETE CASCADE
                    )";
                await createCmd.ExecuteNonQueryAsync();
                logger.LogInformation("OrderItems table created.");
            }

            await createConn.CloseAsync();
            logger.LogInformation("POS API tables created successfully.");
        }
        else
        {
            logger.LogInformation("All required tables exist. Skipping schema creation.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing POS API database.");
        throw;
    }
}

app.Run();
