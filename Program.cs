using Dapper;

using HRIStwinbeeAPI.ConnectionData;
using HRIStwinbeeAPI.Parsers;
using HRIStwinbeeAPI.Repo;
using HRIStwinbeeAPI.Repo.AssetAcceptance;
using HRIStwinbeeAPI.Repo.Depreciation;
using HRIStwinbeeAPI.Repo.FuelAcceptance;
using HRIStwinbeeAPI.Repo.ServiceAcceptance;
using HRIStwinbeeAPI.Repo.Interface;
using HRIStwinbeeAPI.Repo.Interface.AssetAcceptance;
using HRIStwinbeeAPI.Repo.Interface.Depreciation;
using HRIStwinbeeAPI.Repo.Interface.FuelAcceptance;
using HRIStwinbeeAPI.Repo.Interface.ServiceAcceptance;
using HRIStwinbeeAPI.Repo.Interface.ItemMaster;
using HRIStwinbeeAPI.Repo.Interface.ItemWarehouse;
using HRIStwinbeeAPI.Repo.Interface.PO;
using HRIStwinbeeAPI.Repo.Interface.Receiving;
using HRIStwinbeeAPI.Repo.Interface.MaterialIssuance;
using HRIStwinbeeAPI.Repo.Interface.Suppliers;
using HRIStwinbeeAPI.Repo.ItemMaster;
using HRIStwinbeeAPI.Repo.ItemWarehouse;
using HRIStwinbeeAPI.Repo.PO;
using HRIStwinbeeAPI.Repo.Receiving;
using HRIStwinbeeAPI.Repo.MaterialIssuance;
using HRIStwinbeeAPI.Repo.Interface.StockTransfer;
using HRIStwinbeeAPI.Repo.Interface.StockItemInquiry;
using HRIStwinbeeAPI.Repo.StockTransfer;
using HRIStwinbeeAPI.Repo.StockItemInquiry;
using HRIStwinbeeAPI.Repo.Suppliers;
using HRIStwinbeeAPI.Services.Suppliers;
using HRIStwinbeeAPI.Services.Depreciation;
using HRIStwinbeeAPI.Services.PO;
using HRIStwinbeeAPI.Services.StockItemInquiry;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Data;
using System.IO.Compression;
using System.Net;
using System.Security.Claims;

using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.Engine;

using twbERRANDSapi.AUTH;


var builder = WebApplication.CreateBuilder(args);


// ============================================================
// TELERIK SQL DATABASE PROVIDER
// ============================================================

// Required when the Telerik report uses Microsoft.Data.SqlClient.
//
// Important:
// The provider name inside the Telerik report configuration must be:
//
// Microsoft.Data.SqlClient
Telerik.Reporting.Processing.Data.DbProviderFactories.RegisterFactory(
    "Microsoft.Data.SqlClient",
    SqlClientFactory.Instance
);


// ============================================================
// HOSTING AND CORS CONFIGURATION
// MODIFIED:
// 1. Reads HTTP/HTTPS behavior from appsettings.json.
// 2. Sanitizes the configured CORS origins.
// 3. Avoids HTTP Error 500.30 when the CORS list is accidentally
//    missing in appsettings.Production.json.
// ============================================================

const string CorsPolicyName = "INVENTORY";

var useHttpsRedirection =
    builder.Configuration.GetValue<bool>(
        "Hosting:UseHttpsRedirection",
        false
    );

var useHsts =
    builder.Configuration.GetValue<bool>(
        "Hosting:UseHsts",
        false
    );

var httpsPort =
    builder.Configuration.GetValue<int>(
        "Hosting:HttpsPort",
        443
    );

var logCorsPreflight =
    builder.Configuration.GetValue<bool>(
        "Diagnostics:LogCorsPreflight",
        false
    );

var configuredOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

var allowedOrigins = configuredOrigins
    .Select(NormalizeCorsOrigin)
    .Where(origin => origin is not null)
    .Cast<string>()
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedOrigins.Length == 0)
{
    // Safe deployment fallback.
    //
    // This prevents the API from failing to start merely because
    // appsettings.Production.json is missing the CORS section.
    //
    // Keep these values aligned with the actual Angular URLs.
    allowedOrigins = new[]
    {
        "http://localhost:4200",
        "http://localhost:5008",
        "http://192.168.1.88:5008",
        "http://twinbeeprintads.com:5008",
        "http://tpa-inv-frontend.twinbeeprintads.com:5008",
        "http://*.twinbeeprintads.com:5008"
    };

    Console.Error.WriteLine(
        "WARNING: Cors:AllowedOrigins is missing or contains no valid " +
        "origins. The built-in TwinBee fallback list is being used."
    );
}


// ============================================================
// DAPPER CONFIGURATION
// ============================================================

DefaultTypeMap.MatchNamesWithUnderscores = true;


// ============================================================
// ASP.NET CORE SERVICES
// ============================================================

builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Prevents Swagger schema conflicts when different modules
    // contain DTO classes with the same class name.
    //
    // Example:
    // Receiving.SaveRequest
    // PurchaseOrder.SaveRequest
    // AssetAcceptance.SaveRequest
    options.CustomSchemaIds(type =>
        type.FullName?.Replace("+", ".")
        ?? type.Name
    );

    // Do not use ResolveConflictingActions here.
    //
    // Duplicate controller routes must be corrected inside
    // the controller instead of hiding them from Swagger.
});

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TwinBee.ModernErp.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "Supplier.View",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, "SUPPLIER_VIEW")));

    options.AddPolicy(
        "Supplier.Create",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, "SUPPLIER_CREATE")));

    options.AddPolicy(
        "Supplier.Edit",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, "SUPPLIER_EDIT")));

    options.AddPolicy(
        "Supplier.Activate",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, "SUPPLIER_ACTIVATE")));

    options.AddPolicy(
        "Supplier.Deactivate",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, "SUPPLIER_DEACTIVATE")));

    AddDepreciationPolicy(options, "View", "DEPRECIATION_VIEW");
    AddDepreciationPolicy(options, "Create", "DEPRECIATION_CREATE");
    AddDepreciationPolicy(options, "EditDraft", "DEPRECIATION_EDIT_DRAFT");
    AddDepreciationPolicy(options, "Submit", "DEPRECIATION_SUBMIT");
    AddDepreciationPolicy(options, "Approve", "DEPRECIATION_APPROVE");
    AddDepreciationPolicy(options, "Post", "DEPRECIATION_POST");
    AddDepreciationPolicy(options, "Reverse", "DEPRECIATION_REVERSE");
    AddDepreciationPolicy(options, "DeleteDraft", "DEPRECIATION_DELETE_DRAFT");
    AddDepreciationPolicy(options, "Print", "DEPRECIATION_PRINT");
    AddErpPolicy(options, "MaterialIssuance.View", "INVENTORY_ISSUE_VIEW");
    AddErpPolicy(options, "MaterialIssuance.Edit", "MAT_ISSUE_ENTRY");
    AddErpPolicy(options, "MaterialIssuance.Post", "INVENTORY_ISSUE_POST");
    AddErpPolicy(options, "MaterialIssuance.Reverse", "MAT_ISSUE_ENTRY");
    AddErpPolicy(options, "StockTransfer.View", "INVENTORY_TRANSFER_VIEW");
    AddErpPolicy(options, "StockTransfer.Edit", "INVENTORY_TRANSFER_EDIT");
    AddErpPolicy(options, "StockItemInquiry.View", "INQUIRY_STOCK_ITEM");
});


// ============================================================
// DATABASE CONNECTION
// ============================================================

builder.Services.AddSingleton<
    ISqlConnectionFactory,
    SqlConnectionFactory
>();

builder.Services.AddScoped<IDbConnection>(_ =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("INVENTORY")
        ?? throw new InvalidOperationException(
            "Connection string 'INVENTORY' is missing."
        );

    return new SqlConnection(connectionString);
});


// ============================================================
// AUTHENTICATION REPOSITORY
// ============================================================

builder.Services.AddScoped<
    IAuthRepository,
    AuthRepository
>();


// ============================================================
// BIOMETRIX REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    IBiometrixAttLogRepository,
    BiometrixAttLogRepository
>();

builder.Services.AddScoped<
    IBiometrixAttlogEditRepository,
    BiometrixAttlogEditRepository
>();

builder.Services.AddScoped<
    IBiometrixDtrListingRepository,
    BiometrixDtrListingRepository
>();

builder.Services.AddScoped<BiometrixAttLogParser>();


// ============================================================
// INVENTORY REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    IItemMasterRepository,
    ItemMasterRepository
>();

builder.Services.AddScoped<
    IItemWarehouseRepository,
    ItemWarehouseRepository
>();


// ============================================================
// PURCHASE ORDER REPOSITORY
// ============================================================

builder.Services.AddScoped<
    IPurchaseOrderRepository,
    PurchaseOrderRepository
>();

builder.Services.AddScoped<
    IPurchaseOrderReportRepository,
    PurchaseOrderReportRepository
>();

builder.Services.AddScoped<
    IPurchasePriceRepository,
    PurchasePriceRepository
>();

builder.Services.AddScoped<
    IPurchasePriceService,
    PurchasePriceService
>();


// ============================================================
// RECEIVING REPOSITORY
// ============================================================

builder.Services.AddScoped<
    IReceivingRepository,
    ReceivingRepository
>();

builder.Services.AddScoped<IMaterialIssuanceRepository, MaterialIssuanceRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockItemInquiryRepository, StockItemInquiryRepository>();
builder.Services.AddScoped<IStockItemInquiryService, StockItemInquiryService>();


// ============================================================
// SUPPLIER MASTER
// ============================================================

builder.Services.AddScoped<
    ISupplierRepository,
    SupplierRepository
>();

builder.Services.AddScoped<
    ISupplierService,
    SupplierService
>();

builder.Services.AddScoped<
    ICurrentUserContext,
    HttpCurrentUserContext
>();


// ============================================================
// ASSET ACCEPTANCE REPOSITORY
// ============================================================

builder.Services.AddScoped<
    IAssetAcceptanceRepository,
    AssetAcceptanceRepository
>();

builder.Services.AddScoped<
    IFuelAcceptanceRepository,
    FuelAcceptanceRepository
>();

builder.Services.AddScoped<
    IServiceAcceptanceRepository,
    ServiceAcceptanceRepository
>();

builder.Services.AddScoped<IDepreciationRepository, DepreciationRepository>();
builder.Services.AddScoped<IDepreciationService, DepreciationService>();


// ============================================================
// TELERIK REPORTING PATH + CACHE CONFIGURATION
// ============================================================
//
// IMPORTANT:
// - Reporting:CachePath from appsettings.json is the PRIMARY cache path.
// - Telerik FileStorage needs more than "create one file" permission.
//   It creates nested folders/files and must be able to update/delete them.
// - The code below validates nested create/write/read/delete operations
//   before FileStorage is registered.
// - A temporary cache fallback is allowed automatically only in Development.
//   For Production, fix the NTFS/IIS permission instead of silently using TEMP.
// ============================================================

// ReportsPath may contain:
//   "Reports"
// or an absolute physical path.
var configuredReportsPath =
    builder.Configuration["Reporting:ReportsPath"]
    ?? "Reports";

var reportsPath = ResolvePhysicalPath(
    builder.Environment.ContentRootPath,
    configuredReportsPath
);


// Preferred/default Telerik REST-service cache:
//
// appsettings.json:
// "Reporting": {
//   "CachePath": "C:\\ProgramData\\TwinBee\\TPAInventoryAPI\\TelerikReportingCache"
// }
//
// The explicit appsettings value wins. The default below is only a safety
// default in case the setting is accidentally omitted.
var defaultReportCachePath = Path.Combine(
    Environment.GetFolderPath(
        Environment.SpecialFolder.CommonApplicationData
    ),
    "TwinBee",
    "TPAInventoryAPI",
    "TelerikReportingCache"
);

var configuredReportCachePath =
    builder.Configuration["Reporting:CachePath"]
    ?? defaultReportCachePath;

var configuredResolvedReportCachePath =
    ResolvePhysicalPath(
        builder.Environment.ContentRootPath,
        configuredReportCachePath
    );


// Development may temporarily fall back to the current process TEMP folder
// if ProgramData is not writable. Production defaults to FALSE.
//
// Optional override in appsettings:
// "Reporting": {
//   "AllowTemporaryCacheFallback": false
// }
var allowTemporaryReportCacheFallback =
    builder.Configuration.GetValue(
        "Reporting:AllowTemporaryCacheFallback",
        builder.Environment.IsDevelopment()
    );

var fallbackReportCachePath = Path.Combine(
    Path.GetTempPath(),
    "TwinBee",
    "TPAInventoryAPI",
    "TelerikReportingCache"
);


// Helpful when diagnosing IIS/AppPool ACL problems.
// Examples:
//   IIS AppPool\TPAInventoryAPI
//   DOMAIN\User
//   COMPUTER\User
var reportingProcessIdentity =
    GetCurrentProcessIdentity();


// Attempt to create the reports directory, but do not terminate the
// complete ERP API merely because report definitions are not deployed yet.
if (!TryEnsureDirectory(
        reportsPath,
        out var reportsDirectoryError))
{
    Console.Error.WriteLine(
        "WARNING: The Telerik reports directory could not be created. " +
        reportsDirectoryError
    );
}


// Validate the Telerik cache BEFORE registering FileStorage.
//
// This probe intentionally creates a nested folder and a value.dat-like file,
// then performs write/read/overwrite/delete operations. This is stronger than
// testing one file directly under the cache root and better matches the type
// of filesystem operations Telerik FileStorage performs.
var reportCachePath = ResolveTelerikCacheDirectory(
    configuredResolvedReportCachePath,
    fallbackReportCachePath,
    allowTemporaryReportCacheFallback,
    reportingProcessIdentity,
    out var reportCacheUsedFallback,
    out var reportCacheWarning
);

if (!string.IsNullOrWhiteSpace(reportCacheWarning))
{
    Console.Error.WriteLine(
        "WARNING: " + reportCacheWarning
    );
}


// ============================================================
// TELERIK REPORTING REST SERVICE
// Telerik Reporting 2026 Q2 - 20.1.26.707
// ============================================================
//
// Telerik Reporting REST Service requires IStorage. FileStorage is valid
// for the current single-instance ERP deployment as long as the selected
// directory is writable by the actual API process identity.
// ============================================================

builder.Services.TryAddSingleton<
    IReportServiceConfiguration
>(serviceProvider =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var environment =
        serviceProvider.GetRequiredService<IWebHostEnvironment>();

    var hostAppId =
        configuration["Reporting:HostAppId"]
        ?? "TPAInventoryAPI.Reporting";

    var clientSessionTimeout =
        configuration.GetValue(
            "Reporting:ClientSessionTimeoutMinutes",
            15
        );

    var reportSharingTimeout =
        configuration.GetValue(
            "Reporting:ReportSharingTimeoutMinutes",
            0
        );

    if (clientSessionTimeout <= 0)
    {
        throw new InvalidOperationException(
            "Reporting:ClientSessionTimeoutMinutes " +
            "must be greater than zero."
        );
    }

    if (reportSharingTimeout < 0)
    {
        throw new InvalidOperationException(
            "Reporting:ReportSharingTimeoutMinutes " +
            "cannot be negative."
        );
    }

    return new ReportServiceConfiguration
    {
        // Allows Telerik Reporting to read connection strings
        // and engine settings from appsettings.json.
        ReportingEngineConfiguration = configuration,

        // Keep this stable and unique for this REST service when
        // sharing the same storage location with other applications.
        HostAppId = hostAppId,

        // IMPORTANT:
        // reportCachePath is already validated above.
        // Do not point FileStorage at the Reports folder or application root.
        Storage = new FileStorage(
            reportCachePath
        ),

        // Resolves:
        // PurchaseOrder.trdp
        // ReceivingReport.trdp
        // and other report definitions inside the Reports folder.
        ReportSourceResolver =
            new UriReportSourceResolver(
                reportsPath
            ),

        // How long the REST service keeps an inactive viewer client session.
        ClientSessionTimeout =
            clientSessionTimeout,

        // Zero is appropriate for highly transactional ERP reports when you
        // prefer fresh rendering after approval/post/reversal/update actions.
        ReportSharingTimeout =
            reportSharingTimeout,

        // Full Telerik exception details only during local development.
        ExceptionsVerbosity =
            environment.IsDevelopment()
                ? "detailed"
                : "normal"
    };
});


// ============================================================
// RESPONSE COMPRESSION
// ============================================================

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;

    options.Providers.Add<
        GzipCompressionProvider
    >();
});

builder.Services.Configure<
    GzipCompressionProviderOptions
>(options =>
{
    options.Level = CompressionLevel.Fastest;
});


// ============================================================
// JWT AUTHENTICATION
// Uncomment this section when JWT authentication is restored.
// ============================================================

// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;

// var jwtIssuer =
//     builder.Configuration["Jwt:Issuer"]
//     ?? throw new InvalidOperationException(
//         "Jwt:Issuer is missing."
//     );

// var jwtAudience =
//     builder.Configuration["Jwt:Audience"]
//     ?? throw new InvalidOperationException(
//         "Jwt:Audience is missing."
//     );

// var jwtKey =
//     builder.Configuration["Jwt:Key"]
//     ?? throw new InvalidOperationException(
//         "Jwt:Key is missing."
//     );

// builder.Services
//     .AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme =
//             JwtBearerDefaults.AuthenticationScheme;

//         options.DefaultChallengeScheme =
//             JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.RequireHttpsMetadata =
//             !builder.Environment.IsDevelopment();

//         options.TokenValidationParameters =
//             new TokenValidationParameters
//             {
//                 ValidateIssuer = true,
//                 ValidIssuer = jwtIssuer,

//                 ValidateAudience = true,
//                 ValidAudience = jwtAudience,

//                 ValidateIssuerSigningKey = true,
//                 IssuerSigningKey =
//                     new SymmetricSecurityKey(
//                         Encoding.UTF8.GetBytes(jwtKey)
//                     ),

//                 ValidateLifetime = true,
//                 ClockSkew = TimeSpan.FromMinutes(1)
//             };
//     });


// ============================================================
// CORS POLICY
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            // Exact origins and controlled wildcard subdomains are
            // loaded from appsettings.json.
            //
            // Example supported entry:
            // http://*.twinbeeprintads.com:5008
            .WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()

            // Required for JSON requests, authorization headers and
            // browser preflight requests.
            .AllowAnyHeader()
            .AllowAnyMethod()

            // Required because this API currently uses cookie
            // authentication.
            //
            // Do not replace WithOrigins(...) with AllowAnyOrigin()
            // while AllowCredentials() is enabled.
            .AllowCredentials()

            // Reduces repeated OPTIONS requests without making the
            // browser cache the policy for an excessive period.
            .SetPreflightMaxAge(
                TimeSpan.FromMinutes(10)
            );
    });
});


// ============================================================
// FORWARDED HEADERS
// Required for IIS and Cloudflare Tunnel.
// ============================================================

builder.Services.Configure<
    ForwardedHeadersOptions
>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.ForwardLimit = 1;

    // Used when IIS or Cloudflare Tunnel forwards locally.
    options.KnownProxies.Add(
        IPAddress.Loopback
    );

    options.KnownProxies.Add(
        IPAddress.IPv6Loopback
    );
});


// ============================================================
// HTTPS REDIRECTION AND HSTS
// MODIFIED:
// These services are registered only when explicitly enabled in
// appsettings.json. For the present IIS HTTP-only binding on port
// 5009, both values must remain false.
// ============================================================

if (useHttpsRedirection)
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode =
            StatusCodes.Status308PermanentRedirect;

        options.HttpsPort = httpsPort;
    });
}

if (useHsts)
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = false;
        options.IncludeSubDomains = false;
        options.MaxAge = TimeSpan.FromDays(30);
    });
}


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

// Must run first so the original protocol sent by IIS or
// Cloudflare Tunnel is detected correctly.
app.UseForwardedHeaders();


// ============================================================
// EXCEPTION HANDLING
// ============================================================

if (app.Environment.IsDevelopment())
{
    // Shows the actual SwaggerGeneratorException or Telerik
    // exception during local development.
    app.UseDeveloperExceptionPage();
}
else
{
    // AddProblemDetails() above works together with this middleware.
    app.UseExceptionHandler();

    // HSTS is valid only after HTTPS is installed and working.
    if (useHsts)
    {
        app.UseHsts();
    }
}


// Keep HTTP requests on HTTP while the IIS site is bound only to
// http://*:5009. Enable this later only after adding a valid HTTPS
// binding and certificate.
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}


// ============================================================
// RESPONSE COMPRESSION
// ============================================================

app.UseResponseCompression();


// ============================================================
// SWAGGER
// ============================================================

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    // Relative URL works locally and when deployed under
    // an IIS application or virtual directory.
    options.SwaggerEndpoint(
        "./v1/swagger.json",
        "TPA Inventory API v1"
    );

    options.RoutePrefix = "swagger";

    options.DocumentTitle =
        "TPA Inventory API";

    options.DisplayRequestDuration();
});


// ============================================================
// STATIC FILES
// ============================================================

app.UseStaticFiles();


// ============================================================
// ROUTING
// ============================================================

app.UseRouting();


// ============================================================
// CORS
// Must be after routing and before authentication/authorization.
// ============================================================

// Optional temporary preflight diagnostics.
// Set Diagnostics:LogCorsPreflight=true in appsettings.json.
if (logCorsPreflight)
{
    app.Use(async (context, next) =>
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            app.Logger.LogInformation(
                "CORS preflight. Origin={Origin}; Path={Path}; " +
                "RequestedMethod={RequestedMethod}; RequestedHeaders={RequestedHeaders}",
                context.Request.Headers.Origin.ToString(),
                context.Request.Path,
                context.Request.Headers[
                    "Access-Control-Request-Method"
                ].ToString(),
                context.Request.Headers[
                    "Access-Control-Request-Headers"
                ].ToString()
            );
        }

        await next();
    });
}

app.UseCors(CorsPolicyName);


// ============================================================
// AUTHENTICATION
// Cookie authentication is issued by AuthController. The legacy JWT block
// above remains disabled until token-based authentication is configured.
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// DEVELOPMENT-ONLY REPORTING DIAGNOSTICS
// ============================================================
//
// Do not expose physical server paths in Production.
// This endpoint performs a live cache probe so permission regressions
// can be diagnosed without guessing.
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapGet(
        "/diagnostics/reporting",
        () =>
        {
            var cacheWritable =
                TryEnsureTelerikStorageDirectory(
                    reportCachePath,
                    out var liveCacheProbeError
                );

            var reportFiles = Directory.Exists(reportsPath)
                ? Directory
                    .EnumerateFiles(
                        reportsPath,
                        "*.*",
                        SearchOption.AllDirectories
                    )
                    .Where(file =>
                        file.EndsWith(
                            ".trdp",
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        file.EndsWith(
                            ".trdx",
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        file.EndsWith(
                            ".trbp",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .Select(file =>
                        Path.GetRelativePath(
                            reportsPath,
                            file
                        )
                    )
                    .OrderBy(file => file)
                    .ToArray()
                : Array.Empty<string>();

            return Results.Ok(new
            {
                environment =
                    app.Environment.EnvironmentName,

                processIdentity =
                    reportingProcessIdentity,

                reportsPath,

                configuredReportCachePath =
                    configuredResolvedReportCachePath,

                effectiveReportCachePath =
                    reportCachePath,

                usingTemporaryFallback =
                    reportCacheUsedFallback,

                temporaryFallbackAllowed =
                    allowTemporaryReportCacheFallback,

                cacheWritable,

                cacheProbeError =
                    liveCacheProbeError,

                reportFiles
            });
        }
    )
    .AllowAnonymous();
}


// ============================================================
// HEALTH CHECK
// ============================================================
//
// Keep the public health endpoint minimal. Physical report/cache paths are
// intentionally excluded because this endpoint is anonymous.
// ============================================================

app.MapGet(
    "/health",
    () => Results.Ok(new
    {
        status = "ok",
        application = "HRIStwinbeeAPI",
        environment = app.Environment.EnvironmentName,
        serverTime = DateTimeOffset.Now,
        httpsRedirection = useHttpsRedirection,
        hsts = useHsts,
        reporting = new
        {
            configured = true,
            usingTemporaryCacheFallback =
                reportCacheUsedFallback
        }
    })
).AllowAnonymous();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// REQUIRED REPORT FILE VALIDATION
// ============================================================

var requiredReports = new[]
{
    "PurchaseOrder.trdp",
    "ReceivingReport.trdp"
};

foreach (var reportName in requiredReports)
{
    var physicalReportPath =
        Path.Combine(
            reportsPath,
            reportName
        );

    if (!File.Exists(physicalReportPath))
    {
        app.Logger.LogWarning(
            "Telerik report file is missing: {ReportPath}",
            physicalReportPath
        );
    }
    else
    {
        app.Logger.LogInformation(
            "Telerik report file found: {ReportPath}",
            physicalReportPath
        );
    }
}


// ============================================================
// STARTUP DIAGNOSTICS
// These messages appear in the IIS stdout/application log.
// ============================================================

app.Logger.LogInformation(
    "Starting HRIStwinbeeAPI. Environment={Environment}; " +
    "ContentRoot={ContentRoot}; HTTPS redirect={HttpsRedirect}; " +
    "HSTS={Hsts}; ReportsPath={ReportsPath}; " +
    "ConfiguredReportCachePath={ConfiguredReportCachePath}; " +
    "EffectiveReportCachePath={EffectiveReportCachePath}; " +
    "CacheFallback={CacheFallback}; ProcessIdentity={ProcessIdentity}",
    app.Environment.EnvironmentName,
    app.Environment.ContentRootPath,
    useHttpsRedirection,
    useHsts,
    reportsPath,
    configuredResolvedReportCachePath,
    reportCachePath,
    reportCacheUsedFallback,
    reportingProcessIdentity
);

if (reportCacheUsedFallback)
{
    app.Logger.LogWarning(
        "Telerik Reporting is using a TEMPORARY fallback cache. " +
        "ConfiguredPath={ConfiguredPath}; EffectivePath={EffectivePath}; " +
        "ProcessIdentity={ProcessIdentity}. Fix the configured path ACL " +
        "and restart the application.",
        configuredResolvedReportCachePath,
        reportCachePath,
        reportingProcessIdentity
    );
}
else
{
    app.Logger.LogInformation(
        "Telerik Reporting cache validation succeeded. " +
        "CachePath={CachePath}; ProcessIdentity={ProcessIdentity}",
        reportCachePath,
        reportingProcessIdentity
    );
}

foreach (var origin in allowedOrigins)
{
    app.Logger.LogInformation(
        "Configured CORS origin: {Origin}",
        origin
    );
}


// ============================================================
// RUN APPLICATION
// ============================================================

try
{
    app.Run();
}
catch (Exception exception)
{
    // ANCM captures Console.Error when stdout logging is enabled.
    // This gives the actual exception behind HTTP Error 500.30.
    Console.Error.WriteLine(
        "FATAL: HRIStwinbeeAPI failed during application startup."
    );
    Console.Error.WriteLine(exception);
    throw;
}


// ============================================================
// HELPER METHODS
// ============================================================

static string ResolvePhysicalPath(
    string contentRootPath,
    string configuredPath)
{
    if (string.IsNullOrWhiteSpace(configuredPath))
    {
        throw new InvalidOperationException(
            "The configured directory path cannot be empty."
        );
    }

    if (Path.IsPathRooted(configuredPath))
    {
        return Path.GetFullPath(
            configuredPath
        );
    }

    return Path.GetFullPath(
        Path.Combine(
            contentRootPath,
            configuredPath
        )
    );
}


static string? NormalizeCorsOrigin(
    string? configuredOrigin)
{
    if (string.IsNullOrWhiteSpace(configuredOrigin))
    {
        return null;
    }

    var origin = configuredOrigin
        .Trim()
        .TrimEnd('/');

    // Reject Markdown link syntax accidentally copied from chat:
    // [http://host:5008](http://host:5008)
    if (origin.StartsWith("[", StringComparison.Ordinal) ||
        origin.Contains("](", StringComparison.Ordinal))
    {
        Console.Error.WriteLine(
            $"WARNING: Ignoring invalid Markdown-style CORS origin: {origin}"
        );
        return null;
    }

    // Uri doesn't accept '*' as a host label. Validate wildcard
    // origins by temporarily replacing '*.' with 'cors-wildcard.'.
    var validationOrigin = origin.Replace(
        "://*.",
        "://cors-wildcard.",
        StringComparison.OrdinalIgnoreCase
    );

    if (!Uri.TryCreate(
            validationOrigin,
            UriKind.Absolute,
            out var uri))
    {
        Console.Error.WriteLine(
            $"WARNING: Ignoring invalid CORS origin: {origin}"
        );
        return null;
    }

    if (!uri.Scheme.Equals(
            Uri.UriSchemeHttp,
            StringComparison.OrdinalIgnoreCase) &&
        !uri.Scheme.Equals(
            Uri.UriSchemeHttps,
            StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine(
            $"WARNING: Ignoring non-HTTP CORS origin: {origin}"
        );
        return null;
    }

    if (uri.AbsolutePath != "/" ||
        !string.IsNullOrEmpty(uri.Query) ||
        !string.IsNullOrEmpty(uri.Fragment))
    {
        Console.Error.WriteLine(
            $"WARNING: A CORS origin must not contain a path, query or fragment: {origin}"
        );
        return null;
    }

    return origin;
}


static bool TryEnsureDirectory(
    string directoryPath,
    out string? error)
{
    try
    {
        Directory.CreateDirectory(directoryPath);
        error = null;
        return true;
    }
    catch (Exception exception)
    {
        error =
            $"Directory='{directoryPath}'. " +
            $"Error={exception.GetType().Name}: {exception.Message}";
        return false;
    }
}


static string ResolveTelerikCacheDirectory(
    string preferredDirectory,
    string fallbackDirectory,
    bool allowTemporaryFallback,
    string processIdentity,
    out bool usedFallback,
    out string? warning)
{
    // First preference: the configured appsettings.json path.
    if (TryEnsureTelerikStorageDirectory(
            preferredDirectory,
            out var preferredError))
    {
        usedFallback = false;
        warning = null;
        return preferredDirectory;
    }

    // Production should normally stop here. Silently moving report sessions
    // to TEMP can hide a deployment/ACL problem and makes the storage less
    // predictable across IIS recycles or multiple instances.
    if (!allowTemporaryFallback)
    {
        usedFallback = false;
        warning = null;

        throw new InvalidOperationException(
            "The configured Telerik Reporting cache directory is not " +
            "usable by the current API process. " +
            $"Directory='{preferredDirectory}'. " +
            $"ProcessIdentity='{processIdentity}'. " +
            $"Reason='{preferredError}'. " +
            "Grant Modify permission (including child folder/file inheritance) " +
            "to the API/IIS application-pool identity. If this cache contains " +
            "old folders created under another account, stop the API, repair " +
            "the ACL recursively or clear the cache contents, then restart."
        );
    }

    // Development/urgent fallback only.
    if (TryEnsureTelerikStorageDirectory(
            fallbackDirectory,
            out var fallbackError))
    {
        usedFallback = true;

        warning =
            "The configured Telerik cache directory failed the nested " +
            "FileStorage write test. " +
            $"Configured='{preferredDirectory}'. " +
            $"ProcessIdentity='{processIdentity}'. " +
            $"Reason='{preferredError}'. " +
            $"TemporaryFallback='{fallbackDirectory}'. " +
            "This keeps reporting available for development, but the " +
            "recommended fix is to grant recursive Modify permission to " +
            "the configured ProgramData cache and restart the API.";

        return fallbackDirectory;
    }

    usedFallback = false;
    warning = null;

    throw new InvalidOperationException(
        "No writable Telerik Reporting cache directory is available. " +
        $"ProcessIdentity='{processIdentity}'. " +
        $"Preferred='{preferredDirectory}', Error='{preferredError}'. " +
        $"Fallback='{fallbackDirectory}', Error='{fallbackError}'."
    );
}


static bool TryEnsureTelerikStorageDirectory(
    string directoryPath,
    out string? error)
{
    string? probeRoot = null;

    try
    {
        // Telerik FileStorage creates a hierarchy below this path.
        Directory.CreateDirectory(directoryPath);

        probeRoot = Path.Combine(
            directoryPath,
            $".telerik-storage-probe-{Guid.NewGuid():N}"
        );

        // Test nested directory creation, not just a single root-level file.
        var nestedDirectory = Path.Combine(
            probeRoot,
            "Is",
            "A"
        );

        Directory.CreateDirectory(
            nestedDirectory
        );

        // Use a Telerik-like filename to make the test intent obvious.
        var testFilePath = Path.Combine(
            nestedDirectory,
            "value.dat"
        );

        var firstPayload =
            System.Text.Encoding.UTF8.GetBytes(
                "Telerik Reporting FileStorage write test."
            );

        File.WriteAllBytes(
            testFilePath,
            firstPayload
        );

        // Confirm that the process can read the stored value.
        var readPayload =
            File.ReadAllBytes(
                testFilePath
            );

        if (readPayload.Length != firstPayload.Length)
        {
            throw new IOException(
                "The Telerik cache probe file was read with an " +
                "unexpected length."
            );
        }

        // Confirm overwrite/update access.
        File.WriteAllText(
            testFilePath,
            "Telerik Reporting FileStorage overwrite test."
        );

        // Confirm delete access, which is required when cache assets expire.
        File.Delete(
            testFilePath
        );

        // Confirm nested directory cleanup rights.
        Directory.Delete(
            probeRoot,
            recursive: true
        );

        probeRoot = null;
        error = null;
        return true;
    }
    catch (Exception exception)
    {
        error =
            $"{exception.GetType().Name}: {exception.Message}";

        return false;
    }
    finally
    {
        // Best-effort cleanup only. Never hide the original permission error.
        if (!string.IsNullOrWhiteSpace(probeRoot))
        {
            try
            {
                if (Directory.Exists(probeRoot))
                {
                    Directory.Delete(
                        probeRoot,
                        recursive: true
                    );
                }
            }
            catch
            {
                // Intentionally ignored.
            }
        }
    }
}


static string GetCurrentProcessIdentity()
{
    try
    {
        return System.Security.Principal.WindowsIdentity
            .GetCurrent()
            .Name;
    }
    catch
    {
        return Environment.UserName;
    }
}


static bool HasSupplierPermission(
    ClaimsPrincipal user,
    string permissionKey)
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return false;
    }

    var administrator = user.FindFirstValue("administrator");
    if (string.Equals(
            administrator,
            "Y",
            StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return user.Claims.Any(claim =>
        claim.Type == "erp_permission" &&
        string.Equals(
            claim.Value,
            permissionKey,
            StringComparison.OrdinalIgnoreCase));
}

static void AddDepreciationPolicy(
    Microsoft.AspNetCore.Authorization.AuthorizationOptions options,
    string policyName,
    string permissionKey)
{
    options.AddPolicy(
        $"Depreciation.{policyName}",
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, permissionKey)));
}

static void AddErpPolicy(
    Microsoft.AspNetCore.Authorization.AuthorizationOptions options,
    string policyName,
    string permissionKey)
{
    options.AddPolicy(
        policyName,
        policy => policy.RequireAssertion(context =>
            HasSupplierPermission(context.User, permissionKey)));
}
