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

// Retrieves all the coupons
app.MapGet("api/coupons", (ILogger<Program> _logger) =>
{
    _logger.Log(LogLevel.Information, "Fetching coupons");
    return Results.Ok(CouponStore.couponList);
})
.WithName("GetCoupons")
.Produces<IEnumerable<Coupon>>(200);

// Retrieve coupon based on Id
app.MapGet("api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.couponList.FirstOrDefault(u => u.Id == id));
})
.WithName("GetCoupon")
.Produces<Coupon>(200);

// Creates a coupon
app.MapPost("api/coupon", async (IMapper _mapper, IValidator<CreateCouponDTO> _validator, [FromBody] CreateCouponDTO coupon_C_DTO) =>
{
    // Perform validation
    var validationResult = await _validator.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.FirstOrDefault().ToString());
    }
    if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
    {
        return Results.BadRequest("Coupon Name already exist");
    }

    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
    coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
    CouponStore.couponList.Add(coupon);

    CouponDTO coupon_DTO = _mapper.Map<CouponDTO>(coupon);

    // return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    return Results.CreatedAtRoute("GetCoupon", new { Id = coupon.Id }, coupon_DTO);
})
.WithName("CreateCoupon")
.Accepts<Coupon>("application/json")
.Produces<Coupon>(201)
.Produces(400);

// Updates a coupon
app.MapPut("api/coupon", () =>
{

});

// Deletes a coupon
app.MapDelete("api/coupon/{id:int}", (int id) => { });


app.UseHttpsRedirection();
app.Run();
