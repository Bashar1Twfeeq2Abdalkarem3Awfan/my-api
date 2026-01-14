namespace MyAPIv3.DTOs
{
    public class UnitDto
    {
        public long Id { get; set; }
        public string UnitName { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class CreateUnitDto
    {
        public string UnitName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUnitDto
    {
        public string UnitName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}

