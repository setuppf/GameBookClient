using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GEngine {
    static class UiUtil {
        public static Transform GetTransform( GameObject gameObj, string name ) {
            if ( string.IsNullOrEmpty( name ) )
                return gameObj.transform;

            string[] names = name.Split( '/', '\\' );
            Transform tf = gameObj.transform.Find( names[0] );
            for ( int i = 1; i < names.Length; i++ ) {
                if ( tf == null )
                    return null;

                tf = tf.Find( names[i] );
            }
            return tf;
        }
    }
}
