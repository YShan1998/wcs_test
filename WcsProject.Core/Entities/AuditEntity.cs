using SqlSugar;

namespace WcsProject.Core.Entities;

public abstract class AuditEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    
    [SugarColumn(InsertServerTime = true, IsOnlyIgnoreUpdate =true)]
    public DateTime CreatedAt { get; set; }
    
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public string CreatedBy { get; set; }
    
    [SugarColumn(UpdateServerTime = true, IsOnlyIgnoreInsert = true, IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
    
    [SugarColumn(IsOnlyIgnoreInsert = true, IsNullable = true)]
    public string UpdatedBy { get; set; }
    
    [SugarColumn(IsEnableUpdateVersionValidation = true)]
    public int Version { get; set; }
    
    [SugarColumn(ColumnDataType = "boolean", DefaultValue = "false")]
    public bool IsDeleted { get; set; }
    
    [SugarColumn(IsOnlyIgnoreInsert = true, IsNullable = true)]
    public DateTime DeletedAt { get; set; }
    
    [SugarColumn(IsOnlyIgnoreInsert = true, IsNullable = true)]
    public string DeletedBy { get; set; }
}