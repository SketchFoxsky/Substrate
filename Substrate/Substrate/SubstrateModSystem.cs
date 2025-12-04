using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Substrate.Behaviors;
using Substrate.BlockEntities;
using Substrate.Blocks;
using Substrate.Utils;
using Substrate.Utils.CodecPattern;
using Substrate.Utils.CodecPattern.Codecs;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace Substrate
{
    public class SubstrateModSystem : ModSystem
    {
        private const string ConfigFileName = "substrateconfig.json";

        internal static ILogger Logger { get; private set; }

        /// <summary>
        /// Active configuration for the Substrate mod.
        /// Loaded on the server, defaults used if no file exists.
        /// </summary>
        internal static SubstrateConfig Config { get; private set; } = new SubstrateConfig();

        #region Lifecycle

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            // Register blocks / block entities
            api.RegisterBlockClass("BlockFruitingBag", typeof(BlockFruitingBag));
            api.RegisterBlockClass("BlockGrowBed", typeof(BlockGrowBed));
            api.RegisterBlockEntityClass("FruitingBag", typeof(BlockEntityFruitingBag));

            api.RegisterBlockClass("BlockSporePaper", typeof(BlockSporePaper));
            api.RegisterBlockEntityClass("SporePaper", typeof(BlockEntitySporePaper));

            // Behaviors
            api.RegisterCollectibleBehaviorClass("UseInventoryShape", typeof(BehaviorShapeInventory));
            api.RegisterBlockBehaviorClass("BehaviorMushroomGrower", typeof(BehaviorMushroomGrower));

            // Harmony patches
            var harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll(typeof(SubstrateModSystem).Assembly);

            // Only the server needs to own the config file (SP server included)
            if (api.Side == EnumAppSide.Server)
            {
                LoadOrCreateConfig(api);
            }
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            // Add UseInventoryShape behavior to all spore-harvestable mushrooms
            foreach (var obj in api.World.Collectibles)
            {
                if (obj == null || obj.Code == null) continue;

                if (obj is BlockMushroom { Attributes: not null } bm && bm.Attributes.IsTrue("sporeharvestable"))
                {
                    obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new BehaviorShapeInventory(bm));
                }
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Logger = Mod.Logger;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Logger = Mod.Logger;
        }

        #endregion

        #region Config

        private static void LoadOrCreateConfig(ICoreAPI api)
        {
            try
            {
                var loaded = api.LoadModConfig<SubstrateConfig>(ConfigFileName);
                if (loaded != null)
                {
                    Config = loaded;
                }
                else
                {
                    // No file yet, create with defaults
                    Config = new SubstrateConfig();
                }

                // Write back to ensure the file exists / is updated with new fields
                api.StoreModConfig(Config, ConfigFileName);
                Logger?.Notification("[Substrate] Loaded config from {0}", ConfigFileName);
            }
            catch (Exception e)
            {
                Logger?.Error("[Substrate] Failed to load config {0}: {1}", ConfigFileName, e);
                Config = new SubstrateConfig();
            }
        }

        #endregion
    }
}
