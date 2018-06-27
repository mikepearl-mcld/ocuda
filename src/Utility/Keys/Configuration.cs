﻿namespace Ocuda.Utility.Keys
{
    public struct Configuration
    {
        public static readonly string OpsAuthBlankRequestRedirect = "Ops.AuthBlankRequestRedirect";
        public static readonly string OpsAuthRedirect = "Ops.AuthRedirect";
        public static readonly string OpsCulture = "Ops.Culture";
        public static readonly string OpsDatabaseProvider = "Ops.DatabaseProvider";
        public static readonly string OpsDistributedCache = "Ops.DistributedCache";
        public static readonly string OpsDistributedCacheRedisConfiguration
            = "Ops.DistributedCache.RedisConfiguration";
        public static readonly string OpsDomainName = "Ops.DomainName";
        public static readonly string OpsHttpErrorFileTag = "Ops.HttpErrorFileTag";
        public static readonly string OpsInstance = "Ops.Instance";
        public static readonly string OpsRollingLogLocation = "Ops.RollingLogLocation";
        public static readonly string OpsSessionTimeoutMinutes = "Ops.SessionTimeoutMinutes";
    }
}