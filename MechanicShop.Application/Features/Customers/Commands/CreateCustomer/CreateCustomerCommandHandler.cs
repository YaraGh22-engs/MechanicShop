using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

//Command Handler يستخدم CQRS Pattern مع MediatR
//Primary Constructor (C# 12) (مباشرة في تعريف الكلاس)
public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
    )
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{               //               ↑                      ↑
                //            Input Type            Return Type
    private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLower();

        var exists = await _context.Customers.AnyAsync(
            c => c.Email!.ToLower() == email,
            ct);

        if (exists)
        {
            _logger.LogWarning("Customer creation aborted. Email already exists.");

            return CustomerErrors.CustomerExists;
        }

        List<Vehicle> vehicles = [];

        foreach (var v in command.Vehicles)
        {
            var vehicleResult = Vehicle.Create(Guid.NewGuid(), v.Make, v.Model, v.Year, v.LicensePlate);

            if (vehicleResult.IsError)
            {
                return vehicleResult.Errors;
            }

            vehicles.Add(vehicleResult.Value);
        }

        var createCustomerResult = Customer.Create(
            Guid.NewGuid(),
            command.Name.Trim(),
            command.PhoneNumber.Trim(),
            command.Email.Trim(),
            vehicles);

        if (createCustomerResult.IsError)
        {
            return createCustomerResult.Errors;
        }

        _context.Customers.Add(createCustomerResult.Value);

        await _context.SaveChangesAsync(ct);

        //إزالة البيانات المُخزنة مؤقتاً لضمان الحصول على أحدث البيانات
        await _cache.RemoveByTagAsync("customer", ct);

        var customer = createCustomerResult.Value;
        //Success Logging
        _logger.LogInformation("Customer created successfully. Id: {CustomerId}", createCustomerResult.Value.Id);
        //Extension Method: .ToDto() convert Domain to DTO
        return customer.ToDto();
        
        // CQRS 
        //Request → Command → Handler → Domain → Database → Response
        //             
        //CreateCustomerRequest → CreateCustomerCommand → CreateCustomerCommandHandler → Customer.Create() → EF Core → CustomerDto
    }
}