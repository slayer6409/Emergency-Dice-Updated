using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace MysteryDice.Effects
{
    public enum EffectType
    {
        Awful,
        Bad,
        Mixed,
        Good,
        Great
    }

    /// <summary>
    /// This would probably work better as an abstract class, but if it works it works.
    /// </summary>
    public interface IEffect
    {
        EffectType Outcome { get; }
        bool ShowDefaultTooltip { get; }
        string Name { get; }
        string Tooltip { get; }
        void Use();
    }
}
