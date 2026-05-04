using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderServices
    {
        private readonly MyDbContext _context;
        public PurchaseOrderRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SupplierExists(string email)
        {
            return await _context.supplier.AnyAsync(supplier => supplier.email == email);
        }

        public async Task<ResponseBase> AddSupplier(SupplierDTO supplierDTO)
        {
            bool supplierExists = await _context.supplier.AnyAsync(supplier => supplier.email == supplierDTO.email);
            if (supplierExists)
            {
                return ResponseBase.Failure("Supplier with this email already exists.");
            }

            var supplier = new SupplierModel
            {
                company = supplierDTO.company,
                country = supplierDTO.country,
                address = supplierDTO.address,
                apartment = supplierDTO.apartment,
                city = supplierDTO.city,
                postal_code = supplierDTO.postal_code,
                contact_name = supplierDTO.contact_name,
                email = supplierDTO.email,
                phone = supplierDTO.phone
            };

            _context.supplier.Add(supplier);
            await _context.SaveChangesAsync();

            return ResponseBase.Success("Supplier created successfully.");
        }

        public async Task<ResponseBase> GetAllSuppliers()
        {
            var suppliers = await _context.supplier.ToListAsync();

            var supplierDTOs = suppliers.Select(supplier => new SupplierDTO
            {
                company = supplier.company,
                country = supplier.country,
                address = supplier.address,
                apartment = supplier.apartment,
                city = supplier.city,
                postal_code = supplier.postal_code,
                contact_name = supplier.contact_name,
                email = supplier.email,
                phone = supplier.phone
            }).ToList();

            return ResponseBase.Success("Suppliers retrieved successfully", supplierDTOs);
        }

        public async Task<ResponseBase> UpdateSupplier(int id, SupplierDTO supplierDTO)
        {
            var supplier = await _context.supplier.FindAsync(id);
            if (supplier == null)
            {
                return ResponseBase.Failure($"Supplier with ID {id} not found.");
            }

            supplier.company = supplierDTO.company;
            supplier.country = supplierDTO.country;
            supplier.address = supplierDTO.address;
            supplier.apartment = supplierDTO.apartment;
            supplier.city = supplierDTO.city;
            supplier.postal_code = supplierDTO.postal_code;
            supplier.contact_name = supplierDTO.contact_name;
            supplier.email = supplierDTO.email;
            supplier.phone = supplierDTO.phone;

            await _context.SaveChangesAsync();

            return ResponseBase.Success($"Supplier with ID {id} was successfully updated.");
        }
    }
}
