namespace BackEnd.Handlers

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.HttpResults
open Microsoft.EntityFrameworkCore
open BackEnd.Data
open BackEnd.Data.Models
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson

module Handlers =

    let jsonOptions = 
        let opts = JsonSerializerOptions()
        opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        opts.Converters.Add(JsonFSharpConverter())
        opts

    // User handlers
    let getUsers (db: AppDbContext) =
        task {
            let! users = db.Users.ToListAsync()
            return Results.Json(users, jsonOptions) :> IResult
        }

    let getUserById (db: AppDbContext) (id: int) =
        task {
            let userOpt = db.Users.Find(id)
            if isNull (box userOpt) then
                return Results.NotFound() :> IResult
            else
                return Results.Json(userOpt, jsonOptions) :> IResult
        }

    let createUser (db: AppDbContext) (user: User) =
        task {
            user.CreatedAt <- System.DateTime.UtcNow
            db.Users.Add(user) |> ignore
            let! _ = db.SaveChangesAsync()
            return Results.Created($"/api/users/{user.Id}", user) :> IResult
        }

    // Product handlers
    let getProducts (db: AppDbContext) =
        task {
            let! products = db.Products.ToListAsync()
            return Results.Json(products, jsonOptions) :> IResult
        }

    let getProductById (db: AppDbContext) (id: int) =
        task {
            let productOpt = db.Products.Find(id)
            if isNull (box productOpt) then
                return Results.NotFound() :> IResult
            else
                return Results.Json(productOpt, jsonOptions) :> IResult
        }

    let createProduct (db: AppDbContext) (product: Product) =
        task {
            product.CreatedAt <- System.DateTime.UtcNow
            db.Products.Add(product) |> ignore
            let! _ = db.SaveChangesAsync()
            return Results.Created($"/api/products/{product.Id}", product) :> IResult
        }
