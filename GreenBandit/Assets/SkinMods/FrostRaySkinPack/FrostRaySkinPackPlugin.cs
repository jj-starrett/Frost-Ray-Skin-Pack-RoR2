using BepInEx;
using BepInEx.Logging;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;

#pragma warning disable CS0618 // Type or member is obsolete
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: R2API.Utils.ManualNetworkRegistration]
[assembly: EnigmaticThunder.Util.ManualNetworkRegistration]
namespace FrostRaySkinPack
{
    
    [BepInPlugin("com.FrostRay.FrostRaySkinPack","FrostRaySkinPack","1.0.0")]
    public partial class FrostRaySkinPackPlugin : BaseUnityPlugin
    {
        internal static FrostRaySkinPackPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger => Instance?.Logger;
        
        private static AssetBundle assetBundle;
        private static readonly List<Material> materialsWithRoRShader = new List<Material>();
        private void Awake()
        {
            Instance = this;
            BeforeAwake();
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FrostRaySkinPack.frostrayfrostrayskinpack"))
            {
                assetBundle = AssetBundle.LoadFromStream(assetStream);
            }

            BodyCatalog.availability.CallWhenAvailable(BodyCatalogInit);
            HookEndpointManager.Add(typeof(Language).GetMethod(nameof(Language.LoadStrings)), (Action<Action<Language>, Language>)LanguageLoadStrings);

            ReplaceShaders();

            AfterAwake();
        }

        partial void BeforeAwake();
        partial void AfterAwake();
        static partial void BeforeBodyCatalogInit();
        static partial void AfterBodyCatalogInit();

        private static void ReplaceShaders()
        {
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marBanditBody.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marGreenBandit.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marGreenBanditShotgun.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marGreenBanditHat.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marKitsune.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marHuntress.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/marHuntressCape.mat", @"Hopoo Games/Deferred/Standard"));
        }

        private static Material LoadMaterialWithReplacedShader(string materialPath, string shaderName)
        {
            var material = assetBundle.LoadAsset<Material>(materialPath);
            material.shader = Shader.Find(shaderName);

            return material;
        }

        private static void LanguageLoadStrings(Action<Language> orig, Language self)
        {
            orig(self);

            self.SetStringByToken("FROSTRAY_SKIN_GREENBANDITSKIN_NAME", "Bushwhacked");
            self.SetStringByToken("FROSTRAY_SKIN_MERCKITSUNE_NAME", "Kitsune");
            self.SetStringByToken("FROSTRAY_SKIN_HUNTRESSRANGER_NAME", "Serpentine");

        }

        private static void Nothing(Action<SkinDef> orig, SkinDef self)
        {

        }

        private static void BodyCatalogInit()
        {
            BeforeBodyCatalogInit();

            var awake = typeof(SkinDef).GetMethod(nameof(SkinDef.Awake), BindingFlags.NonPublic | BindingFlags.Instance);
            HookEndpointManager.Add(awake, (Action<Action<SkinDef>, SkinDef>)Nothing);

            AddBandit2BodyGreenBanditSkinSkin();
            AddMercBodyMercKitsuneSkin();
            AddHuntressBodyHuntressRangerSkin();
            
            HookEndpointManager.Remove(awake, (Action<Action<SkinDef>, SkinDef>)Nothing);

            AfterBodyCatalogInit();
        }

        static partial void Bandit2BodyGreenBanditSkinSkinAdded(SkinDef skinDef, GameObject bodyPrefab);

        private static void AddBandit2BodyGreenBanditSkinSkin()
        {
            if (!Instance.Config.Bind("GreenBanditSkin", "Enabled", true).Value)
            {
                return;
            }
            var bodyName = "Bandit2Body";
            var skinName = "GreenBanditSkin";
            try
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
                var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                var mdl = modelLocator.modelTransform.gameObject;
                var skinController = mdl.GetComponent<ModelSkinController>();

                var renderers = mdl.GetComponentsInChildren<Renderer>(true);

                var skin = ScriptableObject.CreateInstance<SkinDef>();
                skin.icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\FrostRaySkinPack\Icons\GreenBanditSkinIcon.png");
                skin.name = skinName;
                skin.nameToken = "FROSTRAY_SKIN_GREENBANDITSKIN_NAME";
                skin.rootObject = mdl;
                skin.baseSkins = new SkinDef[] 
                { 
                    skinController.skins[0],
                };
                skin.unlockableDef = null;
                skin.gameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
                skin.rendererInfos = new CharacterModel.RendererInfo[]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marBanditBody.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[2]
                    },
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marGreenBandit.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[3]
                    },
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marGreenBanditShotgun.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[4]
                    },
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marGreenBanditHat.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[6]
                    },
                };
                skin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\Bandit2BodyMesh.mesh"),
                        renderer = renderers[2]
                    },
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\Bandit2CoatMesh.mesh"),
                        renderer = renderers[3]
                    },
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\BanditShotgunMesh.mesh"),
                        renderer = renderers[4]
                    },
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\Bandit2HatMesh.mesh"),
                        renderer = renderers[6]
                    },
                };
                skin.minionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                skin.projectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = skin;

                BodyCatalog.skins[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
                Bandit2BodyGreenBanditSkinSkinAdded(skin, bodyPrefab);
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                InstanceLogger.LogError(e);
            }
        }

        static partial void MercBodyMercKitsuneSkinAdded(SkinDef skinDef, GameObject bodyPrefab);

        private static void AddMercBodyMercKitsuneSkin()
        {
            if (!Instance.Config.Bind("MercKitsune", "Enabled", true).Value)
            {
                return;
            }
            var bodyName = "MercBody";
            var skinName = "MercKitsune";
            try
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
                var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                var mdl = modelLocator.modelTransform.gameObject;
                var skinController = mdl.GetComponent<ModelSkinController>();

                var renderers = mdl.GetComponentsInChildren<Renderer>(true);

                var skin = ScriptableObject.CreateInstance<SkinDef>();
                skin.icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\FrostRaySkinPack\Icons\MercKitsuneIcon.png");
                skin.name = skinName;
                skin.nameToken = "FROSTRAY_SKIN_MERCKITSUNE_NAME";
                skin.rootObject = mdl;
                skin.baseSkins = new SkinDef[] 
                { 
                    skinController.skins[0],
                };
                skin.unlockableDef = null;
                skin.gameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
                skin.rendererInfos = new CharacterModel.RendererInfo[]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marKitsune.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[3]
                    },
                };
                skin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\MercMesh.mesh"),
                        renderer = renderers[3]
                    },
                };
                skin.minionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                skin.projectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = skin;

                BodyCatalog.skins[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
                MercBodyMercKitsuneSkinAdded(skin, bodyPrefab);
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                InstanceLogger.LogError(e);
            }
        }

        static partial void HuntressBodyHuntressRangerSkinAdded(SkinDef skinDef, GameObject bodyPrefab);

        private static void AddHuntressBodyHuntressRangerSkin()
        {
            if (!Instance.Config.Bind("HuntressRanger", "Enabled", true).Value)
            {
                return;
            }
            var bodyName = "HuntressBody";
            var skinName = "HuntressRanger";
            try
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);
                var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                var mdl = modelLocator.modelTransform.gameObject;
                var skinController = mdl.GetComponent<ModelSkinController>();

                var renderers = mdl.GetComponentsInChildren<Renderer>(true);

                var skin = ScriptableObject.CreateInstance<SkinDef>();
                skin.icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\FrostRaySkinPack\Icons\HuntressRangerIcon.png");
                skin.name = skinName;
                skin.nameToken = "FROSTRAY_SKIN_HUNTRESSRANGER_NAME";
                skin.rootObject = mdl;
                skin.baseSkins = new SkinDef[] 
                { 
                    skinController.skins[0],
                };
                skin.unlockableDef = null;
                skin.gameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
                skin.rendererInfos = new CharacterModel.RendererInfo[]
                {
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marHuntress.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[10]
                    },
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marHuntressCape.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                        ignoreOverlays = false,
                        renderer = renderers[11]
                    },
                    new CharacterModel.RendererInfo
                    {
                        defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/marHuntress.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                        ignoreOverlays = false,
                        renderer = renderers[1]
                    },
                };
                skin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\HuntressMesh.mesh"),
                        renderer = renderers[10]
                    },
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\FrostRaySkinPack\Meshes\BowMesh.mesh"),
                        renderer = renderers[1]
                    },
                };
                skin.minionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                skin.projectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = skin;

                BodyCatalog.skins[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
                HuntressBodyHuntressRangerSkinAdded(skin, bodyPrefab);
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                InstanceLogger.LogError(e);
            }
        }
    }

}

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}

namespace EnigmaticThunder.Util
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}
