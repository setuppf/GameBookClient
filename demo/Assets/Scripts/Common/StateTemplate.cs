using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    abstract class StateTemplate<TStateEnumType, TParent>
    {
        protected TParent _parentObj;

        public void SetParentObj(TParent pObj)
        {
            _parentObj = pObj;
        }

        public abstract TStateEnumType GetState();
        public abstract TStateEnumType Update();

        public abstract void EnterState(TStateEnumType lastStateType);
        public abstract void LeaveState();
    }

    delegate T StateTemplateCreateFun<T>();

    class StateTemplateCreator<TStateEnumType, TStateTemplate>
    {
        public TStateEnumType StateType;
        public StateTemplateCreateFun<TStateTemplate> GetInstance;

        public StateTemplateCreator(TStateEnumType stateType, StateTemplateCreateFun<TStateTemplate> getInstance)
        {
            StateType = stateType;
            GetInstance = getInstance;
        }
    }

    abstract class StateTemplateMgr<TStateEnumType, TParent> where TParent : class 
    {
        private StateTemplate<TStateEnumType, TParent> _pState;
        private TStateEnumType _lastStateType;
        private readonly Dictionary<TStateEnumType, StateTemplateCreator<TStateEnumType, StateTemplate<TStateEnumType, TParent>>> _dynCreateMap = new Dictionary<TStateEnumType, StateTemplateCreator<TStateEnumType, StateTemplate<TStateEnumType, TParent>>>();

        public void InitStateTemplateMgr(TStateEnumType defaultState)
        {
            RegisterState();
            _lastStateType = defaultState;
            ChangeState(defaultState);
        }

        protected abstract void RegisterState();
        protected void RegisterStateClass(TStateEnumType enumValue, StateTemplateCreator<TStateEnumType, StateTemplate<TStateEnumType, TParent>> np)
        {
            _dynCreateMap[enumValue] = np;
        }

        public void ChangeState(TStateEnumType stateType)
        {
            var pNewState = CreateStateObj(stateType);
            if (pNewState == null)
                return;

            if (_pState != null)
            {
                _pState.LeaveState();
                _lastStateType = _pState.GetState();
            }

            _pState = pNewState;
            _pState.EnterState(_lastStateType);
        }

        private StateTemplate<TStateEnumType, TParent> CreateStateObj(TStateEnumType stateType)
        {
            if (!_dynCreateMap.TryGetValue(stateType, out var createFun))
                return default(StateTemplate<TStateEnumType, TParent>);

            if (!(createFun.GetInstance() is StateTemplate<TStateEnumType, TParent> pNewState))
                return default(StateTemplate<TStateEnumType, TParent>);

            pNewState.SetParentObj(this as TParent);
            return pNewState;
        }

        public void UpdateState()
        {
            if (_pState == null)
                return;

            var curState = _pState.Update();
            if (!_pState.GetState().Equals(curState))
            {
                ChangeState(curState);
            }
        }
    }
}
