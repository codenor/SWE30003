using ElectronicsStoreAss3.Models;
using Microsoft.AspNetCore.Identity;

namespace ElectronicsStoreAss3.Data
{
    public static class Init
    {
        public static void SeedOwnerAccount(AppDbContext context)
        {
            if (context.Accounts.Any(a => a.Role == Role.Owner))
                return;

            var hasher = new PasswordHasher<object>();
            var account = new Account
            {
                Email = "owner@test.com",
                PasswordHash = hasher.HashPassword(null, "password"),
                Role = Role.Owner
            };

            context.Accounts.Add(account);
            context.SaveChanges();

            var owner = new Owner
            {
                AccountId = account.Id,
                Name = "Max",
                LastName = "Isghey"
            };

            context.Owners.Add(owner);
            context.SaveChanges();
        }
    }
}
