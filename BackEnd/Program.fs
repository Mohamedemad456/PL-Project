namespace BackEnd

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.EntityFrameworkCore
open BackEnd.Data
open BackEnd.Handlers
open Microsoft.AspNetCore.Http

module Program =

    let builder = WebApplication.CreateBuilder()

    // Add services
    builder.Services.AddEndpointsApiExplorer() |> ignore
    builder.Services.AddSwaggerGen() |> ignore
    builder.Services.AddDbContext<AppDbContext>(fun options ->
        let connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        options.UseSqlServer(connectionString) |> ignore
    ) |> ignore

    builder.Services.AddCors(fun options ->
        options.AddDefaultPolicy(fun policy ->
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader() |> ignore
        ) |> ignore
    ) |> ignore

    let app = builder.Build()

    // Configure the HTTP request pipeline
    if app.Environment.IsDevelopment() then
        app.UseSwagger() |> ignore
        app.UseSwaggerUI() |> ignore

    app.UseCors() |> ignore
    app.UseHttpsRedirection() |> ignore

    // User endpoints
    app.MapGet("/api/users", 
        System.Func<AppDbContext, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db -> Handlers.getUsers db)) |> ignore
    
    app.MapGet("/api/users/{id:int}", 
        System.Func<AppDbContext, int, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db id -> Handlers.getUserById db id)) |> ignore
    
    app.MapPost("/api/users", 
        System.Func<AppDbContext, BackEnd.Data.Models.User, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db user -> Handlers.createUser db user)) |> ignore

    // Product endpoints
    app.MapGet("/api/products", 
        System.Func<AppDbContext, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db -> Handlers.getProducts db)) |> ignore
    
    app.MapGet("/api/products/{id:int}", 
        System.Func<AppDbContext, int, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db id -> Handlers.getProductById db id)) |> ignore
    
    app.MapPost("/api/products", 
        System.Func<AppDbContext, BackEnd.Data.Models.Product, System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>>(
            fun db product -> Handlers.createProduct db product)) |> ignore

    // Health check endpoint
    app.MapGet("/health", 
        System.Func<Microsoft.AspNetCore.Http.IResult>(
            fun () -> Microsoft.AspNetCore.Http.Results.Ok({| status = "healthy" |}) :> Microsoft.AspNetCore.Http.IResult)) |> ignore

    app.Run()
