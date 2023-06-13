using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Collections.Generic;
using RWCustom;
namespace SlugTemplate 
{
    [BepInPlugin(MOD_ID, "weaver", "0.0.1")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "abysslasea.weaver";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slugtemplate/super_jump");
        public static readonly PlayerFeature<bool> ExplodeOnDeath = PlayerBool("slugtemplate/explode_on_death");
        public static readonly GameFeature<float> MeanLizards = GameFloat("slugtemplate/mean_lizards");


        // Add hooks-添加钩子
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);


        }
        
        // Load any resources, such as sprites or sounds-加载任何资源 包括图像素材和音效
        private void LoadResources(RainWorld rainWorld)
        {
        }


    }
}