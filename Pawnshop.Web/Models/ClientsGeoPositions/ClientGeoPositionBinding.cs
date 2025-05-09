using System;
using FluentValidation;

namespace Pawnshop.Web.Models.ClientsGeoPositions
{
    public sealed class ClientGeoPositionBinding
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Date { get; set; }
    }

    public sealed class ClientGeoPositionBindingValidator : AbstractValidator<ClientGeoPositionBinding>
    {
        public ClientGeoPositionBindingValidator()
        {
            RuleFor(b => b.Latitude)
                .GreaterThanOrEqualTo(-90)
                .LessThanOrEqualTo(90);
            RuleFor(b => b.Longitude)
                .GreaterThanOrEqualTo(-180)
                .LessThanOrEqualTo(180);
        }
    }

}
