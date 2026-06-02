// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;

namespace StudentApp.Security.Client.Models.Clients
{
    public class SecurityConfigurations
    {
        public string CreatedByPropertyName { get; set; } = "CreatedBy";
        public Type CreatedByPropertyType { get; set; } = typeof(string);
        public string CreatedDatePropertyName { get; set; } = "CreatedWhen";
        public Type CreatedDatePropertyType { get; set; } = typeof(DateTimeOffset);
        public string UpdatedByPropertyName { get; set; } = "UpdatedBy";
        public Type UpdatedByPropertyType { get; set; } = typeof(string);
        public string UpdatedDatePropertyName { get; set; } = "UpdatedWhen";
        public Type UpdatedDatePropertyType { get; set; } = typeof(DateTimeOffset);
        public string DeletedByPropertyName { get; set; } = "DeletedBy";
        public Type DeletedByPropertyType { get; set; } = typeof(string);
        public string DeletedDatePropertyName { get; set; } = "DeletedWhen";
        public Type DeletedDatePropertyType { get; set; } = typeof(DateTimeOffset);
    }
}
