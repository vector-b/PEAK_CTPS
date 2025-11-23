using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WorkCard;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    // -------- CAPA 3D --------
    internal static Texture2D PassportCoverTex;
    private const string PassportMaterialName = "M_Passport";
    private static readonly int BaseTexId = Shader.PropertyToID("_BaseTexture");

    // -------- ÍCONE HUD --------
    internal static Texture2D PassportIconTex;
    // nome da textura original do ícone
    private const string PassportIconTextureName = "Passport";

    private void Awake()
    {
        Log = Logger;

        LoadCoverTexture();
        LoadIconTexture();

        new Harmony("com.github.vector-b.WorkCard").PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;

        Log.LogInfo("[WorkCard] Plugin carregado!");
    }


    private void LoadCoverTexture()
    {
        // BepInEx/plugins/WorkCard/assets/PassportUV_ctps.png
        string texturePath = Path.Combine(
            Paths.PluginPath,
            "WorkCard",
            "assets",
            "PassportUV_ctps.png"
        );

        if (!File.Exists(texturePath))
        {
            Log.LogError("[WorkCard] CTPS cover file not found!");
            return;
        }

        byte[] data = File.ReadAllBytes(texturePath);
        PassportCoverTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(PassportCoverTex, data);
        PassportCoverTex.Apply(true, true);

        Log.LogInfo("[WorkCard] CTPS cover texture loaded.");
    }

    private void LoadIconTexture()
    {
        // BepInEx/plugins/WorkCard/assets/Passport_Icon.png
        string iconPath = Path.Combine(
            Paths.PluginPath,
            "WorkCard",
            "assets",
            "Passport_Icon.png"
        );

        if (!File.Exists(iconPath))
        {
            Log.LogWarning("[WorkCard] CTPS icon file not found; HUD will not be changed.");
            return;
        }

        byte[] data = File.ReadAllBytes(iconPath);
        PassportIconTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(PassportIconTex, data);
        PassportIconTex.Apply(true, true);

        Log.LogInfo($"[WorkCard] CTPS icon loaded: {PassportIconTex.width}x{PassportIconTex.height}.");
    }

    // ================== M_Passport ==================

    private static bool _coverAlreadyReplaced = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_coverAlreadyReplaced)
            return;

        if (PassportCoverTex == null)
        {
            Log.LogWarning("[WorkCard] PassportCoverTex is null, skipping 3D cover.");
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
                Log.LogInfo($"[WorkCard] Applied CTPS to the 3D cover of the material '{mat.name}'.");
                break;
            }
        }
    }

    [HarmonyPatch(typeof(RawImage), "set_texture")]
    internal static class RawImageSetTexturePatch
    {
        private static void Prefix(ref Texture __0, RawImage __instance)
        {
            if (PassportIconTex == null || __0 == null)
                return;

            if (__0.name != PassportIconTextureName)
                return;


            Plugin.Log.LogInfo(
                $"[WorkCard] RawImage.set_texture on'{__instance.gameObject.name}' using '{__0.name}' -> changing to CTPS card."
            );

            __0 = PassportIconTex;
        }
    }
}
