﻿using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Products;

public class ProductsMockService : IProductsService
{
    private readonly IServicesMockData _data;

    public ProductsMockService(IServicesMockData data)
    {
        _data = data;
    }

    public Task<Product> FindByAccountAddressAsync([NotEmpty, NotNull] string accountAddress)
    {
        Guard.Against.NullOrEmpty(accountAddress, nameof(accountAddress));

        Product result = _data.Products.FirstOrDefault(x => x.Author.Account.Address == accountAddress);
        return Task.FromResult(result);
    }

    public Task<int> GetCountAsync()
    {
        int result = _data.Products.Count;
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<Product>> GetAllAsync()
    {
        ObservableCollection<Product> result = new(_data.Products);
        return Task.FromResult(result);
    }
}
