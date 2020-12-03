// Create an AssetBundle for Windows.
using UnityEngine;
using UnityEditor;

public class ExportAssetBundles : MonoBehaviour
{
    [UnityEditor.MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("AssetBundles", BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
    }
}