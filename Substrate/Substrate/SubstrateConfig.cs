using System;

namespace Substrate
{
    /// <summary>
    /// Configuration for the Substrate mod.
    /// Serialized to /ModConfig/substrateconfig.json.
    /// </summary>
    [Serializable]
    public class SubstrateConfig
    {
        /// <summary>
        /// Global multiplier for mushroom harvest size.
        /// 1.0 = default behavior, 0.5 = half, 2.0 = double, etc.
        /// </summary>
        public float HarvestMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// Clamp for the final effective grow chance (0..1 typically).
        /// Keeps things from going too crazy if HarvestMultiplier is very high.
        /// </summary>
        public float MaxEffectiveChance { get; set; } = 1.0f;
    }
}
