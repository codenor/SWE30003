using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly AppDbContext _context;

        public ShoppingCartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCartViewModel> GetCartBySessionIdAsync(string sessionId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                return await CreateCartAsync(sessionId);
            }

            return MapToViewModel(cart);
        }

        public async Task<ShoppingCartViewModel> GetCartByAccountIdAsync(int accountId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (cart == null)
            {
                var sessionId = Guid.NewGuid().ToString();
                return await CreateCartAsync(sessionId, accountId);
            }
            
            return MapToViewModel(cart);
        }

        public async Task<bool> AddToCartAsync(string sessionId, AddToCartRequest request)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(sessionId);
                var product = await _context.Product
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.IsActive);

                if (product == null || product.Inventory == null || !product.Inventory.IsInStock)
                {
                    return false;
                }

                var existingItem = await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartId == cart.ShoppingCartId &&
                                               ci.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + request.Quantity;
                    if (newQuantity > product.Inventory.StockLevel)
                    {
                        return false;
                    }

                    existingItem.Quantity = newQuantity;
                    existingItem.ShoppingCart!.LastModified = DateTime.Now;
                }
                else
                {
                    if (request.Quantity > product.Inventory.StockLevel)
                    {
                        return false;
                    }

                    var cartItem = new ShoppingCartItem
                    {
                        ShoppingCartId = cart.ShoppingCartId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        UnitPrice = product.Price,
                        AddedDate = DateTime.Now
                    };

                    _context.ShoppingCartItems.Add(cartItem);
                    cart.LastModified = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
      
        public async Task<bool> AddToCartAsync(int accountId, AddToCartRequest request)
        {
            try
            {
                var cart = await GetOrCreateCartByAccountIdAsync(accountId);
                var product = await _context.Product
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.IsActive);
                
                if (product == null || product.Inventory == null || !product.Inventory.IsInStock)
                {
                    return false;
                }
                
                var existingItem = await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartId == cart.ShoppingCartId && 
                                             ci.ProductId == request.ProductId);
                
                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + request.Quantity;
                    
                    if (newQuantity > product.Inventory.StockLevel)
                    {
                        return false;
                    }
                    
                    existingItem.Quantity = newQuantity;
                    existingItem.ShoppingCart!.LastModified = DateTime.Now;
                }
                else
                {
                    if (request.Quantity > product.Inventory.StockLevel)
                    {
                        return false;
                    }
                    
                    var cartItem = new ShoppingCartItem
                    {
                        ShoppingCartId = cart.ShoppingCartId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        UnitPrice = product.Price,
                        AddedDate = DateTime.Now
                    };
                    
                    _context.ShoppingCartItems.Add(cartItem);
                    cart.LastModified = DateTime.Now;
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int newQuantity)
        {
            try
            {
                var cartItem = await _context.ShoppingCartItems
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Inventory)
                    .Include(ci => ci.ShoppingCart)
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartItemId == cartItemId);
                
                if (cartItem == null || cartItem.Product == null || cartItem.Product.Inventory == null)
                {
                    return false;
                }
                
                if (newQuantity > cartItem.Product.Inventory.StockLevel)
                {
                    return false;
                }
                
                cartItem.Quantity = newQuantity;
                cartItem.ShoppingCart!.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.ShoppingCartItems
                    .Include(ci => ci.ShoppingCart)
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartItemId == cartItemId);
                
                if (cartItem == null)
                {
                    return false;
                }
                
                _context.ShoppingCartItems.Remove(cartItem);
                cartItem.ShoppingCart!.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> ClearCartAsync(string sessionId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);
                
                if (cart == null) return true;
                
                _context.ShoppingCartItems.RemoveRange(cart.CartItems);
                cart.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> ClearCartAsync(int accountId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.AccountId == accountId);
                
                if (cart == null) return true;
                
                _context.ShoppingCartItems.RemoveRange(cart.CartItems);
                cart.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> MergeCartsAsync(string sessionId, int accountId)
        {
            try
            {
                var sessionCart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);
                
                var accountCart = await GetOrCreateCartByAccountIdAsync(accountId);
                
                if (sessionCart == null || !sessionCart.CartItems.Any())
                {
                    return true;
                }
                
                foreach (var sessionItem in sessionCart.CartItems)
                {
                    var existingAccountItem = await _context.ShoppingCartItems
                        .FirstOrDefaultAsync(ci => ci.ShoppingCartId == accountCart.ShoppingCartId && 
                                                 ci.ProductId == sessionItem.ProductId);
                    
                    if (existingAccountItem != null)
                    {
                        existingAccountItem.Quantity += sessionItem.Quantity;
                    }
                    else
                    {
                        var newItem = new ShoppingCartItem
                        {
                            ShoppingCartId = accountCart.ShoppingCartId,
                            ProductId = sessionItem.ProductId,
                            Quantity = sessionItem.Quantity,
                            UnitPrice = sessionItem.UnitPrice,
                            AddedDate = DateTime.Now
                        };
                        
                        _context.ShoppingCartItems.Add(newItem);
                    }
                }
                
                _context.ShoppingCartItems.RemoveRange(sessionCart.CartItems);
                accountCart.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<int> GetCartItemCountAsync(string sessionId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            
            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }
        
        public async Task<int> GetCartItemCountAsync(int accountId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);
            
            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }
        
        public async Task<bool> ValidateCartItemsAsync(string sessionId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            
            if (cart == null) return true;
            
            return ValidateCartItems(cart);
        }
        
        public async Task<bool> ValidateCartItemsAsync(int accountId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);
            
            if (cart == null) return true;
            
            return ValidateCartItems(cart);
        }
        
        public async Task<ShoppingCartViewModel> CreateCartAsync(string sessionId, int? accountId = null)
        {
            var cart = new ShoppingCart
            {
                SessionId = sessionId,
                AccountId = accountId,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now
            };
            
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();
            
            return MapToViewModel(cart);
        }
        
        // Private helper methods
        private async Task<ShoppingCart> GetOrCreateCartAsync(string sessionId)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    SessionId = sessionId,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                };
                
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            return cart;
        }
        
        private async Task<ShoppingCart> GetOrCreateCartByAccountIdAsync(int accountId)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.AccountId == accountId);
            
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    SessionId = Guid.NewGuid().ToString(),
                    AccountId = accountId,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                };
                
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            return cart;
        }
        
        private static ShoppingCartViewModel MapToViewModel(ShoppingCart cart)
        {
            return new ShoppingCartViewModel
            {
                ShoppingCartId = cart.ShoppingCartId,
                SessionId = cart.SessionId,
                AccountId = cart.AccountId,
                CartItems = cart.CartItems.Select(ci => new ShoppingCartItemViewModel
                {
                    ShoppingCartItemId = ci.ShoppingCartItemId,
                    ShoppingCartId = ci.ShoppingCartId,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    AddedDate = ci.AddedDate,
                    ProductName = ci.Product?.Name ?? "",
                    ProductSKU = ci.Product?.SKU ?? "",
                    ProductImagePath = ci.Product?.ImagePath,
                    ProductStockLevel = ci.Product?.Inventory?.StockLevel ?? 0
                }).ToList()
            };
        }
        
        private static bool ValidateCartItems(ShoppingCart cart)
        {
            foreach (var item in cart.CartItems)
            {
                if (item.Product == null || !item.Product.IsActive)
                {
                    return false;
                }
                
                if (item.Product.Inventory == null || item.Quantity > item.Product.Inventory.StockLevel)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}