using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine {
    class CvsAnalysis : SingletonObject<CvsAnalysis> {


        public string[] GetProperty( string line ) {
            List<string> properties = new List<string>( );

            bool isQuoted = false;
            int index = 0;
            do {
                isQuoted = line[0] == '\"';

                if( isQuoted ) {
                    line = line.Remove( 0, 1 );
                    index = line.IndexOf( "\"", StringComparison.Ordinal );
                } else {
                    index = line.IndexOf( ",", StringComparison.Ordinal );
                }

                if( index > 0 ) {
                    properties.Add( line.Substring( 0, index ) );
                    line = line.Remove( 0, index + 1 );
                } else {
                    properties.Add( line );
                    break;
                }

                if( string.IsNullOrEmpty( line ) )
                    break;

            } while( true );

            return properties.ToArray( );
        }
    }
}
