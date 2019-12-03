
using System.IO;

#if Editor
using Editor.Common;
#endif

namespace GEngine
{
    class ResourceAll : SingletonObject<ResourceAll>
    {
        public ResourceMapMgr MapMgr = new ResourceMapMgr();

        public void Init()
        {
            MapMgr.LoadFromFile(Path.Combine(Global.GetInstance().GetReferencePath(), "world.csv"));
        }
    }
}