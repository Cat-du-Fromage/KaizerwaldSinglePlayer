using System;

namespace Kaizerwald
{
    //TODO: Pour le moment on peut avoir un joueur avec des troupes dans plusieurs team différentes!(A CORRIGER)
    [Serializable]
    public struct RegimentSpawner
    {
        public ulong PlayerID;
        public int TeamID;
        public int Number;
        public RegimentType RegimentType;
    }
}