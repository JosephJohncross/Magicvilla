using System.Net;
using MagicVilla_coupon;
using MagicVilla_coupon.Data;
using MagicVilla_coupon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MagicVilla_coupon.Models.DTOs;
using AutoMapper;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MagicVilla", Version = "v1" });
});
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
var app = builder.Build();

// Configure the http request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MagicVilla");
    });
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(
                "An unexpected fault happened. Try again"
            );
        });
    });
}


// Retrieves all the coupons
app.MapGet("api/coupons", (ILogger<Program> _logger) =>
{
    ApiResponse response = new();
    _logger.Log(LogLevel.Information, "Fetching coupons");

    response.IsSuccess = true;
    response.Result = CouponStore.couponList;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
})
.WithName("GetCoupons")
.Produces<ApiResponse>(200);

// Retrieve coupon based on Id
app.MapGet("api/coupon/{id:int}", (int id) =>
{
    ApiResponse response = new()
    {
        IsSuccess = true,
        Result = CouponStore.couponList.FirstOrDefault(u => u.Id == id),
        StatusCode = HttpStatusCode.OK
    };
    return Results.Ok(response);
})
.WithName("GetCoupon")
.Produces<ApiResponse>(200);

// Creates a coupon
app.MapPost("api/coupon", async (IMapper _mapper, IValidator<CreateCouponDTO> _validator, [FromBody] CreateCouponDTO coupon_C_DTO) =>
{
    ApiResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };
    // Perform validation
    var validationResult = await _validator.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }
    if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already exists");
        return Results.BadRequest(response);
        // return Results.BadRequest("");
    }

    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
    coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
    CouponStore.couponList.Add(coupon);

    CouponDTO coupon_DTO = _mapper.Map<CouponDTO>(coupon);

    response.IsSuccess = true;
    response.Result = coupon_DTO;
    response.StatusCode = HttpStatusCode.OK;

    // return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    // return Results.CreatedAtRoute("GetCoupon", new { Id = coupon.Id }, coupon_DTO);
    return Results.Ok(response);
})
.WithName("CreateCoupon")
.Accepts<CreateCouponDTO>("application/json")
.Produces<ApiResponse>(201)
.Produces(400);

// Updates a coupon
app.MapPut("api/coupon", async (IMapper _mapper, IValidator<CouponUpdateDTO> _validator, [FromBody] CouponUpdateDTO coupon_U_DTO) =>
{
    ApiResponse response = new()
    {
        IsSuccess = false,
        StatusCode = HttpStatusCode.BadRequest
    };

    // Perform validation
    var validationResult = await _validator.ValidateAsync(coupon_U_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }
    if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon_U_DTO.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("Coupon Name already exists");
        return Results.BadRequest(response);
        // return Results.BadRequest("");
    }

    try
    {
        Coupon coupon = CouponStore.couponList.Find(u => u.Id == coupon_U_DTO.Id);
        coupon.Name = coupon_U_DTO.Name;
        coupon.isActive = coupon_U_DTO.isActive;
        coupon.Percent = coupon_U_DTO.Percent;

        CouponDTO coupon_DTO = _mapper.Map<CouponDTO>(coupon);

        // Response
        response.StatusCode = HttpStatusCode.OK;
        response.IsSuccess = true;
        response.Result = coupon_DTO;
        return Results.Ok(response);
    }
    catch
    {
        response.ErrorMessages.Add("Coupon Not Found");
        return Results.BadRequest(response);
    }
})
.WithName("UpdateCoupon")
.Accepts<CouponUpdateDTO>("application/json")
.Produces<ApiResponse>(201)
.Produces(400);

// Deletes a coupon
app.MapDelete("api/coupon/{id:int}", (int id) =>
{
    ApiResponse response = new() { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false };
    Coupon coupon = CouponStore.couponList.Find(u => u.Id == id);

    if (coupon != null)
    {
        CouponStore.couponList.Remove(coupon);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;
        return Results.Ok(response);
    }

    response.ErrorMessages.Add("Coupon does not exist");
    return Results.BadRequest(response);

})
.WithName("DeleteCoupon")
.Produces<ApiResponse>(204)
.Produces(400);

//For practice 
// app.MapGet("/hello", (LinkGenerator generator) =>
// {
//     return generator.GetPathByName("GetCoupon");
// });


app.UseHttpsRedirection();
app.Run();
