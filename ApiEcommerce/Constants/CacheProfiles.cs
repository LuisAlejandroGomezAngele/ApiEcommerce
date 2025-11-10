using System;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Constants
{
    public static class CacheProfiles
    {
        public const string Default30 = "Default30";
        public const string Default20 = "Default20";

        public static readonly CacheProfile Default30Profile = new()
        {
            Duration = 30
        };
        public static readonly CacheProfile Default20Profile = new()
        {
            Duration = 20
        };
    }
}