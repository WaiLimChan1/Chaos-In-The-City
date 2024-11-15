//using System;
//using System.Reflection;
//using UnityEditor;
//using UnityEngine;
//using Object = UnityEngine.Object;

//[CustomEditor(typeof(Tile_SO), true)]
//[CanEditMultipleObjects]
//public class Tile_SOEditor : Editor
//{
//    #region Update Project Icon To Tile Sprite
//    //-------------------------------------------------------------------------------------------------------------------------------------
//    // Get reference to the Tile_SO object
//    private Tile_SO tile { get { return (target as Tile_SO); } }

//    // Override the static preview rendering method to show sprite previews in the Project window
//    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
//    {
//        // Check if the tileSprite exists
//        if (tile.tileSprite != null)
//        {
//            // Get Unity's internal SpriteUtility class using reflection
//            Type t = GetType("UnityEditor.SpriteUtility");
//            if (t != null)
//            {
//                // Use reflection to find the method that renders the static preview of a sprite
//                MethodInfo method = t.GetMethod("RenderStaticPreview", new Type[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
//                if (method != null)
//                {
//                    // Invoke the method to generate the preview
//                    object ret = method.Invoke("RenderStaticPreview", new object[] { tile.tileSprite, Color.white, width, height });
//                    if (ret is Texture2D)
//                    {
//                        // Return the generated preview as a Texture2D
//                        return ret as Texture2D;
//                    }
//                }
//            }
//        }

//        // If no sprite is found or something goes wrong, return the default preview
//        return base.RenderStaticPreview(assetPath, subAssets, width, height);
//    }

//    // Utility function to get type using reflection
//    private static Type GetType(string TypeName)
//    {
//        var type = Type.GetType(TypeName);
//        if (type != null)
//            return type;

//        if (TypeName.Contains("."))
//        {
//            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
//            var assembly = Assembly.Load(assemblyName);
//            if (assembly == null)
//                return null;
//            type = assembly.GetType(TypeName);
//            if (type != null)
//                return type;
//        }

//        var currentAssembly = Assembly.GetExecutingAssembly();
//        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
//        foreach (var assemblyName in referencedAssemblies)
//        {
//            var assembly = Assembly.Load(assemblyName);
//            if (assembly != null)
//            {
//                type = assembly.GetType(TypeName);
//                if (type != null)
//                    return type;
//            }
//        }
//        return null;
//    }
//    //-------------------------------------------------------------------------------------------------------------------------------------
//    #endregion



//    #region Tile Sprite Icon
//    //-------------------------------------------------------------------------------------------------------------------------------------
//    public override void OnInspectorGUI()
//    {
//        // Get a reference to the TileData object
//        Tile_SO tile_SO = (Tile_SO)target;

//        // Check if the tileSprite exists
//        if (tile_SO.tileSprite != null)
//        {
//            // Draw a label for the sprite preview
//            GUILayout.Label("Sprite Preview", EditorStyles.boldLabel);

//            // Draw the sprite with a larger size (e.g., 128x128 pixels)
//            GUILayout.Label(AssetPreview.GetAssetPreview(tile_SO.tileSprite), GUILayout.Width(128), GUILayout.Height(128));
//        }

//        // Draw the default fields
//        DrawDefaultInspector();
//    }
//    //-------------------------------------------------------------------------------------------------------------------------------------
//    #endregion
//}