using MagicVilla_coupon.Data;
using MagicVilla_coupon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MagicVilla", Version = "v1" });
});
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
app.MapPost("api/coupon", ([FromBody] Coupon coupon) =>
{
    if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
    {
        return Results.BadRequest("Invalid Id or Coupon Name");
    }
    if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon.Name.ToLower()) != null)
    {
        return Results.BadRequest("Coupon Name already exist");
    }

    coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
    CouponStore.couponList.Add(coupon);

    // return Results.Created($"/api/coupon/{coupon.Id}", coupon);
    return Results.CreatedAtRoute("GetCoupon", new { Id = coupon.Id }, coupon);
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
