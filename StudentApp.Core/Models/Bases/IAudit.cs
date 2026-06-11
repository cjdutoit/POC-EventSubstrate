// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace Glory2Him.Core.Models.Bases
{
    public interface IAudit
    {
        string CreatedBy { get; set; }
        DateTimeOffset CreatedWhen { get; set; }
        string UpdatedBy { get; set; }
        DateTimeOffset UpdatedWhen { get; set; }
        bool IsDeleted { get; set; }
        string? DeletedBy { get; set; }
        DateTimeOffset? DeletedWhen { get; set; }
        string? DeletionReason { get; set; }
    }
}
