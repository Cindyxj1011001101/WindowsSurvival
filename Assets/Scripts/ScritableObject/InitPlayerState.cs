using UnityEngine;

namespace ScritableObject
{
    [CreateAssetMenu(fileName = "InitPlayerStateData", menuName = "ScritableObject/InitPlayerStateData")]
    public class InitPlayerStateData:SingleScriptableObject<InitPlayerStateData>
    {
        public int Oxygen;
        public int Health;
        public int Fullness;
        public int Thirst;
        public int Tired;
        public int San;
    }
}   