using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);

// Constants
const string SecretKey = "your_very_long_secret_key_at_least_32_chars";
const string Issuer = "your_issuer";
const string Audience = "your_audience";

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// List of dictionaries to registered employees
var employees = new List<Dictionary<string, object>>
{
    new Dictionary<string, object>
    {
        { "name", "John" },
        { "age", 30 },
        { "job", "Engineer" },
        { "salary", 50000 }
    },
    new Dictionary<string, object>
    {
        { "name", "Mary" },
        { "age", 25 },
        { "job", "Designer" },
        { "salary", 45000 }
    },
    new Dictionary<string, object>
    {
        { "name", "Peter" },
        { "age", 35 },
        { "job", "Developer" },
        { "salary", 60000 }
    }
};

// Dictionary to store users with hashed passwords
var users = new List<Dictionary<string, string>>
{
    new Dictionary<string, string> { { "username", "user" }, { "password", Hasher.passwordHash("1234")} },
};

app.UseAuthentication();
app.UseAuthorization();

// Endpoint to get home page
app.MapGet("/", async context =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync("static/users.html");
    }
    else
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync("static/login.html");
    }
});

// Endpoint to get login page
app.MapGet("/login", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("static/login.html");
});

// Endpoint to get users page
app.MapGet("/users", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("static/users.html");
});

// Endpoint to login
app.MapPost("/login", async context =>
{
    try
    {
        var userFound = false;
        var form = await context.Request.ReadFormAsync();

        var username = form["username"];
        var password = form["password"];

        // Validate form inputs
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Username and password are required");
            return;
        }

        // Validate user credentials
        foreach (var user in users)
        {
            if (user["username"] == username && Hasher.passwordHash(password) == user["password"])
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var key = Encoding.ASCII.GetBytes(SecretKey);
                var token = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{ \"token\": \"{tokenString}\" }}");
                Console.WriteLine($"User \"{username}\" logged in");
                userFound = true;
            }
        }
        if (userFound == false)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"error\": \"Invalid username or password\"}");
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("{\"error\": \"500\"}");
    }
});

// Endpoint to get all users
app.MapGet("/readusers", async context =>
{
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(employees);
    Console.WriteLine("Employees:");
    foreach (var employee in employees)
    {
        Console.WriteLine($"Name: {employee["name"]}, Age: {employee["age"]}, Job: {employee["job"]}, Salary: {employee["salary"]}");
    }
    Console.WriteLine("---------------------------------------");
});

// Endpoint to add user
app.MapPost("/adduser", async context =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        var name = form["name"];
        var age = form["age"];
        var job = form["job"];
        var salary = form["salary"];

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(age) || string.IsNullOrEmpty(job) || string.IsNullOrEmpty(salary))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Name, age, job, and salary are required");
            return;
        }

        var newEmployee = new Dictionary<string, object>
        {
            { "name", name },
            { "age", int.Parse(age) },
            { "job", job },
            { "salary", int.Parse(salary) }
        };

        employees.Add(newEmployee);

        Console.WriteLine($"New employee added: Name: {name}, Age: {age}, Job: {job}, Salary: {salary}");
        Console.WriteLine("---------------------------------------");

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(employees);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("{\"error\": \"500\"}");
    }
});

// Endpoint to delete user/s
app.MapDelete("/deleteusers", async context =>
{   
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        var userNames= JsonSerializer.Deserialize<DeleteUsersRequest>(body);
        foreach(var name in userNames.names)
        {
            var userFound = false;
            for (int i = 0; i < employees.Count; i++)
            {
                if (employees[i]["name"].ToString() == name)
                {
                    Console.WriteLine($"User \"{name}\" deleted");
                    employees.RemoveAt(i);
                    userFound = true;
                    break;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("{\"error\": \"500\"}");
    }
});

// Endpoint to edit user
app.MapPut("/edituser", async context =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    try
    {
        var employee = JsonSerializer.Deserialize<EmployeeDetails>(body);
        if (employee != null)
        {
            for (int i = 0; i < employees.Count; i++)
            {
                if (employees[i]["name"].ToString() == employee.name)
                {
                    employees[i]["age"] = employee.age;
                    employees[i]["job"] = employee.job;
                    employees[i]["salary"] = employee.salary;
                    Console.WriteLine($"User \"{employee.name}\" edited");
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("{\"status\": \"success\"}");
                    return;
                }
            }
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("{\"error\": \"User not found\"}");
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("{\"error\": \"Invalid user data\"}");
        }
    }
    catch (JsonException ex)
    {
        Console.WriteLine(ex.Message);
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("{\"error\": \"Invalid JSON format\"}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("{\"error\": \"500\"}");
    }
});

app.Run();

public static class Hasher
{
    public static string passwordHash(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

public class DeleteUsersRequest
{
    public List<string> names { get; set; }
}

public class EmployeeDetails
{
    public string name { get; set; }
    public int age { get; set; }
    public string job { get; set; }
    public int salary { get; set; }
}