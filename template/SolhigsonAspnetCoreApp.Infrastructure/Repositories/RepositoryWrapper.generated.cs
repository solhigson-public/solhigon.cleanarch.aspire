#nullable enable

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
    public partial class RepositoryWrapper : SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IRepositoryWrapper
    {
        public SolhigsonAspnetCoreApp.Infrastructure.AppDbContext DbContext { get; }

		private SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.ICountryRepository _countryRepository;
		public virtual SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.ICountryRepository CountryRepository
		{ get { return _countryRepository ??= new SolhigsonAspnetCoreApp.Infrastructure.Repositories.CountryRepository(DbContext); } }

		private SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.ICurrencyRepository _currencyRepository;
		public virtual SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.ICurrencyRepository CurrencyRepository
		{ get { return _currencyRepository ??= new SolhigsonAspnetCoreApp.Infrastructure.Repositories.CurrencyRepository(DbContext); } }

		private SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IInstitutionRepository _institutionRepository;
		public virtual SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions.IInstitutionRepository InstitutionRepository
		{ get { return _institutionRepository ??= new SolhigsonAspnetCoreApp.Infrastructure.Repositories.InstitutionRepository(DbContext); } }


        public RepositoryWrapper(SolhigsonAspnetCoreApp.Infrastructure.AppDbContext dbContext)
        {
            DbContext = dbContext;
        }
        
        public System.Threading.Tasks.Task SaveChangesAsync()
        {
            return DbContext.SaveChangesAsync();
        }
                
        public int SaveChanges()
        {
            return DbContext.SaveChanges();
        }
    }
}