using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CTPS;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    // -------- CAPA 3D --------
    internal static Texture2D PassportCoverTex = null!;
    private const string PassportMaterialName = "M_Passport";
    private static readonly int BaseTexId = Shader.PropertyToID("_BaseTexture");

    // -------- ÍCONE HUD --------
    internal static Texture2D PassportIconTex = null!;
    private const string PassportIconTextureName = "Passport";

    private void Awake()
    {
        Log = Logger;

        LoadCoverTexture();
        LoadIconTexture();

        new Harmony("com.github.vector-b.CTPS").PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;

        Log.LogInfo("[CTPS] Plugin carregado!");
    }


    private void LoadCoverTexture()
    {
        string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string texturePath = Path.Combine(pluginDir, "assets", "PassportUV_ctps.png");

        if (!File.Exists(texturePath))
        {
            Log.LogError("[CTPS] CTPS cover file not found!");
            return;
        }

        byte[] data = File.ReadAllBytes(texturePath);
        PassportCoverTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(PassportCoverTex, data);
        PassportCoverTex.Apply(true, true);

        Log.LogInfo("[CTPS] CTPS cover texture loaded.");
    }

    private void LoadIconTexture()
    {
        string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string iconPath = Path.Combine(pluginDir, "assets", "Passport_Icon.png");

        if (!File.Exists(iconPath))
        {
            Log.LogWarning("[CTPS] CTPS icon file not found; HUD will not be changed.");
            return;
        }

        byte[] data = File.ReadAllBytes(iconPath);
        PassportIconTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(PassportIconTex, data);
        PassportIconTex.Apply(true, true);

        Log.LogInfo($"[CTPS] CTPS icon loaded: {PassportIconTex.width}x{PassportIconTex.height}.");
    }

    // ================== M_Passport ==================

    private static bool _coverAlreadyReplaced = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_coverAlreadyReplaced)
            return;

        if (PassportCoverTex == null)
        {
            Log.LogWarning("[CTPS] PassportCoverTex is null, skipping 3D cover.");
            return;
        }

        var mats = Resources.FindObjectsOfTypeAll<Material>();
        foreach (var mat in mats)
        {
            if (mat != null &&
                mat.name == PassportMaterialName &&
                mat.HasProperty(BaseTexId))
            {
                mat.SetTexture(BaseTexId, PassportCoverTex);
                _coverAlreadyReplaced = true;
                Log.LogInfo($"[CTPS] Applied CTPS to the 3D cover of the material '{mat.name}'.");
                break;
            }
        }
    }

    [HarmonyPatch(typeof(RawImage), nameof(RawImage.texture), MethodType.Setter)]
    internal static class RawImageSetTexturePatch
    {
        private static void Prefix(ref Texture value, RawImage __instance)
        {
            if (PassportIconTex == null || value == null)
                return;

            if (value.name != PassportIconTextureName)
                return;


            Plugin.Log.LogInfo(
                $"[CTPS] RawImage.texture on '{__instance.gameObject.name}' using '{value.name}' -> changing to CTPS card."
            );

            value = PassportIconTex;
        }
    }
}
