using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MagicVilla_coupon.Models.DTOs;

namespace MagicVilla_coupon.Validations
{
    public class CouponCreateValidation : AbstractValidator<CreateCouponDTO>
    {
        public CouponCreateValidation()
        {
            RuleFor(model => model.Name).NotEmpty();
            RuleFor(model => model.Percent).InclusiveBetween(1, 100);
        }
    }
}