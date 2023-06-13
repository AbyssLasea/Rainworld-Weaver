using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace weaver
{
    public class state
    {
        public class Mode : ExtEnum<Mode>
        {
            public static Mode Retracted;

            public static Mode ShootingOut;

            public static Mode AttachedToTerrain;

            public static Mode AttachedToObject;

            public static Mode Retracting;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public Mode(string value, bool register = false)
            {
                throw null;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static Mode()
            {
                throw null;
            }
        }
    }
}
