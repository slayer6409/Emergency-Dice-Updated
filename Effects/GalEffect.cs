using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace MysteryDice.Effects
{

    public interface GalEffect : IEffect
    {
        
        public EffectType RealOutcome  { get; }
        public EffectType NoGalOutcome  { get; }
    }
}
