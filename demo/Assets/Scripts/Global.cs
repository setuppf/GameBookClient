using UnityEngine;

namespace GEngine
{
    class Global : SingletonObject<Global>
    {

        private string _resPath;
        private string _referencePath;
        private string _serverIp;
        private int _serverPort;

        public void SetServer(string ip, int port)
        {
            _serverIp = ip;
            _serverPort = port;
        }

        public void SetResPath(string path)
        {
            GameLogger.GetInstance().Output($"Res Path:{path}");
            _resPath = path;
        }

        public string GetServerIp()
        {
            return _serverIp;
        }

        public int GetServerPort()
        {
            return _serverPort;
        }


        public string GetResPath()
        {
            return _resPath;
        }

        public void SetReferencePath(string path)
        {
            GameLogger.GetInstance().Output($"Csv Path:{path}");
            _referencePath = path;
        }

        public string GetReferencePath()
        {
            return _referencePath;
        }
    }
}

