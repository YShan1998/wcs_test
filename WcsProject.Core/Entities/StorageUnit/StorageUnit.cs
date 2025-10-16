namespace WcsProject.Core.Entities.Matrix;

[SugarTable]
public class StorageUnit : AuditEntity
{
    [SugarColumn(Length = 10, IsNullable = false)]
    public string Code { get; set; }

    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; }

    [SugarColumn(IsNullable = true)] public int SizeX { get; set; }

    [SugarColumn(IsNullable = true)] public int SizeY { get; set; }

    [SugarColumn(IsNullable = true)] public int SizeZ { get; set; }

    [SugarColumn(IsJson = true, IsNullable = true)]
    public string ExtraProperties { get; set; }
}