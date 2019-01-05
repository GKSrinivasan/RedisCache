using Laserbeam.DataManager.Common;
using Laserbeam.DataManager.HR.Analytics;
using Laserbeam.DataManager.HR.Common;
using Laserbeam.DataManager.HR.Interfaces.Analytics;
using Laserbeam.DataManager.HR.Interfaces.Common;
using Laserbeam.DataManager.HR.Interfaces.Merit;
using Laserbeam.DataManager.HR.Interfaces.MeritMatrix;
using Laserbeam.DataManager.HR.Merit;
using Laserbeam.DataManager.HR.MeritMatrix;
using Laserbeam.DataManager.Interfaces.Common;
using Laserbeam.Libraries.Common;
using Laserbeam.Libraries.Interfaces.Common;
using Laserbeam.ProcessManager.Common;
using Laserbeam.ProcessManager.HR.Analytics;
using Laserbeam.ProcessManager.HR.Common;
using Laserbeam.ProcessManager.HR.Interfaces.Analytics;
using Laserbeam.ProcessManager.HR.Interfaces.Common;
using Laserbeam.ProcessManager.HR.Interfaces.Merit;
using Laserbeam.ProcessManager.HR.Interfaces.MeritMatrix;
using Laserbeam.ProcessManager.HR.Merit;
using Laserbeam.ProcessManager.HR.MeritMatrix;
using Laserbeam.ProcessManager.Interfaces.Common;
using StructureMap;
using System.Configuration;

namespace Laserbeam.RegistryManager.HR
{    
    public static class DependencyRegistry
    {
        #region Static Methods

        // Author         :   Raja Ganapathy
        // Creation Date  :   05-Jul-2016     
        // Reviewed By    :  Srinivasan Kamalakannan
        // Reviewed Date  :  31-Aug-2016 
        // Comments       :   Added process manager and repository in registry        
        /// <summary>
        /// Registers the process manager and repository in container
        /// </summary>        
        /// <param name="container">Instance of structuremap container</param>
        /// <returns>Returns true on successfull configuration and false on failure</returns>
        public static void Register(IContainer container)
        {
            string tenantName = ConfigurationManager.AppSettings["TenantName"].ToString();
            container.Configure(c =>
            {
                c.For<IRedisCacheProvider>().Add(new RedisCacheProvider(tenantName)).Singleton();
                c.For<IBaseRepository>().Use<BaseRepository>().Transient();
            });
            container.Configure(c =>
            {
                //DataManager
                c.For<IAppUserRepository>().Use<AppUserRepository>().Transient();
                c.For<ICompensationRepository>().Use<CompensationRepository>().Transient();                
                c.For<IDashboardRepository>().Use<DashboardRepository>().Transient();                
                c.For<IEmailDetailsRepository>().Use<EmailDetailsRepository>().Transient();                
                c.For<IMeritMatrixRepository>().Use<MeritMatrixRepository>().Transient();
                c.For<IRatingDistributionRepository>().Use<RatingDistributionRepository>().Transient();
                c.For<ISessionRepository>().Use<SessionRepository>().Transient();                
                c.For<IBonusMatrixRepository>().Use<BonusMatrixRepository>().Transient();
                c.For<IMeritMetrixAndBudgetPlanRepository>().Use<MeritMetrixAndBudgetPlanRepository>().Transient();
                c.For<ITeamReviewRepository>().Use<TeamReviewRepository>().Transient();
                c.For<ISelfReviewRepository>().Use<SelfReviewRepository>().Transient();
                c.For<ICollaborationRepository>().Use<CollaborationRepository>().Transient();

                //ProcessManager
                c.For<IAccountProcessManager>().Use<AccountProcessManager>().Transient();                                
                c.For<ICompensationProcessManager>().Use<CompensationProcessManager>().Transient();
                c.For<ICommentProcessManager>().Use<CommentProcessManager>().Transient();
                c.For<IDashboardProcessManager>().Use<DashboardProcessManager>().Transient();
                c.For<IEmailProcessManager>().Use<EmailProcessManager>().Transient();                
                c.For<ILayoutProcessManager>().Use<LayoutProcessManager>().Transient();                
                c.For<IMeritMatrixProcessManager>().Use<MeritMatrixProcessManager>().Transient();                
                c.For<IRatingDistributionProcessManager>().Use<RatingDistributionProcessManager>().Transient();
                c.For<ISessionProcessManager>().Use<SessionProcessManager>().Transient();                                
                c.For<IEmail>().Use<Email>().Transient();
                c.For<IExport>().Use<Export>().Transient();
                c.For<IKeyGenerator>().Use<KeyGenerator>().Transient();
                c.For<IPasswordEncryption>().Use<PasswordEncryption>().Transient();                
                c.For<IBonusMatrixProcessManager>().Use<BonusMatrixProcessManager>().Transient();
                c.For<IMeritMetrixAndBudgetPlanProcessManager>().Use<MeritMetrixAndBudgetPlanProcessManager>().Transient();
                c.For<ITeamReviewProcessManager>().Use<TeamReviewProcessManager>().Transient();
                c.For<ISelfReviewProcessManager>().Use<SelfReviewProcessManager>().Transient();
                c.For<ICollaborationProcessManager>().Use<CollaborationProcessManager>().Transient();

                //c.For<ITenantCacheProvider>().Use<TenantCacheProvider>().Transient();
                c.For<ITenantCacheProvider>().Use(new TenantCacheProvider(container.GetInstance<IBaseRepository>(), container.GetInstance<IRedisCacheProvider>())).Transient();
            });           
        }

        #endregion
    }
}
