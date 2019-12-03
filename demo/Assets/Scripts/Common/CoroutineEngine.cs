using System.Collections;
using UnityEngine;

namespace GEngine {
    class CoroutineEngine : SingletonBehaviour<CoroutineEngine> {
        protected override void OnAwake( ) { }

        public Coroutine Execute( IEnumerator routine ) {
            return _singleton.StartCoroutine( routine );
        }
    }
}
