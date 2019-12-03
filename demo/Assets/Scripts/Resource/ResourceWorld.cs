
namespace GEngine
{

    enum ResourceWorldType
    {
        [EditorEnum("登录")]
        Login = 1,

        [EditorEnum("角色选择")]
        Roles = 2, // 角色选择场景

        [EditorEnum("公共地图")]
        Public = 3,

        [EditorEnum("副本地图")]
        Dungeon = 4,
    };

    class ResourceWorld : Reference
    {

        private bool IsMapType(ResourceWorldType type)
        {
            int value = GetInt("Type");
            if (value == (int)type)
                return true;

            return false;
        }

        public bool IsLobby()
        {
            return IsMapType(ResourceWorldType.Roles);
        }

        [Column("ID", 80)]
        public int Id => _id;

        [Column("名字", 100)] 
        public string Name { get; set; }

        [Column("初始地图", 100)]
        public bool Init { get; set; }

        [Column("类型", 100, "GEngine.ResourceWorldType")]
        public int Type { get; set; }

        [Column("AB包路径", 260)]
        public string AbPath { get; set; }

        [Column("资源名", 100)]
        public string ResName { get; set; }

        [Column("初始UI", 100, "GEngine.UiType")]
        public int UiRes { get; set; }

        [Column("初始位置", 130)]
        public string PlayerInitPos { get; set; }

        public override void LoadAfter()
        {
            Name = GetString("Name");
            AbPath = GetString("AbPath");
            ResName = GetString("ResName");
            UiRes = GetInt("UiResType");
            Init = GetBool("Init");
            Type = GetInt("Type");
            PlayerInitPos = GetString("PlayerInitPos");
        }
    }

    class ResourceMapMgr : ReferenceMgr<ResourceWorld> { }

}