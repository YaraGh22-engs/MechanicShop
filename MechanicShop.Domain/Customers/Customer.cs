using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MechanicShop.Domain.Customers
{
    public sealed class Customer : AuditableEntity
    {
        public string? Name { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Email { get; private set; }

        private readonly List<Vehicle> _vehicles = [];
        public IEnumerable<Vehicle> Vehicles => _vehicles.AsReadOnly();

        private Customer()
        { }

        private Customer(Guid id, string name, string phoneNumber, string email, List<Vehicle> vehicles)
            : base(id)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            _vehicles = vehicles;
        }

        // create must be static
        public static Result<Customer> Create(Guid id, string name, string phoneNumber, string email, List<Vehicle> vehicles)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CustomerErrors.NameRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            {
                return CustomerErrors.InvalidPhoneNumber;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return CustomerErrors.EmailRequired;
            }

            try
            {
                _ = new MailAddress(email);
            }
            catch
            {
                return CustomerErrors.EmailInvalid;
            }

            return new Customer(id, name, phoneNumber, email, vehicles);
        }

        public Result<Updated> Update(string name, string email, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CustomerErrors.NameRequired;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return CustomerErrors.EmailRequired;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            {
                return CustomerErrors.InvalidPhoneNumber;
            }

            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;

            return Result.Updated;
        }
        // Upsert (Update + Insert)
        public Result<Updated> UpsertParts(List<Vehicle> incomingVehicle)
        {
            //// 1. Delete unwanted vehicles
            _vehicles.RemoveAll(existing => incomingVehicle.All(v => v.Id != existing.Id));

            //// 2. Update the existing one or add a new one
            foreach (var incoming in incomingVehicle)
            {
                var existing = _vehicles.FirstOrDefault(v => v.Id == incoming.Id);
                if (existing is null)
                {
                    _vehicles.Add(incoming); // add a new one
                }
                else
                {
                    var updateVehicleResult = existing.Update(incoming.Make, incoming.Model, incoming.Year, incoming.LicensePlate);

                    if (updateVehicleResult.IsError)
                    {
                        return updateVehicleResult.Errors; // Update the existing one
                    }
                }
            }

            return Result.Updated;
        }
    }
}
