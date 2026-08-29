namespace ImageDrop.Persistence.Repositories.Base;

public interface IBaseRepository<T> where T : class
{
    protected internal ImageDropContext Context { get; set; }

    public async Task Add(T entity) 
        => await Context.Set<T>().AddAsync(entity);
    
    public async Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) 
        => await Context.Set<T>().AnyAsync(predicate, cancellationToken);
    
    public async Task<T?> GetEntityWithTracking(Expression<Func<T, bool>> predicateExpression, CancellationToken cancellationToken = default)
        => await Context.Set<T>().Where(predicateExpression).FirstOrDefaultAsync(cancellationToken);
    
    public async Task<T?> GetEntityWithoutTracking(Expression<Func<T, bool>> predicateExpression, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AsNoTracking().Where(predicateExpression).FirstOrDefaultAsync(cancellationToken);

    public async Task<TResult?> GetSelectedItem<TResult>(Expression<Func<T, TResult>> select, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) 
        => await Context.Set<T>()
            .Where(predicate)
            .Select(select)
            .FirstOrDefaultAsync(cancellationToken);
    
    public async Task<ICollection<TResult>> GetSelectedItems<TResult>(
        Expression<Func<T, TResult>> select, 
        Expression<Func<T, bool>> predicate, 
        Expression<Func<T, object>>? orderBy = null, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Context.Set<T>().Where(predicate);
        
        if (orderBy is not null)
            query = query.OrderBy(orderBy);
        
        return await query.Select(select)
            .ToListAsync(cancellationToken);
    } 
        
    
    public async Task Delete(Expression<Func<T, bool>> predicateExpression)
        => await Context.Set<T>().Where(predicateExpression).ExecuteDeleteAsync();
}