using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using PgrStudentManagement.Adapters.API.Controllers;
using PgrStudentManagement.Adapters.Web.Authentication.Controllers;
using PgrStudentManagement.Adapters.Web.Controllers;
using PgrStudentManagement.Adapters.Web.DummyModule;

namespace PgrStudentManagement.Tests.Web.Controllers;

public class AuthorizationMetadataTests
{
    [Fact]
    public void DummyController_PrivateActions_HaveAuthorizeAttribute()
    {
        var privatePageMethod = typeof(DummyController).GetMethod(nameof(DummyController.PrivatePage));
        var privateResultMethod = typeof(DummyController).GetMethod(nameof(DummyController.PrivateResult));

        Assert.NotNull(privatePageMethod);
        Assert.NotNull(privateResultMethod);

        Assert.NotNull(privatePageMethod.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(privateResultMethod.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void DummyController_PublicActions_HaveAllowAnonymousAttribute()
    {
        var indexMethod = typeof(DummyController).GetMethod(nameof(DummyController.Index));
        var publicPageMethod = typeof(DummyController).GetMethod(nameof(DummyController.PublicPage));
        var publicResultMethod = typeof(DummyController).GetMethod(nameof(DummyController.PublicResult));

        Assert.NotNull(indexMethod);
        Assert.NotNull(publicPageMethod);
        Assert.NotNull(publicResultMethod);

        Assert.NotNull(indexMethod.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(publicPageMethod.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(publicResultMethod.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void AccountController_HasCorrectAuthorizationAttributes()
    {
        var loginGet = typeof(AccountController).GetMethods()
            .Single(m => m.Name == nameof(AccountController.Login)
                         && m.GetParameters().Length == 1
                         && m.GetParameters()[0].ParameterType == typeof(string));
        var loginPost = typeof(AccountController).GetMethods()
            .Single(m => m.Name == nameof(AccountController.Login)
                         && m.GetParameters().Length == 1
                         && m.GetParameters()[0].ParameterType.Name == "LoginViewModel");
        var logout = typeof(AccountController).GetMethod(nameof(AccountController.Logout));
        var accessDenied = typeof(AccountController).GetMethod(nameof(AccountController.AccessDenied));

        Assert.NotNull(loginGet.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(loginPost.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(logout?.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(accessDenied?.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void StudentsController_HasAuthorizeAttribute()
    {
        var attribute = typeof(StudentsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
    }

    [Fact]
    public void StudentsApiController_HasAuthorizeAttribute()
    {
        var attribute = typeof(StudentsApiController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
    }

    [Fact]
    public void StudentsApiController_LookupStudent_HasAllowAnonymousAttribute()
    {
        var method = typeof(StudentsApiController).GetMethod(nameof(StudentsApiController.LookupStudent));
        Assert.NotNull(method);
        var attribute = method.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.NotNull(attribute);
    }
}
