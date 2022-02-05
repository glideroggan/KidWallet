using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Data.DTOs;

namespace server.Repositories;

public interface IRepo<TModel>
{
    IQueryable<TModel> GetAll(WalletContext ctx);
    ValueTask<TModel?> GetByIdAsync(WalletContext ctx, int id);
    ValueTask<TModel> GetBy2Id(WalletContext ctx, params int[] id);
    Task UpdateAsync(WalletContext ctx, TModel model);
    Task AddOrUpdateAsync(WalletContext ctx, TModel model);
    TModel Add(WalletContext ctx, TModel model);
    Task SaveAsync(WalletContext ctx);
    void Remove(WalletContext ctx, TModel model);
    void ExecuteSqlCommand(WalletContext ctx, string v);
    Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction CreateTransaction(WalletContext ctx);
    TModel Reload(WalletContext ctx, TModel entity);
}

public class Repo<T> : IRepo<T> where T : class
{
    public T Add(WalletContext ctx,T model)
    {
        return ctx.Set<T>().Add(model).Entity;

    }

    public async Task AddOrUpdateAsync(WalletContext ctx, T stat)
    {
        await UpdateAsync(ctx, stat);
    }

    public IQueryable<T> GetAll(WalletContext ctx)
    {
        return ctx.Set<T>();
    }

    public async ValueTask<T?> GetByIdAsync(WalletContext ctx, int id)
    {
        return await ctx
            .Set<T>()
            .FindAsync(id);
    }

    public async ValueTask<T?> GetBy2Id(WalletContext ctx, params int[] id)
    {
        return await ctx.Set<T>().FindAsync(id[0], id[1]);
    }

    public void Remove(WalletContext ctx, T model)
    {
        ctx.Set<T>().Remove(model);
    }

    public async Task SaveAsync(WalletContext ctx)
    {
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(WalletContext ctx, T model)
    {
        ctx.Set<T>().Update(model);
    }

    public void ExecuteSqlCommand(WalletContext ctx, string v)
    {
        ctx.Database.ExecuteSqlRaw(v);
    }

    public Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction CreateTransaction(WalletContext ctx)
    {
        return ctx.Database.BeginTransaction();
    }

    public T Reload(WalletContext ctx, T entity)
    {
        ctx.Entry(entity).Reload();
        return ctx.Entry(entity).Entity;
    }
}
