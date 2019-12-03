using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GEngine
{

    class ToUiPlayerProperies : ToUiData
    {
        public ulong Id;
        public string Name;
        public Proto.Gender Gender;
    }

    class ToUiAccountInfo : ToUiData
    {
        public string Account;
        public List<ToUiPlayerProperies> Players = new List<ToUiPlayerProperies>();
    }

    class PlayerLitter
    {
        private ulong _id;
        public ulong Id => _id;

        private string _name;
        public string Name => _name;

        private Proto.Gender _gender;
        public Proto.Gender Gender => _gender;

        private int _level;
        public int Level => _level;

        public void Parse(Proto.PlayerLittle proto)
        {
            _id = proto.Sn;
            _name = proto.Name;
            _gender = proto.Gender;
            _level = proto.Level;
        }
    }

    class AccountInfo : IToUi<ToUiAccountInfo>
    {

        private string _account;
        public string Account => _account;

        private readonly List<PlayerLitter> _players = new List<PlayerLitter>();
        public List<PlayerLitter> Players => _players;

        public AccountInfo() : base(UiUpdateDataType.AccountInfo)
        {
        }

        public void Parse(Proto.PlayerList proto)
        {
            _account = proto.Account;
            _players.Clear();

            foreach (Proto.PlayerLittle one in proto.Player)
            {
                PlayerLitter info = new PlayerLitter();
                info.Parse(one);
                _players.Add(info);
            }

            ToUi();
        }

        protected override void ToUi()
        {
            ToUiAccountInfo updateObj = new ToUiAccountInfo {Account = _account};

            foreach (var one in _players)
            {
                ToUiPlayerProperies uiOne = new ToUiPlayerProperies { Name = one.Name, Gender = one.Gender, Id = one.Id };
                updateObj.Players.Add(uiOne);
            }

            UpdataUiData(updateObj);
        }
    }
}
