using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using PgrStudentManagement.Adapters.API.AcademicSupervisors.StudentLookupClient;
using PgrStudentManagement.Adapters.API.Controllers;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.Persistence.Documents.Gateway;
using PgrStudentManagement.Adapters.Persistence.Documents.Mappers;
using PgrStudentManagement.Adapters.Persistence.Relational;
using PgrStudentManagement.Adapters.Persistence.Relational.Gateway;
using PgrStudentManagement.Adapters.Persistence.Relational.Mappers;
using PgrStudentManagement.Adapters.Web.Authentication.Configuration;
using PgrStudentManagement.Adapters.Web.Controllers;
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Business.AcademicSupervisors.AssignSupervisor;
using PgrStudentManagement.Business.AcademicSupervisors.Gateway;
using PgrStudentManagement.Business.DummyModule;
using PgrStudentManagement.Business.ResearchActivities.Gateway;
using PgrStudentManagement.Business.ResearchActivities.RecordResearchActivity;
using PgrStudentManagement.Business.ResearchActivities.ViewStudentResearchActivities;
using PgrStudentManagement.Business.Security;
using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Business.Students.GetStudentSummary;
using PgrStudentManagement.Business.Students.ViewStudentEnrolments;
using PgrStudentManagement.Domain.ResearchActivities;

var builder = WebApplication.CreateBuilder(args);

// Register persistence
builder.Services.AddDbContext<StudentManagementDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("StudentManagementDatabase")
        ?? "Data Source=student-management.db"));

// Optional MongoDB configuration (if configured)
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDatabase");
if (!string.IsNullOrWhiteSpace(mongoConnectionString))
{
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
    builder.Services.AddScoped<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var databaseName = builder.Configuration["MongoDatabaseName"] ?? "pgr_student_management";
        return client.GetDatabase(databaseName);
    });
}

// Register mappers
builder.Services.AddSingleton<EnrolmentPersistenceMapper>();
builder.Services.AddSingleton<StudentEnrolmentsWebMapper>();
builder.Services.AddSingleton<StudentEnrolmentsApiMapper>();
builder.Services.AddSingleton<StudentLookupApiMapper>();
builder.Services.AddSingleton<ResearchActivityDocumentMapper>();
builder.Services.AddSingleton<ResearchActivityWebMapper>();
builder.Services.AddSingleton<ResearchActivityApiMapper>();
builder.Services.AddSingleton<AcademicSupervisorPersistenceMapper>();
builder.Services.AddSingleton<AcademicSupervisorApiMapper>();

// Register gateways (output ports)
builder.Services.AddScoped<IEnrolmentGateway, EfEnrolmentGateway>();
builder.Services.AddScoped<IResearchActivityGateway, MongoResearchActivityGateway>();
builder.Services.AddScoped<IAcademicSupervisorGateway, EfAcademicSupervisorGateway>();
builder.Services.AddScoped<IStudentGateway, EfStudentGateway>();

// HTTP Student Lookup Adapter for Academic Supervisors (Part 2)
var studentsBaseUrl = builder.Configuration["Services:Students:BaseUrl"]
    ?? throw new InvalidOperationException("The Students service base URL is not configured.");

builder.Services.AddHttpClient<IStudentLookup, HttpStudentLookupAdapter>(client =>
{
    client.BaseAddress = new Uri(studentsBaseUrl);
});

// Register use cases (input ports)
builder.Services.AddScoped<IViewStudentEnrolmentsUseCase, ViewStudentEnrolmentsUseCase>();
builder.Services.AddScoped<IGetStudentSummaryUseCase, GetStudentSummaryUseCase>();
builder.Services.AddScoped<IRecordResearchActivityUseCase, RecordResearchActivityUseCase>();
builder.Services.AddScoped<IViewStudentResearchActivitiesUseCase, ViewStudentResearchActivitiesUseCase>();
builder.Services.AddScoped<IAssignSupervisorUseCase, AssignSupervisorUseCase>();
builder.Services.AddScoped<IDummyService, DummyService>();

// Register configuration
builder.Services.Configure<DemoAuthenticationOptions>(
    builder.Configuration.GetSection(DemoAuthenticationOptions.SectionName));

// Register cookie authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/access-denied";

        options.Cookie.Name = "PgrStudentManagement.Auth";

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Register RBAC authorisation policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AppPolicies.StudentsList,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsRead,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsCreate,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator));

    options.AddPolicy(
        AppPolicies.StudentsUpdateStatus,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsUpdateExpectedDate,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsRecordSubmission,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));

    options.AddPolicy(
        AppPolicies.StudentsReadEnrolments,
        policy => policy.RequireRole(
            AppRoles.CollegeAdministrator,
            AppRoles.PostgraduateResearchTutor));
});

// Register MVC and API controllers from adapter assemblies
builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(StudentsController).Assembly)
    .AddApplicationPart(typeof(StudentsApiController).Assembly);

var app = builder.Build();

// Ensure relational database exists and is seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StudentManagementDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(context);

    // Seed sample research activities
    var activityGateway = scope.ServiceProvider.GetRequiredService<IResearchActivityGateway>();
    var existingActivities = await activityGateway.GetForStudentAsync("S100001", CancellationToken.None);
    if (existingActivities.Count == 0)
    {
        await activityGateway.AddAsync(new ResearchActivity
        {
            Id = "act-s1-001",
            StudentNumber = "S100001",
            ActivityType = ResearchActivityType.ResearchReflection,
            Title = "Literature review on Clean Architecture patterns in enterprise applications",
            ActivityDate = new DateTime(2025, 11, 10),
            CreatedOn = new DateTime(2025, 11, 10, 14, 0, 0),
            Tags = ["literature", "architecture"],
            Details = new Dictionary<string, object> { ["Notes"] = "Analyzed decoupling of domain and business logic from database concerns." }
        }, CancellationToken.None);

        await activityGateway.AddAsync(new ResearchActivity
        {
            Id = "act-s1-002",
            StudentNumber = "S100001",
            ActivityType = ResearchActivityType.ConferenceAttendance,
            Title = "Attendance at International Software Architecture Conference",
            ActivityDate = new DateTime(2026, 1, 20),
            CreatedOn = new DateTime(2026, 1, 21, 9, 30, 0),
            Tags = ["conference", "presentation"],
            Details = new Dictionary<string, object> { ["Notes"] = "Presented lightning talk on hexagonal architecture and ports-and-adapters." }
        }, CancellationToken.None);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
