#nullable enable
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Solhigson.Framework.Infrastructure;

namespace SolhigsonAspnetCoreApp.Infrastructure.Repositories
{
    /*
     * Generated by: Solhigson.Framework.efcoretool
     *
     * https://github.com/solhigson-public/solhigson.framework
     * https://www.nuget.org/packages/solhigson.framework.efcoretool
     *
     * This file is ALWAYS overwritten, DO NOT place custom code here
     */
    public partial class CurrencyRepository : SolhigsonAspnetCoreAppCachedRepositoryBase<SolhigsonAspnetCoreApp.Domain.Entities.Currency
        ,SolhigsonAspnetCoreApp.Domain.CacheModels.CurrencyCacheModel>, 
            SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.ICurrencyRepository
    {
        public CurrencyRepository(SolhigsonAspnetCoreApp.Infrastructure.AppDbContext dbContext) : base(dbContext)
        {
        }

		public virtual async Task<SolhigsonAspnetCoreApp.Domain.Entities.Currency?> GetByIdAsync(string id)
		{
			if (id is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.Id == id;
			return await Where(query).FirstOrDefaultAsync();
		}

		public virtual async Task<TK?> GetByIdAsync<TK>(string id) where TK : class
		{
			if (id is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.Id == id;
			return await Where<TK>(query).FirstOrDefaultAsync();
		}

		public virtual async Task<SolhigsonAspnetCoreApp.Domain.Entities.Currency?> GetByAlphabeticCodeAsync(string alphabeticCode)
		{
			if (alphabeticCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.AlphabeticCode == alphabeticCode;
			return await Where(query).FirstOrDefaultAsync();
		}

		public virtual async Task<TK?> GetByAlphabeticCodeAsync<TK>(string alphabeticCode) where TK : class
		{
			if (alphabeticCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.AlphabeticCode == alphabeticCode;
			return await Where<TK>(query).FirstOrDefaultAsync();
		}

		public virtual async Task<SolhigsonAspnetCoreApp.Domain.Entities.Currency?> GetByNumericCodeAsync(string numericCode)
		{
			if (numericCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.NumericCode == numericCode;
			return await Where(query).FirstOrDefaultAsync();
		}

		public virtual async Task<TK?> GetByNumericCodeAsync<TK>(string numericCode) where TK : class
		{
			if (numericCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.NumericCode == numericCode;
			return await Where<TK>(query).FirstOrDefaultAsync();
		}


		//Cached Methods
		public virtual async Task<SolhigsonAspnetCoreApp.Domain.CacheModels.CurrencyCacheModel?> GetByIdCachedAsync(string id)
		{
			if (id is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.Id == id;
			return await GetSingleCachedAsync(query);
		}

		public virtual async Task<TK?> GetByIdCachedAsync<TK>(string id) where TK : class
		{
			if (id is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.Id == id;
			return await GetSingleCachedAsync<TK>(query);
		}

		public virtual async Task<SolhigsonAspnetCoreApp.Domain.CacheModels.CurrencyCacheModel?> GetByAlphabeticCodeCachedAsync(string alphabeticCode)
		{
			if (alphabeticCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.AlphabeticCode == alphabeticCode;
			return await GetSingleCachedAsync(query);
		}

		public virtual async Task<TK?> GetByAlphabeticCodeCachedAsync<TK>(string alphabeticCode) where TK : class
		{
			if (alphabeticCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.AlphabeticCode == alphabeticCode;
			return await GetSingleCachedAsync<TK>(query);
		}

		public virtual async Task<SolhigsonAspnetCoreApp.Domain.CacheModels.CurrencyCacheModel?> GetByNumericCodeCachedAsync(string numericCode)
		{
			if (numericCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.NumericCode == numericCode;
			return await GetSingleCachedAsync(query);
		}

		public virtual async Task<TK?> GetByNumericCodeCachedAsync<TK>(string numericCode) where TK : class
		{
			if (numericCode is null) { return null; }

			Expression<Func<SolhigsonAspnetCoreApp.Domain.Entities.Currency, bool>> query = 
				t => t.NumericCode == numericCode;
			return await GetSingleCachedAsync<TK>(query);
		}


    }
}